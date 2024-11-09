﻿namespace BossMod.Shadowbringers.Dungeon.D01Holminster.D012TesleentheForgiven;

public enum OID : uint
{
    Boss = 0x278B, // R1.8
    HolyWaterVoidzone = 0x1EABF9, // R0.5
    Helper2 = 0x2A4B,
    Helper3 = 0x2A4C, // R1.32
    Helper4 = 0x2A4D, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // 278B->player, no cast, single-target
    TheTickler = 15823, // 278B->player, 4.0s cast, single-target, tankbuster
    ScoldsBridle = 15824, // 278B->self, 4.0s cast, range 40 circle, raidwide
    FeveredFlagellation1 = 15829, // 278B->self, 8.0s cast, single-target
    FeveredFlagellation2 = 15830, // 278B->players, no cast, width 4 rect charge, limit cut mechanic
    ExorciseA = 15826, // 278B->none, 5.0s cast, single-target
    ExorciseB = 15827, // 278B->location, no cast, range 6 circle
    HolyWaterVoidzones = 15825, // 278B->self, no cast, single-target
    HolyWater = 15828 // Helper->location, 7.0s cast, range 6 circle
}

public enum IconID : uint
{
    Tankbuster = 198, // player
    Icon1 = 79, // player
    Icon2 = 80, // player
    Icon3 = 81, // player
    Icon4 = 82, // player
    Stackmarker = 62 // player
}

class TheTickler(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.TheTickler));
class ScoldsBridle(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ScoldsBridle));

class FeveredFlagellation(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect rect = new(default, 2);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (CurrentBaits.Count > 0 && (AID)spell.Action.ID == AID.FeveredFlagellation2)
            CurrentBaits.RemoveAt(0);
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if ((IconID)iconID is >= IconID.Icon1 and <= IconID.Icon4)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, rect));
    }

    public override void Update()
    {
        for (var i = 0; i < CurrentBaits.Count; ++i)
        {
            var b = CurrentBaits[i];
            CurrentBaits[i] = b with { Shape = rect with { LengthFront = (b.Target.Position - b.Source.Position).Length() } };
        }
    }
}

class Exorcise(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.ExorciseA), 6, 4, 4);
class HolyWater(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, ActionID.MakeSpell(AID.HolyWater), m => m.Enemies(OID.HolyWaterVoidzone).Where(z => z.EventState != 7), 0.8f);

class D012TesleentheForgivenStates : StateMachineBuilder
{
    public D012TesleentheForgivenStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheTickler>()
            .ActivateOnEnter<ScoldsBridle>()
            .ActivateOnEnter<FeveredFlagellation>()
            .ActivateOnEnter<Exorcise>()
            .ActivateOnEnter<HolyWater>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "legendoficeman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 676, NameID = 8300)]
public class D012TesleentheForgiven(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Circle(new(78, -82), 19.5f)], [new Rectangle(new(78, -62), 20, 1), new Rectangle(new(78, -102), 20, 1)]);
}
