﻿namespace BossMod;

// utility that recalculates ai hints based on different data sources (eg active bossmodule, etc)
// when there is no active bossmodule (eg in outdoor or on trash), we try to guess things based on world state (eg actor casts)
public sealed class AIHintsBuilder : IDisposable
{
    private const float RaidwideSize = 30;
    private const float HalfWidth = 0.5f;
    public readonly Pathfinding.ObstacleMapManager Obstacles;
    private readonly WorldState _ws;
    private readonly BossModuleManager _bmm;
    private readonly ZoneModuleManager _zmm;
    private readonly EventSubscriptions _subscriptions;
    private readonly Dictionary<ulong, (Actor Caster, Actor? Target, AOEShape Shape)> _activeAOEs = [];
    private ArenaBoundsCircle? _activeFateBounds;
    private static readonly HashSet<uint> ignore = [27503, 33626]; // action IDs that the AI should ignore
    private static readonly PartyRolesConfig _config = Service.Config.Get<PartyRolesConfig>();
    private static readonly Dictionary<uint, (byte, byte, byte, uint, string, string, string, int, bool)> _spellCache = [];

    public AIHintsBuilder(WorldState ws, BossModuleManager bmm, ZoneModuleManager zmm)
    {
        _ws = ws;
        _bmm = bmm;
        _zmm = zmm;
        Obstacles = new(ws);
        _subscriptions = new
        (
            ws.Actors.CastStarted.Subscribe(OnCastStarted),
            ws.Actors.CastFinished.Subscribe(OnCastFinished),
            ws.Client.ActiveFateChanged.Subscribe(_ => _activeFateBounds = null)
        );
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
        Obstacles.Dispose();
    }

    public void Update(AIHints hints, int playerSlot, bool moveImminent)
    {
        var player = _ws.Party[playerSlot];

        hints.Clear();
        if (moveImminent || player?.PendingKnockbacks.Count > 0)
            hints.MaxCastTime = 0;
        if (player != null)
        {
            var playerAssignment = _config[_ws.Party.Members[playerSlot].ContentId];
            var activeModule = _bmm.ActiveModule?.StateMachine.ActivePhase != null ? _bmm.ActiveModule : null;
            FillEnemies(hints, playerAssignment == PartyRolesConfig.Assignment.MT || playerAssignment == PartyRolesConfig.Assignment.OT && !_ws.Party.WithoutSlot(false, false, true).Any(p => p != player && p.Role == Role.Tank));
            if (activeModule != null)
            {
                activeModule.CalculateAIHints(playerSlot, ref player, ref playerAssignment, ref hints);
            }
            else
            {
                CalculateAutoHints(hints, player);
                _zmm.ActiveModule?.CalculateAIHints(playerSlot, player, hints);
            }
        }
        hints.Normalize();
    }

    // Fill list of potential targets from world state
    private void FillEnemies(AIHints hints, bool playerIsDefaultTank)
    {
        var allowedFateID = Utils.IsPlayerSyncedToFate(_ws) ? _ws.Client.ActiveFate.ID : 0;

        foreach (var actor in _ws.Actors.Actors.Values)
        {
            if (!actor.IsTargetable || actor.IsAlly || actor.IsDead)
                continue;
            var index = actor.CharacterSpawnIndex;
            if (index < 0 || index >= hints.Enemies.Length)
                continue;

            int priority;
            if (actor.FateID != 0)
            {
                if (actor.FateID != allowedFateID)
                    priority = AIHints.Enemy.PriorityInvincible; // Fate mob in an irrelevant fate
                else
                    priority = 0; // Relevant fate mob
            }
            else if (actor.PredictedDead)
                priority = AIHints.Enemy.PriorityPointless; // Mob is about to die
            else if (actor.AggroPlayer)
                priority = 0; // Aggroed player
            else if (actor.InCombat && _ws.Party.FindSlot(actor.TargetID) >= 0)
                priority = 0; // Assisting party members
            else
                priority = AIHints.Enemy.PriorityUndesirable; // Default undesirable

            var enemy = hints.Enemies[index] = new(actor, priority, playerIsDefaultTank);
            hints.PotentialTargets.Add(enemy);
        }
    }

