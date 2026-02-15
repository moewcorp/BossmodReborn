#pragma warning disable IDE0028
namespace BossMod.Dawntrail.Savage.M12S2Lindwurm;

sealed class IdyllicDreamStaging(BossModule module) : StagingAssignment<Replication3Role>(module, playerGroupSize: 4, cloneGroupSize: 2, hasBossTether: false)
{
    readonly M12S2LindwurmConfig _config = Service.Config.Get<M12S2LindwurmConfig>();

    public bool WurmsFinished;

    protected override Replication3Role DetermineCloneRole(WurmClone w)
    {
        var clock = Clockspot.GetClosest(w.Position);
        var spread = w.Shape == CloneShape.Spread;

        switch (clock)
        {
            case Clockspot.N:
            case Clockspot.NE:
                return spread ? Replication3Role.Defam1 : Replication3Role.Stack1;

            case Clockspot.E:
            case Clockspot.SE:
                return spread ? Replication3Role.Defam2 : Replication3Role.Stack2;

            case Clockspot.S:
            case Clockspot.SW:
                return spread ? Replication3Role.Defam3 : Replication3Role.Stack3;

            case Clockspot.W:
            case Clockspot.NW:
                return spread ? Replication3Role.Defam4 : Replication3Role.Stack4;

            default:
                ReportError($"Unexpected clockspot {clock} for clone {w.Actor}");
                return spread ? Replication3Role.Defam1 : Replication3Role.Stack1;
        }
    }

    protected override IEnumerable<string> GetHelpHints(int slot, Actor actor)
    {
        if (WurmsFinished)
            return [];

        var wurm = WurmsBySlot[slot];
        if (wurm != null)
        {
            var group = wurm.Position.Deg > 0 ? "N" : "S";
            return [$"Clone: {wurm.Shape}, order {wurm.SpawnOrder + 1}, group {group}"];
        }

        return base.GetHelpHints(slot, actor);
    }

    /*protected override Replication3Role? DeterminePlayerRole(PlayerClone c)
    {
        if (!_config.Rep3Assignments.IsValid())
            return null;

        var clock = Clockspot.GetClosest(c.Position);
        return _config.Rep3Assignments[clock];
    }*/
    protected override Replication3Role? DeterminePlayerRole(PlayerClone c)
    {
        var eff = _config.GetReplication3();

        var clock = Clockspot.GetClosest(c.Position);
        return eff.GetRole(clock);
    }
}

