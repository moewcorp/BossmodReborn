namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouTigris;

public enum OID : uint
{
    Boss = 0x3D29, //R=5.75
    GymnasiouTigrisMikra = 0x3D2A, //R=3.5
    GymnasticGarlic = 0x3D51, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticQueen = 0x3D53, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticEggplant = 0x3D50, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticOnion = 0x3D4F, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticTomato = 0x3D52, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasiouLyssa = 0x3D4E, //R=3.75
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/GymnasiouTigrisMikra/GymnasiouLyssa->player->player, no cast, single-target

    AbsoluteZero = 32208, // Boss->self, 4.0s cast, range 45 90-degree cone
    BlizzardIII = 32209, // Boss->location, 3.0s cast, range 6 circle
    FrumiousJaws = 32206, // Boss->player, 5.0s cast, single-target
    Eyeshine = 32207, // Boss->self, 4.0s cast, range 35 circle
    CatchingClaws = 32210, // GymnasiouTigrisMikra->self, 3.0s cast, range 12 90-degree cone

    PluckAndPrune = 32302, // GymnasticEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // GymnasticQueen->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // GymnasticTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // GymnasticGarlic->self, 3.5s cast, range 7 circle
    TearyTwirl = 32301, // GymnasticOnion->self, 3.5s cast, range 7 circle
    HeavySmash = 32317, // GymnasiouLyssa->location, 3.0s cast, range 6 circle
    Telega = 9630 // GymnasiouLyssa/Mandragoras->self, no cast, single-target, bonus add disappear
}

class AbsoluteZero(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AbsoluteZero), new AOEShapeCone(45, 45.Degrees()));
class FrumiousJaws(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.FrumiousJaws));
class BlizzardIII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.BlizzardIII), 6);
class Eyeshine(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.Eyeshine));
class CatchingClaws(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CatchingClaws), new AOEShapeCone(12, 45.Degrees()));

abstract class Mandragoras(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(7));
class PluckAndPrune(BossModule module) : Mandragoras(module, AID.PluckAndPrune);
class TearyTwirl(BossModule module) : Mandragoras(module, AID.TearyTwirl);
class HeirloomScream(BossModule module) : Mandragoras(module, AID.HeirloomScream);
class PungentPirouette(BossModule module) : Mandragoras(module, AID.PungentPirouette);
class Pollen(BossModule module) : Mandragoras(module, AID.Pollen);

class HeavySmash(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HeavySmash), 6);

class GymnasiouTigrisStates : StateMachineBuilder
{
    public GymnasiouTigrisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AbsoluteZero>()
            .ActivateOnEnter<FrumiousJaws>()
            .ActivateOnEnter<BlizzardIII>()
            .ActivateOnEnter<Eyeshine>()
            .ActivateOnEnter<CatchingClaws>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<HeavySmash>()
            .Raw.Update = () => module.Enemies(GymnasiouTigris.All).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 11999)]
public class GymnasiouTigris(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GymnasticEggplant, (uint)OID.GymnasticGarlic, (uint)OID.GymnasticOnion, (uint)OID.GymnasticTomato,
    (uint)OID.GymnasticQueen, (uint)OID.GymnasiouLyssa];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.GymnasiouTigrisMikra, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.GymnasiouTigrisMikra));
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
                OID.GymnasticQueen => 2,
                OID.GymnasiouTigrisMikra => 1,
                _ => 0
            };
        }
    }
}