    private void CalculateAutoHints(AIHints hints, Actor player)
    {
        var inFate = Utils.IsPlayerSyncedToFate(_ws);
        var center = inFate ? _ws.Client.ActiveFate.Center : player.PosRot.XYZ();
        var (e, bitmap) = Obstacles.Find(center);
        var resolution = bitmap?.PixelSize ?? 0.5f;
        if (inFate)
        {
            hints.PathfindMapCenter = new(_ws.Client.ActiveFate.Center.XZ());
            hints.PathfindMapBounds = (_activeFateBounds ??= new ArenaBoundsCircle(_ws.Client.ActiveFate.Radius, resolution));
            if (e != null && bitmap != null)
            {
                var originCell = (hints.PathfindMapCenter - e.Origin) / resolution;
                var originX = (int)originCell.X;
                var originZ = (int)originCell.Z;
                var halfSize = (int)(_ws.Client.ActiveFate.Radius / resolution);
                hints.PathfindMapObstacles = new(bitmap, new(originX - halfSize, originZ - halfSize, originX + halfSize, originZ + halfSize));
            }
        }
        else if (e != null && bitmap != null)
        {
            var originCell = (player.Position - e.Origin) / resolution;
            var originX = (int)originCell.X;
            var originZ = (int)originCell.Z;
            // if player is too close to the border, adjust origin
            originX = Math.Min(originX, bitmap.Width - e.ViewWidth);
            originZ = Math.Min(originZ, bitmap.Height - e.ViewHeight);
            originX = Math.Max(originX, e.ViewWidth);
            originZ = Math.Max(originZ, e.ViewHeight);
            // TODO: consider quantizing even more, to reduce jittering when player moves?..
            hints.PathfindMapCenter = e.Origin + resolution * new WDir(originX, originZ);
            hints.PathfindMapBounds = new ArenaBoundsRect(e.ViewWidth * resolution, e.ViewHeight * resolution, MapResolution: resolution); // note: we don't bother caching these bounds, they are very lightweight
            hints.PathfindMapObstacles = new(bitmap, new(originX - e.ViewWidth, originZ - e.ViewHeight, originX + e.ViewWidth, originZ + e.ViewHeight));
        }
        else
        {
            hints.PathfindMapCenter = player.Position.Rounded(5);
            // try to keep player near grid center
            var playerOffset = player.Position - hints.PathfindMapCenter;
            if (playerOffset.X < -1.25f)
                hints.PathfindMapCenter.X -= 2.5f;
            else if (playerOffset.X > 1.25f)
                hints.PathfindMapCenter.X += 2.5f;
            if (playerOffset.Z < -1.25f)
                hints.PathfindMapCenter.Z -= 2.5f;
            else if (playerOffset.Z > 1.25f)
                hints.PathfindMapCenter.Z += 2.5f;
            // keep default bounds
        }

        foreach (var aoe in _activeAOEs.Values)
        {
            var target = aoe.Caster.CastInfo!.LocXZ;
            var rot = aoe.Caster.CastInfo!.Rotation;
            var finishAt = _ws.FutureTime(aoe.Caster.CastInfo.NPCRemainingTime);
            hints.AddForbiddenZone(aoe.Shape, target, rot, finishAt);
        }
    }

    private void OnCastStarted(Actor actor)
    {
        if (_bmm.ActiveModule?.StateMachine.ActivePhase != null) // no need to do all of this if it won't be used anyway
            return;
        if (actor.Type is not ActorType.Enemy and not ActorType.Helper || actor.IsAlly)
            return;
        var actionID = actor.CastInfo!.Action.ID;
        if (ignore.Contains(actionID))
            return;
        var spell = actor.CastInfo!.IsSpell();
        if (!spell)
            return;
        var data = GetSpellData(actionID);
        if (data.CastType == 1)
            return;
        if (data.CastType is 2 or 5 && data.EffectRange >= RaidwideSize)
            return;
        AOEShape? shape = data.CastType switch
        {
            2 => new AOEShapeCircle(data.EffectRange), // used for some point-blank aoes and enemy location-targeted - does not add caster hitbox
            3 => new AOEShapeCone(data.EffectRange + actor.HitboxRadius, DetermineConeAngle(data) * HalfWidth),
            4 => new AOEShapeRect(data.EffectRange + actor.HitboxRadius, data.XAxisModifier * HalfWidth),
            5 => new AOEShapeCircle(data.EffectRange + actor.HitboxRadius),
            //6 => custom shapes
            //7 => new AOEShapeCircle(data.EffectRange), - used for player ground-targeted circles a-la asylum
            8 => new AOEShapeRect((actor.CastInfo!.LocXZ - actor.Position).Length(), data.XAxisModifier * HalfWidth),
            10 => new AOEShapeDonut(3, data.EffectRange),
            11 => new AOEShapeCross(data.EffectRange, data.XAxisModifier * HalfWidth),
            12 => new AOEShapeRect(data.EffectRange, data.XAxisModifier * HalfWidth),
            13 => new AOEShapeCone(data.EffectRange, DetermineConeAngle(data) * HalfWidth),
            _ => null
        };
        if (shape == null)
        {
            Service.Log($"[AutoHints] Unknown cast type {data.CastType} for {actor.CastInfo.Action}");
            return;
        }
        var target = _ws.Actors.Find(actor.CastInfo.TargetID);
        _activeAOEs[actor.InstanceID] = (actor, target, shape);
    }

    private void OnCastFinished(Actor actor) => _activeAOEs.Remove(actor.InstanceID);

    private static Angle DetermineConeAngle((byte, byte, byte, uint RowId, string Name, string PathAlly, string Path, int Pos, bool Omen) data)
    {
        if (!data.Omen)
        {
            Service.Log($"[AutoHints] No omen data for {data.RowId} '{data.Name}'...");
            return 180.Degrees();
        }
        var path = data.Path;
        var pos = data.Pos;
        if (pos < 0 || pos + 6 > path.Length || !int.TryParse(path.AsSpan(pos + 3, 3), out var angle))
        {
            Service.Log($"[AutoHints] Can't determine angle from omen ({path}/{data.PathAlly}) for {data.RowId} '{data.Name}'...");
            return 180.Degrees();
        }
        return angle.Degrees();
    }

    private static (byte CastType, byte EffectRange, byte XAxisModifier, uint RowId, string Name, string PathAlly, string path, int pos, bool Omen) GetSpellData(uint actionID)
    {
        if (_spellCache.TryGetValue(actionID, out var actionRow))
            return actionRow;
        var row = Service.LuminaRow<Lumina.Excel.Sheets.Action>(actionID);
        (byte, byte, byte, uint, string, string, string, int, bool)? data;
        var omenPath = row!.Value.Omen.Value.Path.ToString();
        data = (row.Value.CastType, row.Value.EffectRange, row.Value.XAxisModifier, row.Value.RowId, row.Value.Name.ToString(), row.Value.Omen.Value.PathAlly.ToString(), omenPath, omenPath.IndexOf("fan", StringComparison.Ordinal), row.Value.Omen.ValueNullable != null);
        return _spellCache[actionID] = data!.Value;
    }
}