/*
class IdyllicDreamStaging(BossModule module) : StagingAssignment<Replication3Role>(module, playerGroupSize: 4, cloneGroupSize: 2, hasBossTether: false)
{
    readonly RM12S2TheLindwurmConfig _config = Service.Config.Get<RM12S2TheLindwurmConfig>();

    public bool WurmsFinished;

    protected override Replication3Role DetermineCloneRole(WurmClone w)
    {
        return Clockspot.GetClosest(w.Position) switch
        {
            Clockspot.N or Clockspot.NE => w.Shape == CloneShape.Spread ? Replication3Role.Defam1 : Replication3Role.Stack1,
            Clockspot.E or Clockspot.SE => w.Shape == CloneShape.Spread ? Replication3Role.Defam2 : Replication3Role.Stack2,
            Clockspot.S or Clockspot.SW => w.Shape == CloneShape.Spread ? Replication3Role.Defam3 : Replication3Role.Stack3,
            Clockspot.W or Clockspot.NW => w.Shape == CloneShape.Spread ? Replication3Role.Defam4 : Replication3Role.Stack4,
            _ => throw new NotImplementedException()
        };
    }

    protected override IEnumerable<string> GetHelpHints(int slot, Actor actor)
    {
        if (WurmsFinished)
            yield break;

        if (WurmsBySlot[slot] is { } w)
        {
            yield return $"Clone: {w.Shape}, order {w.SpawnOrder + 1}, group {(w.Position.Deg > 0 ? "N" : "S")}";
        }
        else
            foreach (var h in base.GetHelpHints(slot, actor))
                yield return h;
    }

    protected override Replication3Role? DeterminePlayerRole(PlayerClone c)
    {
        if (!_config.Rep3Assignments.IsValid())
            return null;

        return _config.Rep3Assignments[Clockspot.GetClosest(c.Position)];
    }
}
*/
sealed class IdyllicDreamPowerGusherSnakingKick(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = new();
    readonly List<AOEInstance> _portaled = new();

    public bool Visible;
    public bool Risky;
    public bool WatchTeleport;

    public void Reset() => NumCasts = 0;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Visible || _predicted.Count == 0)
            return default;

        if (Risky)
        {
            var span = CollectionsMarshal.AsSpan(_predicted);
            for (var i = 0; i < span.Length; ++i)
            {
                if (!span[i].Risky)
                    span[i] = span[i] with { Risky = true };
            }
        }
        return CollectionsMarshal.AsSpan(_predicted);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.PowerGusherAOEVisual => new AOEShapeCone(60, 45.Degrees()),
            AID.SnakingKickCastFirst => new AOEShapeCircle(10),
            _ => null
        };

        if (shape != null)
        {
            _predicted.Add(new(
                shape,
                spell.LocXZ,
                spell.Rotation,
                WorldState.FutureTime(29.3f)
            ));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var aid = (AID)spell.Action.ID;

        if (aid is AID.PowerGusher or AID.IdyllicDreamSnakingKick)
        {
            NumCasts++;
            Visible = false;

            if (WatchTeleport)
                _predicted.Clear();
        }

        if (!WatchTeleport)
            return;

        if (aid == AID.TemporalTearEnter)
        {
            var pos = caster.Position;

            for (var i = _predicted.Count - 1; i >= 0; --i)
            {
                if (_predicted[i].Origin.AlmostEqual(pos, 1))
                {
                    _portaled.Add(_predicted[i]);
                    _predicted.RemoveAt(i);
                }
            }
        }
        else if (aid == AID.TemporalTearExit)
        {
            var newPos = spell.TargetXZ;

            var span = CollectionsMarshal.AsSpan(_portaled);
            for (var i = 0; i < span.Length; ++i)
                _predicted.Add(span[i] with { Origin = newPos });

            _portaled.Clear();
        }
        else if (aid == AID.Dash)
        {
            var oldPos = caster.Position;
            var newPos = spell.TargetXZ;

            var span = CollectionsMarshal.AsSpan(_predicted);
            for (var i = 0; i < span.Length; ++i)
            {
                if (span[i].Origin.AlmostEqual(oldPos, 1))
                    span[i] = span[i] with { Origin = newPos };
            }
        }
    }
}
sealed class IdyllicDreamWurmStackSpread : Components.UniformStackSpread
{
    public readonly List<(Actor Target, CloneShape Shape, DateTime Activation)> Stored = new();
    public int NumCasts;
    const float SpreadDelay = 1.2f;

    public IdyllicDreamWurmStackSpread(BossModule module) : base(module, 5f, 20f, 3)
    {
        var staging = module.FindComponent<IdyllicDreamStaging>();
        if (staging == null)
        {
            ReportError("IdyllicDreamWurmStackSpread: staging component not found");
            return;
        }

        var firstCast = module.WorldState.FutureTime(10.5f);

        // Copy to avoid mutating staging state
        var clones = new List<IdyllicDreamStaging.WurmClone>(staging.WurmClones);
        clones.Sort((a, b) => a.SpawnOrder.CompareTo(b.SpawnOrder));

        var count = clones.Count;
        for (var i = 0; i < count; ++i)
        {
            var clone = clones[i];

            if (clone.Target == null || clone.Shape == null)
            {
                ReportError($"{clone.Actor} is invalid, this should never happen");
                continue;
            }

            var baseDelay = clone.SpawnOrder * 5f;
            var extraDelay = clone.Shape == CloneShape.Spread ? SpreadDelay : 0f;

            Stored.Add((
                clone.Target,
                clone.Shape.Value,
                firstCast.AddSeconds(baseDelay + extraDelay)
            ));
        }
    }

