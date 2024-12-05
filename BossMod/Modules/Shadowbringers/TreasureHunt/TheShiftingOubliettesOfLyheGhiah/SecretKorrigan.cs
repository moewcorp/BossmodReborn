namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretKorrigan;

public enum OID : uint
{
    Boss = 0x3022, //R=2.85
    SecretMandragora = 0x301C, //R=0.84
    SecretQueen = 0x3021, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    SecretGarlic = 0x301F, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    SecretTomato = 0x3020, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    SecretOnion = 0x301D, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    SecretEgg = 0x301E, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/SecretMandragora/Mandragoras->player, no cast, single-target

    Hypnotize = 21674, // Boss->self, 4.0s cast, range 40 circle
    LeafDagger = 21675, // Boss->location, 2.5s cast, range 3 circle
    SaibaiMandragora = 21676, // Boss->self, 3.0s cast, single-target
    Ram = 21673, // Boss->player, 3.0s cast, single-target

    Pollen = 6452, // SecretQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // SecretOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // SecretTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // SecretEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // SecretGarlic->self, 3.5s cast, range 6+R circle
    Telega = 9630 // Mandragoras->self, no cast, single-target, bonus adds disappear
}

class Hypnotize(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.Hypnotize));
class Ram(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.Ram));
class SaibaiMandragora(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.SaibaiMandragora), "Calls adds");
class LeafDagger(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.LeafDagger), 3);

abstract class Mandragoras(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(6.84f));
class PluckAndPrune(BossModule module) : Mandragoras(module, AID.PluckAndPrune);
class TearyTwirl(BossModule module) : Mandragoras(module, AID.TearyTwirl);
class HeirloomScream(BossModule module) : Mandragoras(module, AID.HeirloomScream);
class PungentPirouette(BossModule module) : Mandragoras(module, AID.PungentPirouette);
class Pollen(BossModule module) : Mandragoras(module, AID.Pollen);

class SecretKorriganStates : StateMachineBuilder
{
    public SecretKorriganStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hypnotize>()
            .ActivateOnEnter<LeafDagger>()
            .ActivateOnEnter<SaibaiMandragora>()
            .ActivateOnEnter<Ram>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .Raw.Update = () => Module.WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9806)]
public class SecretKorrigan(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.SecretMandragora));
        Arena.Actors(Enemies(OID.SecretEgg).Concat(Enemies(OID.SecretTomato)).Concat(Enemies(OID.SecretQueen)).Concat(Enemies(OID.SecretGarlic)).Concat(Enemies(OID.SecretOnion)), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.SecretOnion => 6,
                OID.SecretEgg => 5,
                OID.SecretGarlic => 4,
                OID.SecretTomato => 3,
                OID.SecretQueen => 2,
                OID.SecretMandragora => 1,
                _ => 0
            };
        }
    }
}
