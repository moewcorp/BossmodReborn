namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouSatyros;

public enum OID : uint
{
    Boss = 0x3D2D, //R=7.5
    GymnasiouElaphos = 0x3D2E, //R=2.08
    StormsGrip = 0x3D2F, //R=1.0
    GymnasiouLyssa = 0x3D4E, //R=3.75
    GymnasiouLampas = 0x3D4D, //R=2.001
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 872, // Boss/GymnasiouElaphos->player, no cast, single-target
    AutoAttack2 = 870, // GymnasiouLyssa->player, no cast, single-target

    StormWingVisual1 = 32220, // Boss->self, 5.0s cast, single-target
    StormWingVisual2 = 32219, // Boss->self, 5.0s cast, single-target
    StormWing = 32221, // Helper->self, 5.0s cast, range 40 90-degree cone
    DreadDive = 32218, // Boss->player, 5.0s cast, single-target
    FlashGale = 32222, // Boss->location, 3.0s cast, range 6 circle
    StormsGripActivate = 32199, // StormsGrip->self, no cast, single-target
    WindCutter = 32227, // StormsGrip->self, no cast, range 4 circle
    BigHorn = 32226, // GymnasiouElaphos->player, no cast, single-target
    WingblowVisual = 32224, // Boss->self, 4.0s cast, single-target
    Wingblow = 32225, // Helper->self, 4.0s cast, range 15 circle

    HeavySmash = 32317, // GymnasiouLyssa->location, 3.0s cast, range 6 circle
    Telega = 9630 // GymnasiouLyssa->self, no cast, single-target, bonus add disappear
}

class StormWing(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.StormWing), new AOEShapeCone(40, 45.Degrees()));
class FlashGale(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.FlashGale), 6);
class WindCutter(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID.StormsGrip));
class Wingblow(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Wingblow), new AOEShapeCircle(15));
class DreadDive(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.DreadDive));

class HeavySmash(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HeavySmash), 6);

class GymnasiouSatyrosStates : StateMachineBuilder
{
    public GymnasiouSatyrosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StormWing>()
            .ActivateOnEnter<FlashGale>()
            .ActivateOnEnter<WindCutter>()
            .ActivateOnEnter<Wingblow>()
            .ActivateOnEnter<DreadDive>()
            .ActivateOnEnter<HeavySmash>()
            .Raw.Update = () => module.Enemies(GymnasiouSatyros.All).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12003)]
public class GymnasiouSatyros(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GymnasiouLampas, (uint)OID.GymnasiouLyssa];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.GymnasiouElaphos, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.GymnasiouElaphos));
        Arena.Actors(Enemies(bonusAdds), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.GymnasiouLampas => 3,
                OID.GymnasiouLyssa => 2,
                OID.GymnasiouElaphos => 1,
                _ => 0
            };
        }
    }
}