    // Suppress default UniformStackSpread hints
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
    }

    public override void Update()
    {
        Stacks.Clear();
        Spreads.Clear();

        var count = Stored.Count;
        if (count == 0)
            return;

        var limit = count > 2 ? 2 : count;

        for (var i = 0; i < limit; ++i)
        {
            var entry = Stored[i];

            if (entry.Shape == CloneShape.Spread)
                AddSpread(entry.Target, entry.Activation);
            else
                AddStack(entry.Target, entry.Activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var aid = (AID)spell.Action.ID;

        if (aid is AID.IdyllicDreamManaBurstVisual or AID.IdyllicDreamHeavySlam)
            NumCasts++;

        if (aid is AID.IdyllicDreamManaBurst or AID.IdyllicDreamHeavySlam)
        {
            if (Stored.Count > 0)
                Stored.RemoveAt(0);
        }
    }
}
sealed class IdyllicDreamManaBurstPlayer(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];

    public bool Risky;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _predicted.Count;
        if (count == 0)
            return default;

        if (Risky)
        {
            var span = CollectionsMarshal.AsSpan(_predicted);
            for (var i = 0; i < span.Length; ++i)
            {
                if (!span[i].Risky)
                    span[i] = span[i] with { Risky = true };
            }
        }

        return CollectionsMarshal.AsSpan(_predicted);
    }

    public void Predict(int spawnOrder)
    {
        var staging = Module.FindComponent<IdyllicDreamStaging>();
        if (staging == null)
        {
            ReportError("IdyllicDreamManaBurstPlayer: staging component not found");
            return;
        }

        var party = Module.Raid.WithSlot(includeDead: true);
        var count = party.Length;

        for (var i = 0; i < count; ++i)
        {
            ref var entry = ref party[i];
            var slot = entry.Item1;
            var player = entry.Item2;

            var playerClone = staging.PlayersBySlot[slot];
            if (playerClone == null)
            {
                ReportError($"Player {player} has no assigned clone");
                continue;
            }

            var wurmClone = staging.WurmsBySlot[slot];
            if (wurmClone == null)
            {
                ReportError($"Player {player} has no clone tether");
                continue;
            }

            if (playerClone.SpawnOrder != spawnOrder)
                continue;

            if (wurmClone.Shape != CloneShape.Spread)
                continue;

            _predicted.Add(new AOEInstance(
                new AOEShapeCircle(20f),
                playerClone.Actor.Position,
                default,
                Module.WorldState.FutureTime(10f) // TODO: correct activation timing
            ));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.ManaBurstReplay)
            return;

        var target = Module.WorldState.Actors.Find(spell.MainTargetID);
        if (target == null)
            return;

        var pos = target.Position;

        for (var i = _predicted.Count - 1; i >= 0; --i)
        {
            if (_predicted[i].Origin.AlmostEqual(pos, 1f))
                _predicted.RemoveAt(i);
        }

        NumCasts++;
    }
}
sealed class IdyllicDreamHeavySlamPlayer(BossModule module) : Components.GenericTowers(module)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        // intentionally suppress default tower hints
    }

    public void Predict(int spawnOrder)
    {
        var staging = Module.FindComponent<IdyllicDreamStaging>();
        if (staging == null)
        {
            ReportError("IdyllicDreamHeavySlamPlayer: staging component not found");
            return;
        }

        var party = Module.Raid.WithSlot(includeDead: true);
        var count = party.Length;

        for (var i = 0; i < count; ++i)
        {
            ref var entry = ref party[i];
            var slot = entry.Item1;
            var player = entry.Item2;

            var playerClone = staging.PlayersBySlot[slot];
            if (playerClone == null)
            {
                ReportError($"Player {player} has no assigned clone");
                continue;
            }

            var wurmClone = staging.WurmsBySlot[slot];
            if (wurmClone == null)
            {
                ReportError($"Player {player} has no clone tether");
                continue;
            }

            if (playerClone.SpawnOrder != spawnOrder)
                continue;

            if (wurmClone.Shape != CloneShape.Stack)
                continue;

            Towers.Add(new(
                playerClone.Actor.Position,
                5f,
                2,
                6,
                activation: Module.WorldState.FutureTime(10f) // TODO: correct timing
            ));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.HeavySlamReplay)
            return;

        var pos = caster.Position;

        for (var i = Towers.Count - 1; i >= 0; --i)
        {
            if (Towers[i].Position.AlmostEqual(pos, 1f))
                Towers.RemoveAt(i);
        }

        NumCasts++;
    }
}
class IdyllicDreamPlayerCastCounter(BossModule module) : Components.CastCounterMulti(module, [(uint)AID.ManaBurstReplay, (uint)AID.HeavySlamReplay]);

