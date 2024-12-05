namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouMeganereis;

public enum OID : uint
{
    Boss = 0x3D39, //R=6.0
    GymnasiouNereis = 0x3D3A, //R=2.0
    GymnasticGarlic = 0x3D51, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticQueen = 0x3D53, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticEggplant = 0x3D50, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticOnion = 0x3D4F, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticTomato = 0x3D52, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasiouLampas = 0x3D4D, //R=2.001
    GymnasiouLyssa = 0x3D4E, //R=3.75
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // GymnasiouLyssa->player, no cast, single-target
    AutoAttack2 = 871, // GymnasiouNereis->player, no cast, single-target
    AutoAttack3 = 872, // Boss->player, no cast, single-target

    WaveOfTurmoilVisual = 32257, // Boss->self, 5.0s cast, single-target
    WaveOfTurmoil = 32258, // Helper->self, 5.0s cast, range 40 circle, knockback 20 away from source
    Hydrobomb = 32259, // Helper->location, 6.5s cast, range 10 circle
    Ceras = 32255, // Boss->player, 5.0s cast, single-target
    WaterspoutVisual = 32261, // Boss->self, 3.0s cast, single-target
    Waterspout = 32262, // Helper->location, 3.0s cast, range 8 circle
    FallingWaterVisual = 32199, // Boss->self, no cast, single-target
    FallingWater = 32260, // Helper->player, 5.0s cast, range 8 circle
    Hydrocannon = 32264, // GymnasiouNereis->self, 3.6s cast, range 17 width 3 rect
    Hydrocannon2 = 32256, // Boss->self, 3.0s cast, range 27 width 6 rect
    Immersion = 32263, // Boss->self, 5.0s cast, range 50 circle

    PluckAndPrune = 32302, // GymnasticEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // GymnasticQueen->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // GymnasticTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // GymnasticGarlic->self, 3.5s cast, range 7 circle
    TearyTwirl = 32301, // GymnasticOnion->self, 3.5s cast, range 7 circle
    HeavySmash = 32317, // GymnasiouLyssa->location, 3.0s cast, range 6 circle
    Telega = 9630 // Mandragoras/Lyssa->self, no cast, single-target, bonus add disappear
}

class Ceras(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Ceras));

class WaveOfTurmoil(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.WaveOfTurmoil), 20, stopAtWall: true)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Module.FindComponent<Hydrobomb>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;
}

class Hydrobomb(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Hydrobomb), 10);
class Waterspout(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Waterspout), 8);
class Hydrocannon(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Hydrocannon), new AOEShapeRect(17, 1.5f));
class Hydrocannon2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Hydrocannon2), new AOEShapeRect(27, 3));
class FallingWater(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.FallingWater), 8);
class Immersion(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Immersion));

abstract class Mandragoras(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(7));
class PluckAndPrune(BossModule module) : Mandragoras(module, AID.PluckAndPrune);
class TearyTwirl(BossModule module) : Mandragoras(module, AID.TearyTwirl);
class HeirloomScream(BossModule module) : Mandragoras(module, AID.HeirloomScream);
class PungentPirouette(BossModule module) : Mandragoras(module, AID.PungentPirouette);
class Pollen(BossModule module) : Mandragoras(module, AID.Pollen);

class HeavySmash(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HeavySmash), 6);

class GymnasiouMeganereisStates : StateMachineBuilder
{
    public GymnasiouMeganereisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Ceras>()
            .ActivateOnEnter<WaveOfTurmoil>()
            .ActivateOnEnter<Hydrobomb>()
            .ActivateOnEnter<Waterspout>()
            .ActivateOnEnter<Hydrocannon>()
            .ActivateOnEnter<Hydrocannon2>()
            .ActivateOnEnter<FallingWater>()
            .ActivateOnEnter<Immersion>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<HeavySmash>()
            .Raw.Update = () => module.Enemies(GymnasiouMeganereis.All).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12014)]
public class GymnasiouMeganereis(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GymnasticEggplant, (uint)OID.GymnasticGarlic, (uint)OID.GymnasticOnion, (uint)OID.GymnasticTomato,
    (uint)OID.GymnasticQueen, (uint)OID.GymnasiouLyssa, (uint)OID.GymnasiouLampas];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.GymnasiouNereis, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.GymnasiouNereis));
        Arena.Actors(Enemies(bonusAdds), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.GymnasticOnion => 6,
                OID.GymnasticEggplant => 5,
                OID.GymnasticGarlic => 4,
                OID.GymnasticTomato => 3,
                OID.GymnasticQueen or OID.GymnasiouLampas or OID.GymnasiouLyssa => 2,
                OID.GymnasiouNereis => 1,
                _ => 0
            };
        }
    }
}
