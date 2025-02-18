﻿namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

class ChasmOfVollokFangSmall(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.ChasmOfVollokFangSmallAOE))
{
    public readonly List<AOEInstance> AOEs = [];
    private static readonly float platformOffset = 30 / MathF.Sqrt(2);
    private static readonly AOEShapeRect _shape = new(5, 2.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ChasmOfVollokFangSmall)
        {
            // the visual cast happens on one of the side platforms at intercardinals, offset by 30
            var pos = spell.LocXZ;
            var offset = new WDir(pos.X > Arena.Center.X ? -platformOffset : +platformOffset, pos.Z > Arena.Center.Z ? -platformOffset : +platformOffset);
            AOEs.Add(new(_shape, pos + offset, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }
}

// note: we can start showing aoes earlier, right when fang actors spawn
class ChasmOfVollokFangLarge(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.ChasmOfVollokFangLargeAOE))
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeRect _shape = new(10, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.VollokLargeAOE)
        {
            AOEs.Add(new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            var pos = spell.LocXZ;
            var mainOffset = Trial.T02ZoraalJa.ZoraalJa.ArenaCenter - Arena.Center;
            var fangOffset = pos - Arena.Center;
            var mirrorOffset = fangOffset.Dot(mainOffset) > 0 ? -2 * mainOffset : 2 * mainOffset;
            AOEs.Add(new(_shape, pos + mirrorOffset, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }
}

class ChasmOfVollokPlayer(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.ChasmOfVollokPlayer), "GTFO from occupied cell!")
{
    public bool Active;
    private readonly List<Actor> _targets = [];
    private DateTime _activation;

    private static readonly AOEShapeRect _shape = new(2.5f, 2.5f, 2.5f);
    private static readonly WDir _localX = (-135).Degrees().ToDirection();
    private static readonly WDir _localZ = 135.Degrees().ToDirection();

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Active)
            yield break;
        var platformOffset = 2 * (Arena.Center - Trial.T02ZoraalJa.ZoraalJa.ArenaCenter);
        foreach (var t in _targets.Exclude(actor))
        {
            var playerOffset = t.Position - Trial.T02ZoraalJa.ZoraalJa.ArenaCenter;
            var playerX = _localX.Dot(playerOffset);
            var playerZ = _localZ.Dot(playerOffset);
            if (Math.Abs(playerX) >= 15 || Math.Abs(playerZ) >= 15)
            {
                playerOffset -= platformOffset;
                playerX = _localX.Dot(playerOffset);
                playerZ = _localZ.Dot(playerOffset);
            }
            var cellX = CoordinateToCell(playerX);
            var cellZ = CoordinateToCell(playerZ);
            var cellCenter = Trial.T02ZoraalJa.ZoraalJa.ArenaCenter + _localX * CellCenterCoordinate(cellX) + _localZ * CellCenterCoordinate(cellZ);

            yield return new(_shape, cellCenter, 45.Degrees(), _activation);
            if (platformOffset != default)
                yield return new(_shape, cellCenter + platformOffset, 45.Degrees(), _activation);
        }
    }

    public override void Update()
    {
        // assume that if player dies, he won't participate in the mechanic
        _targets.RemoveAll(t => t.IsDead);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => PlayerPriority.Normal;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.ChasmOfVollok)
        {
            _targets.Add(actor);
            _activation = WorldState.FutureTime(6.1f);
        }
    }

    private static int CoordinateToCell(float x) => x switch
    {
        < -5 => 0,
        < 0 => 1,
        < 5 => 2,
        _ => 3
    };

    private static float CellCenterCoordinate(int c) => -7.5f + c * 5;
}
