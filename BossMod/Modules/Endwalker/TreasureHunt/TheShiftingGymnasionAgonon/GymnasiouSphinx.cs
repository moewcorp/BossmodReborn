namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouSphinx;

public enum OID : uint
{
    Boss = 0x3D3B, //R=4.83
    GymnasiouGryps = 0x3D3C, //R=2.3
    VerdantPlume = 0x3D3D, //R=0.65
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
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // GymnasiouGryps->player, no cast, single-target

    FeatherWind = 32267, // Boss->self, 4.0s cast, single-target, spawns Verdant Plumes
    Explosion = 32273, // VerdantPlume->self, 5.0s cast, range 3-12 donut
    Scratch = 32265, // Boss->player, 5.0s cast, single-target
    AeroIIVisual = 32268, // Boss->self, 3.0s cast, single-target
    AeroII = 32269, // Helper->location, 3.0s cast, range 4 circle
    FervidPulse = 32272, // Boss->self, 5.0s cast, range 50 width 14 cross
    Spreadmarkers = 32199, // Boss->self, no cast, single-target
    FeatherRain = 32271, // Helper->player, 5.0s cast, range 6 circle
    FrigidPulse = 32270, // Boss->self, 5.0s cast, range 12-60 donut
    AlpineDraft = 32274, // GymnasiouGryps->self, 3.0s cast, range 45 width 5 rect
    MoltingPlumage = 32266, // Boss->self, 5.0s cast, range 60 circle

    PluckAndPrune = 32302, // GymnasticEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // GymnasticQueen->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // GymnasticTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // GymnasticGarlic->self, 3.5s cast, range 7 circle
    TearyTwirl = 32301, // GymnasticOnion->self, 3.5s cast, range 7 circle
    HeavySmash = 32317, // GymnasiouLyssa->location, 3.0s cast, range 6 circle
    Telega = 9630 // Mandragoras/Lampas/Lyssa->self, no cast, single-target, bonus add disappear
}

class Scratch(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Scratch));
class Explosion(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Explosion), new AOEShapeDonut(3, 12));
class FrigidPulse(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FrigidPulse), new AOEShapeDonut(12, 60));
class FervidPulse(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FervidPulse), new AOEShapeCross(50, 7));
class MoltingPlumage(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MoltingPlumage));
class AlpineDraft(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AlpineDraft), new AOEShapeRect(45, 2.5f));
class FeatherRain(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.FeatherRain), 6);
class AeroII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.AeroII), 4);

abstract class Mandragoras(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(7));
class PluckAndPrune(BossModule module) : Mandragoras(module, AID.PluckAndPrune);
class TearyTwirl(BossModule module) : Mandragoras(module, AID.TearyTwirl);
class HeirloomScream(BossModule module) : Mandragoras(module, AID.HeirloomScream);
class PungentPirouette(BossModule module) : Mandragoras(module, AID.PungentPirouette);
class Pollen(BossModule module) : Mandragoras(module, AID.Pollen);

class HeavySmash(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HeavySmash), 6);

class GymnasiouSphinxStates : StateMachineBuilder
{
    public GymnasiouSphinxStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Scratch>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<FrigidPulse>()
            .ActivateOnEnter<FervidPulse>()
            .ActivateOnEnter<MoltingPlumage>()
            .ActivateOnEnter<AlpineDraft>()
            .ActivateOnEnter<FeatherRain>()
            .ActivateOnEnter<AeroII>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<HeavySmash>()
            .Raw.Update = () => module.Enemies(GymnasiouSphinx.All).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12016)]
public class GymnasiouSphinx(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GymnasticEggplant, (uint)OID.GymnasticGarlic, (uint)OID.GymnasticOnion, (uint)OID.GymnasticTomato,
    (uint)OID.GymnasticQueen, (uint)OID.GymnasiouLyssa, (uint)OID.GymnasiouLampas];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.GymnasiouGryps, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.GymnasiouGryps));
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
                OID.GymnasiouGryps => 1,
                _ => 0
            };
        }
    }
}
