namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouStyphnolobion;

public enum OID : uint
{
    Boss = 0x3D37, //R=5.3
    GymnasiouHippogryph = 0x3D38, //R=2.53
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
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // GymnasiouHippogryph->player, no cast, single-target

    EarthQuakerVisual1 = 32199, // Boss->self, no cast, single-target
    EarthQuakerVisual2 = 32247, // Boss->self, 3.5s cast, single-target
    EarthQuaker1 = 32248, // Helper->self, 4.0s cast, range 10 circle
    EarthQuaker2 = 32249, // Helper->self, 6.0s cast, range 10-20 donut
    Rake = 32245, // Boss->player, 5.0s cast, single-target
    EarthShaker = 32250, // Boss->self, 3.5s cast, single-target
    EarthShaker2 = 32251, // Helper->players, 4.0s cast, range 60 30-degree cone
    StoneIII = 32252, // Boss->self, 2.5s cast, single-target
    StoneIII2 = 32253, // Helper->location, 3.0s cast, range 6 circle
    BeakSnap = 32254, // GymnasiouHippogryph->player, no cast, single-target
    Tiiimbeeer = 32246, // Boss->self, 5.0s cast, range 50 circle

    PluckAndPrune = 32302, // GymnasticEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // GymnasticQueen->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // GymnasticTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // GymnasticGarlic->self, 3.5s cast, range 7 circle
    TearyTwirl = 32301, // GymnasticOnion->self, 3.5s cast, range 7 circle
    HeavySmash = 32317, // GymnasiouLyssa->location, 3.0s cast, range 6 circle
    Telega = 9630 // Mandragoras/GymnasiouLyssa->self, no cast, single-target, bonus add disappear
}

class Rake(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Rake));
class Tiiimbeeer(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Tiiimbeeer));
class StoneIII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.StoneIII2), 6);
class EarthShaker(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.EarthShaker2), new AOEShapeCone(60, 15.Degrees()), endsOnCastEvent: true);

class EarthQuaker(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10), new AOEShapeDonut(10, 20)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.EarthQuakerVisual2)
            AddSequence(Module.Center, Module.CastFinishAt(spell, 0.5f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Sequences.Count > 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.EarthQuaker1 => 0,
                AID.EarthQuaker2 => 1,
                _ => -1
            };
            AdvanceSequence(order, Arena.Center, WorldState.FutureTime(2));
        }
    }
}

abstract class Mandragoras(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(7));
class PluckAndPrune(BossModule module) : Mandragoras(module, AID.PluckAndPrune);
class TearyTwirl(BossModule module) : Mandragoras(module, AID.TearyTwirl);
class HeirloomScream(BossModule module) : Mandragoras(module, AID.HeirloomScream);
class PungentPirouette(BossModule module) : Mandragoras(module, AID.PungentPirouette);
class Pollen(BossModule module) : Mandragoras(module, AID.Pollen);

class HeavySmash(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HeavySmash), 6);

class GymnasiouStyphnolobionStates : StateMachineBuilder
{
    public GymnasiouStyphnolobionStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Rake>()
            .ActivateOnEnter<Tiiimbeeer>()
            .ActivateOnEnter<StoneIII>()
            .ActivateOnEnter<EarthShaker>()
            .ActivateOnEnter<EarthQuaker>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<HeavySmash>()
            .Raw.Update = () => module.Enemies(GymnasiouStyphnolobion.All).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12012)]
public class GymnasiouStyphnolobion(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GymnasticEggplant, (uint)OID.GymnasticGarlic, (uint)OID.GymnasticOnion, (uint)OID.GymnasticTomato,
    (uint)OID.GymnasticQueen, (uint)OID.GymnasiouLyssa];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.GymnasiouHippogryph, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.GymnasiouHippogryph));
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
                OID.GymnasticQueen or OID.GymnasiouLyssa => 2,
                OID.GymnasiouHippogryph => 1,
                _ => 0
            };
        }
    }
}
