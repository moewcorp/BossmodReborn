namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarMandragora;

public enum OID : uint
{
    Boss = 0x2542, //R=2.85
    AltarKorrigan = 0x255C, //R=0.84
    AltarQueen = 0x254A, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    AltarGarlic = 0x2548, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    AltarTomato = 0x2549, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    AltarOnion = 0x2546, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    AltarEgg = 0x2547, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/Mandragoras->player, no cast, single-target
    AutoAttack2 = 6499, // AltarKorrigan->player, no cast, single-target

    OpticalIntrusion = 13367, // Boss->player, 3.0s cast, single-target
    LeafDagger = 13369, // Boss->location, 2.5s cast, range 3 circle
    SaibaiMandragora = 13370, // Boss->self, 3.0s cast, single-target
    Hypnotize = 13368, // Boss->self, 2.5s cast, range 20+R 90-degree cone, gaze, paralysis

    PluckAndPrune = 6449, // AltarEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // AltarGarlic->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // AltarOnion->self, 3.5s cast, range 6+R circle
    Pollen = 6452, // AltarQueen->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // AltarTomato->self, 3.5s cast, range 6+R circle
    Telega = 9630 // Mandragoras->self, no cast, single-target, bonus add disappear
}

class OpticalIntrusion(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.OpticalIntrusion));
class Hypnotize(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Hypnotize), new AOEShapeCone(22.85f, 45.Degrees()));
class SaibaiMandragora(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.SaibaiMandragora), "Calls adds");
class LeafDagger(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.LeafDagger), 3);

abstract class Mandragoras(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(6.84f));
class PluckAndPrune(BossModule module) : Mandragoras(module, AID.PluckAndPrune);
class TearyTwirl(BossModule module) : Mandragoras(module, AID.TearyTwirl);
class HeirloomScream(BossModule module) : Mandragoras(module, AID.HeirloomScream);
class PungentPirouette(BossModule module) : Mandragoras(module, AID.PungentPirouette);
class Pollen(BossModule module) : Mandragoras(module, AID.Pollen);

class AltarMandragoraStates : StateMachineBuilder
{
    public AltarMandragoraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OpticalIntrusion>()
            .ActivateOnEnter<SaibaiMandragora>()
            .ActivateOnEnter<LeafDagger>()
            .ActivateOnEnter<Hypnotize>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .Raw.Update = () => Module.WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7600)]
public class AltarMandragora(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.AltarKorrigan));
        Arena.Actors(Enemies(OID.AltarEgg).Concat(Enemies(OID.AltarTomato)).Concat(Enemies(OID.AltarQueen)).Concat(Enemies(OID.AltarGarlic))
        .Concat(Enemies(OID.AltarOnion)), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.AltarOnion => 6,
                OID.AltarEgg => 5,
                OID.AltarGarlic => 4,
                OID.AltarTomato => 3,
                OID.AltarQueen => 2,
                OID.AltarKorrigan => 1,
                _ => 0
            };
        }
    }
}
