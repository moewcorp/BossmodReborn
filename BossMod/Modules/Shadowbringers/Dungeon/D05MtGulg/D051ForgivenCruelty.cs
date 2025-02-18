namespace BossMod.Shadowbringers.Dungeon.D05MtGulg.D051ForgivenCruelty;

public enum OID : uint
{
    Boss = 0x27CA, //R=6.89
    Helper2 = 0x27CB, //R=0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // 27CA->player, no cast, single-target

    Rake = 15611, // 27CA->player, 3.0s cast, single-target
    LumenInfinitum = 16818, // 27CA->self, 3.7s cast, range 40 width 5 rect
    TyphoonWingVisual1 = 15615, // 27CA->self, 5.0s cast, single-target
    TyphoonWingVisual2 = 15614, // 27CA->self, 5.0s cast, single-target
    TyphoonWingVisual3 = 15617, // 27CA->self, 7.0s cast, single-target
    TyphoonWingVisual4 = 15618, // 27CA->self, 7.0s cast, single-target
    TyphoonWing1 = 15616, // 233C->self, 5.0s cast, range 25 60-degree cone
    TyphoonWing2 = 17153, // 233C->self, 7.0s cast, range 25 60-degree cone
    CycloneWingVisual = 15612, // 27CA->self, 3.0s cast, single-target
    CycloneWing = 15613, // 233C->self, 4.0s cast, range 40 circle
    HurricaneWing = 15619 // 233C->self, 5.0s cast, range 10 circle
}

class Rake(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.Rake));
class CycloneWing(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.CycloneWing));
class LumenInfinitum(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LumenInfinitum), new AOEShapeRect(40, 2.5f));
class HurricaneWing(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HurricaneWing), 10);

class TyphoonWing(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(25, 30.Degrees()));
class TyphoonWing1(BossModule module) : TyphoonWing(module, AID.TyphoonWing1);
class TyphoonWing2(BossModule module) : TyphoonWing(module, AID.TyphoonWing2);

class D051ForgivenCrueltyStates : StateMachineBuilder
{
    public D051ForgivenCrueltyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Rake>()
            .ActivateOnEnter<HurricaneWing>()
            .ActivateOnEnter<TyphoonWing1>()
            .ActivateOnEnter<TyphoonWing2>()
            .ActivateOnEnter<CycloneWing>()
            .ActivateOnEnter<LumenInfinitum>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 659, NameID = 8260)]
public class D051ForgivenCruelty(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Circle(new(188, -170), 19.5f)], [new Rectangle(new(168.25f, -170), 20, 1, 90.Degrees()), new Rectangle(new(207.75f, -170), 20, 1, 90.Degrees())]);
}
