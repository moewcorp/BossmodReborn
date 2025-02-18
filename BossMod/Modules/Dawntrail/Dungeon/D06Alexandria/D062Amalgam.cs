﻿namespace BossMod.Dawntrail.Dungeon.D06Alexandria.D062Amalgam;

public enum OID : uint
{
    Boss = 0x416A, // R5.075
    AddBlock = 0x416B, // R1.2
    Helper = 0x233C // R0.5
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    ElectrowaveVisual = 36337, // Boss->self, 5.0s cast, single-target
    Electrowave = 36338, // Helper->self, 5.0s cast, range 54 circle, raidwide
    Disassembly = 36323, // Boss->self, 5.0s cast, range 40 circle, raidwide

    CentralizedCurrent = 36327, // Boss->self, 5.0s cast, range 90 width 15 rect, central line AOE

    SplitCurrentVisual = 36329, // Boss->self, 5.0s cast, single-target
    SplitCurrent1 = 36331, // Helper->self, 5.0s cast, range 90 width 25 rect, side line AOE
    SplitCurrent2 = 36330, // Helper->self, 5.0s cast, range 90 width 25 rect, side line AOE

    SupercellMatrix1 = 39136, // Helper->self, 10.1s cast, triangle cone, 90-degrees, 40 range
    SupercellMatrix2 = 39138, // Helper->self, 7.8s cast, range 55 width 8 rect, portal line AOEs

    StaticSparkVisual = 36325, // Boss->self, no cast, single-target
    StaticSpark = 36334, // Helper->player, 5.0s cast, range 6 circle, Spread
    Amalgamight = 36339, // Boss->player, 5.0s cast, single-target, tankbuster
    Voltburst = 36336, // Helper->location, 4.0s cast, range 6 circle, 3x baited AOEs

    SuperboltVisual = 36332, // Boss->self, 5.0s cast, single-target
    Superbolt = 36333, // Helper->players, 5.0s cast, range 6 circle, stack

    TernaryChargeVisual = 39253, // Boss->self, 4.0s cast, single-target
    TernaryCharge1 = 39254, // Helper->location, 4.0s cast, range 10 circle
    TernaryCharge2 = 39255, // Helper->location, 6.0s cast, range 10-20 donut
    TernaryCharge3 = 39256, // Helper->location, 8.0s cast, range 20-30 donut
}

class ElectrowaveArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(D062Amalgam.ArenaCenter, 23f)], [new Square(D062Amalgam.ArenaCenter, 20f)]);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Electrowave && Arena.Bounds == D062Amalgam.StartingBounds)
            _aoe = new(square, Arena.Center, default, Module.CastFinishAt(spell, 0.5f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x27)
        {
            Arena.Bounds = D062Amalgam.DefaultBounds;
            _aoe = null;
        }
    }
}

class Electrowave(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Electrowave));
class Disassembly(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Disassembly));
class CentralizedCurrent(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CentralizedCurrent), new AOEShapeRect(90f, 7.5f));

abstract class SplitCurrent(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(90f, 12.5f));
class SplitCurrent1(BossModule module) : SplitCurrent(module, AID.SplitCurrent1);
class SplitCurrent2(BossModule module) : SplitCurrent(module, AID.SplitCurrent2);

class SupercellMatrix1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SupercellMatrix1), new AOEShapeTriCone(40f, 45.Degrees()));
class SupercellMatrix2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SupercellMatrix2), new AOEShapeRect(55f, 4f));
class StaticSpark(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.StaticSpark), 6f);
class Amalgamight(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Amalgamight));
class Voltburst(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Voltburst), 6f);
class Superbolt(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.Superbolt), 6f, 4, 4);

class TernaryCharge(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TernaryCharge1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.TernaryCharge1 => 0,
                (uint)AID.TernaryCharge2 => 1,
                (uint)AID.TernaryCharge3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

class D062AmalgamStates : StateMachineBuilder
{
    public D062AmalgamStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectrowaveArenaChange>()
            .ActivateOnEnter<Electrowave>()
            .ActivateOnEnter<Disassembly>()
            .ActivateOnEnter<CentralizedCurrent>()
            .ActivateOnEnter<SplitCurrent1>()
            .ActivateOnEnter<SplitCurrent2>()
            .ActivateOnEnter<SupercellMatrix1>()
            .ActivateOnEnter<SupercellMatrix2>()
            .ActivateOnEnter<StaticSpark>()
            .ActivateOnEnter<Amalgamight>()
            .ActivateOnEnter<Voltburst>()
            .ActivateOnEnter<Superbolt>()
            .ActivateOnEnter<TernaryCharge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS), erdelf", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 827, NameID = 12864)]
public class D062Amalgam(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(-533, -373);
    public static readonly ArenaBoundsSquare StartingBounds = new(23);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20);
}
