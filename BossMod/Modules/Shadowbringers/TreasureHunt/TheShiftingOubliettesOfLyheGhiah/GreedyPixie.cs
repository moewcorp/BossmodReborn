namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.GreedyPixie;

public enum OID : uint
{
    Boss = 0x3018, //R=1.6
    SecretMorpho = 0x3019, //R=1.8
    PixieDouble1 = 0x304C, //R=1.6
    PixieDouble2 = 0x304D, //R=1.6
    SecretQueen = 0x3021, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    SecretGarlic = 0x301F, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    SecretTomato = 0x3020, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    SecretOnion = 0x301D, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    SecretEgg = 0x301E, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    KeeperOfKeys = 0x3034, // R3.23
    FuathTrickster = 0x3033, // R0.75
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 23185, // Boss/Mandragoras->player, no cast, single-target
    AutoAttack2 = 872, // KeeperOfKeys/SecretMorpho->player, no cast, single-target

    WindRune = 21686, // Boss->self, 3.0s cast, range 40 width 8 rect
    SongRune = 21684, // Boss->location, 3.0s cast, range 6 circle
    StormRune = 21682, // Boss->self, 4.0s cast, range 40 circle
    BushBash1 = 22779, // PixieDouble2->self, 7.0s cast, range 12 circle
    BushBash2 = 21683, // Boss->self, 5.0s cast, range 12 circle
    NatureCall1 = 22780, // PixieDouble1->self, 7.0s cast, range 30 120-degree cone, turns player into a plant
    NatureCall2 = 21685, // Boss->self, 5.0s cast, range 30 120-degree cone, turns player into a plant

    Pollen = 6452, // SecretQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // SecretOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // SecretTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // SecretEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // SecretGarlic->self, 3.5s cast, range 6+R circle
    Mash = 21767, // KeeperOfKeys->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // KeeperOfKeys->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // KeeperOfKeys->self, 4.0s cast, range 11 circle
    Scoop = 21768, // KeeperOfKeys->self, 4.0s cast, range 15 120-degree cone
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus adds disappear
}

class Windrune(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WindRune), new AOEShapeRect(40, 4));
class SongRune(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.SongRune), 6);
class StormRune(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.StormRune));

abstract class BushBash(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(12));
class BushBash1(BossModule module) : BushBash(module, AID.BushBash1);
class BushBash2(BossModule module) : BushBash(module, AID.BushBash2);

abstract class NatureCall(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(30, 60.Degrees()));
class NatureCall1(BossModule module) : NatureCall(module, AID.NatureCall1);
class NatureCall2(BossModule module) : NatureCall(module, AID.NatureCall2);

abstract class Mandragoras(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(6.84f));
class PluckAndPrune(BossModule module) : Mandragoras(module, AID.PluckAndPrune);
class TearyTwirl(BossModule module) : Mandragoras(module, AID.TearyTwirl);
class HeirloomScream(BossModule module) : Mandragoras(module, AID.HeirloomScream);
class PungentPirouette(BossModule module) : Mandragoras(module, AID.PungentPirouette);
class Pollen(BossModule module) : Mandragoras(module, AID.Pollen);

class Spin(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCircle(11));
class Mash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Mash), new AOEShapeRect(13, 2));
class Scoop(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Scoop), new AOEShapeCone(15, 60.Degrees()));

class GreedyPixieStates : StateMachineBuilder
{
    public GreedyPixieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Windrune>()
            .ActivateOnEnter<StormRune>()
            .ActivateOnEnter<SongRune>()
            .ActivateOnEnter<BushBash1>()
            .ActivateOnEnter<BushBash2>()
            .ActivateOnEnter<NatureCall1>()
            .ActivateOnEnter<NatureCall2>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .Raw.Update = () => Module.WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9797)]
public class GreedyPixie(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.SecretMorpho));
        Arena.Actors(Enemies(OID.SecretEgg).Concat(Enemies(OID.SecretTomato)).Concat(Enemies(OID.SecretQueen)).Concat(Enemies(OID.SecretGarlic)).Concat(Enemies(OID.SecretOnion))
        .Concat(Enemies(OID.KeeperOfKeys)).Concat(Enemies(OID.FuathTrickster)), Colors.Vulnerable);
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
                OID.SecretTomato or OID.FuathTrickster => 3,
                OID.SecretQueen or OID.KeeperOfKeys => 2,
                OID.SecretMorpho => 1,
                _ => 0
            };
        }
    }
}
