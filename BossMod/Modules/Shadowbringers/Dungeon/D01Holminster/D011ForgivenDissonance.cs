namespace BossMod.Shadowbringers.Dungeon.D01Holminster.D011ForgivenDissonance;

public enum OID : uint
{
    Boss = 0x278A, // R4.0
    Orbs = 0x2896, // R1.1
    Helper2 = 0x2A4B, // R3.45
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    ThePathOfLight = 15813, // Boss->self, 4.0s cast, range 40 circle
    BrazenBull = 15817, // Boss->self, 3.0s cast, single-target
    GibbetCage = 15816, // Boss->self, 3.0s cast, range 8 circle
    Thumbscrew = 15814, // Boss->location, 4.8s cast, width 8 rect charge
    HereticsFork = 15822, // Orbs->self, 8.0s cast, range 40 width 6 cross
    LightShot = 15819, // Orbs->self, 3.0s cast, range 40 width 4 rect
    WoodenHorse = 15815, // Boss->self, 5.0s cast, range 40 90-degree cone
    Pillory = 15812 // Boss->player, 5.0s cast, single-target
}

class Thumbscrew(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.Thumbscrew), 4);
class ThePathofLight(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ThePathOfLight));
class GibbetCage(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GibbetCage), 8);
class HereticsFork(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HereticsFork), new AOEShapeCross(40f, 3f));
class LightShot(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LightShot), new AOEShapeRect(40f, 2f));
class WoodenHorse(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WoodenHorse), new AOEShapeCone(40f, 45f.Degrees()));
class Pillory(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.Pillory));

class D011ForgivenDissonanceStates : StateMachineBuilder
{
    public D011ForgivenDissonanceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Thumbscrew>()
            .ActivateOnEnter<ThePathofLight>()
            .ActivateOnEnter<GibbetCage>()
            .ActivateOnEnter<HereticsFork>()
            .ActivateOnEnter<LightShot>()
            .ActivateOnEnter<WoodenHorse>()
            .ActivateOnEnter<Pillory>();

    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "legendoficeman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 676, NameID = 8299)]
public class D011ForgivenDissonance(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Circle(new(-15, 240), 19.5f)], [new Rectangle(new(-15, 260), 20, 1.25f), new Rectangle(new(-15, 220), 20, 1.2f)]);
}