/*
class IdyllicDreamPowerGusherSnakingKick(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];
    readonly List<AOEInstance> _portaled = [];

    public bool Visible;
    public bool Risky;

    public bool WatchTeleport;

    public void Reset()
    {
        NumCasts = 0;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Visible)
            yield break;

        foreach (var p in _predicted)
            yield return p with { Risky = Risky };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.PowerGusherAOEVisual => new AOEShapeCone(60, 45.Degrees()),
            AID.SnakingKickCastFirst => new AOEShapeCircle(10),
            _ => null
        };
        if (shape != null)
            _predicted.Add(new(shape, spell.LocXZ, spell.Rotation, WorldState.FutureTime(29.3f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PowerGusher or AID.IdyllicDreamSnakingKick)
        {
            NumCasts++;
            Visible = false;

            if (WatchTeleport)
                _predicted.Clear();
        }

        if (WatchTeleport)
        {
            if ((AID)spell.Action.ID == AID.TemporalTearEnter)
            {
                _portaled.AddRange(_predicted.Where(p => p.Origin.AlmostEqual(caster.Position, 1)));
                _predicted.RemoveAll(p => p.Origin.AlmostEqual(caster.Position, 1));
            }

            if ((AID)spell.Action.ID == AID.TemporalTearExit)
            {
                _predicted.AddRange(_portaled.Select(p => p with { Origin = spell.TargetXZ }));
                _portaled.Clear();
            }

            if ((AID)spell.Action.ID == AID.Dash)
            {
                for (var i = 0; i < _predicted.Count; i++)
                {
                    if (_predicted[i].Origin.AlmostEqual(caster.Position, 1))
                        _predicted.Ref(i).Origin = spell.TargetXZ;
                }
            }
        }
    }
}
*/
/*
class IdyllicDreamWurmStackSpread : Components.UniformStackSpread
{
    public readonly List<(Actor Target, CloneShape Shape, DateTime Activation)> Stored = [];

    public int NumCasts;

    const float SpreadDelay = 1.2f;

    public IdyllicDreamWurmStackSpread(BossModule module) : base(module, 5, 20, 3, alwaysShowSpreads: true)
    {
        EnableHints = false;

        var firstCast = WorldState.FutureTime(10.5f);

        foreach (var clone in Module.FindComponent<IdyllicDreamStaging>()!.WurmClones.OrderBy(c => c.SpawnOrder))
        {
            if (clone.Target == null || clone.Shape == null)
            {
                ReportError($"{clone.Actor} is invalid, this should never happen");
                continue;
            }
            Stored.Add((clone.Target, clone.Shape.Value, firstCast.AddSeconds(clone.SpawnOrder * 5 + clone.Shape == CloneShape.Spread ? SpreadDelay : 0)));
        }
    }

    public override void Update()
    {
        Stacks.Clear();
        Spreads.Clear();

        foreach (var (t, sh, a) in Stored.Take(2))
        {
            if (sh == CloneShape.Spread)
                AddSpread(t, a);
            else
                AddStack(t, a);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.IdyllicDreamManaBurstVisual or AID.IdyllicDreamHeavySlam)
        {
            NumCasts++;
        }

        if ((AID)spell.Action.ID is AID.IdyllicDreamManaBurst or AID.IdyllicDreamHeavySlam)
        {
            if (Stored.Count > 0)
                Stored.RemoveAt(0);
        }
    }
}
*/
/*
sealed class IdyllicDreamManaBurstPlayer(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEInstance[] _predicted = new AOEInstance[8];
    private int _predictedCount;

    public bool Risky;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_predictedCount == 0)
            return default;

        for (var i = 0; i < _predictedCount; ++i)
            _predicted[i].Risky = Risky;

        return _predicted.AsSpan(0, _predictedCount);
    }

    public void Predict(int spawnOrder)
    {
        var stg = Module.FindComponent<IdyllicDreamStaging>();
        if (stg == null)
            return;

        var raid = Raid.WithSlot(includeDead: true);
        var len = raid.Length;

        for (var i = 0; i < len; ++i)
        {
            var (slot, player) = raid[i];

            var pc = stg.PlayersBySlot[slot];
            if (pc == null)
            {
                ReportError($"Player {player} has no assigned clone");
                continue;
            }

            var pw = stg.WurmsBySlot[slot];
            if (pw == null)
            {
                ReportError($"Player {player} has no clone tether");
                continue;
            }

            if (pc.SpawnOrder != spawnOrder || pw.Shape != CloneShape.Spread)
                continue;

            _predicted[_predictedCount++] =
                new(new AOEShapeCircle(20),
                    pc.Actor.Position,
                    default,
                    WorldState.FutureTime(10));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.ManaBurstReplay)
            return;

        var target = WorldState.Actors.Find(spell.MainTargetID);
        if (target != null)
        {
            var write = 0;
            for (var i = 0; i < _predictedCount; ++i)
            {
                if (!_predicted[i].Origin.AlmostEqual(target.Position, 1))
                    _predicted[write++] = _predicted[i];
            }
            _predictedCount = write;
        }

        ++NumCasts;
    }
}
*/

/*
class IdyllicDreamManaBurstPlayer(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];

    public bool Risky;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Select(p => p with { Risky = Risky });

    public void Predict(int spawnOrder)
    {
        var stg = Module.FindComponent<IdyllicDreamStaging>()!;

        foreach (var (i, player) in Raid.WithSlot(includeDead: true))
        {
            if (stg.PlayersBySlot[i] is not { } pc)
            {
                ReportError($"Player {player} has no assigned clone");
                continue;
            }
            if (stg.WurmsBySlot[i] is not { } pw)
            {
                ReportError($"Player {player} has no clone tether");
                continue;
            }

            if (pc.SpawnOrder != spawnOrder || pw.Shape != CloneShape.Spread)
                continue;

            // TODO: fix activation time
            _predicted.Add(new(new AOEShapeCircle(20), pc.Actor.Position, default, WorldState.FutureTime(10)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ManaBurstReplay)
        {
            var targetpos = WorldState.Actors.Find(spell.MainTargetID)?.Position ?? default;
            _predicted.RemoveAll(p => p.Origin.AlmostEqual(targetpos, 1));
            NumCasts++;
        }
    }
}
*/
/*
class IdyllicDreamHeavySlamPlayer : Components.GenericTowers
{
    public IdyllicDreamHeavySlamPlayer(BossModule module) : base(module)
    {
        EnableHints = false;
    }

    public void Predict(int spawnOrder)
    {
        var stg = Module.FindComponent<IdyllicDreamStaging>()!;

        foreach (var (i, player) in Raid.WithSlot(includeDead: true))
        {
            if (stg.PlayersBySlot[i] is not { } pc)
            {
                ReportError($"Player {player} has no assigned clone");
                continue;
            }
            if (stg.WurmsBySlot[i] is not { } pw)
            {
                ReportError($"Player {player} has no clone tether");
                continue;
            }

            if (pc.SpawnOrder != spawnOrder || pw.Shape != CloneShape.Stack)
                continue;

            // TODO: fix activation time
            Towers.Add(new(pc.Actor.Position, 5, 2, 6, activation: WorldState.FutureTime(10)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.HeavySlamReplay)
        {
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
            NumCasts++;
        }
    }
}
*/