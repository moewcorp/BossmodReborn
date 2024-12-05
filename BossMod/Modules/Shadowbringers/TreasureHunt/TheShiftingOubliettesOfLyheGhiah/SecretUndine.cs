namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretUndine;

public enum OID : uint
{
    Boss = 0x3011, //R=3.6
    AqueousAether = 0x3013, //R=1.12
    Bubble = 0x3012, //R=1.3, untargetable
    SecretQueen = 0x3021, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    SecretGarlic = 0x301F, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    SecretTomato = 0x3020, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    SecretOnion = 0x301D, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    SecretEgg = 0x301E, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards 
    KeeperOfKeys = 0x3034, // R3.23
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 23186, // Boss/AqueousAether->player, no cast, single-target
    AutoAttack2 = 872, // KeeperOfKeys/Mandragoras->player, no cast, single-target

    Hydrowhirl = 21658, // Boss->self, 3.0s cast, range 8 circle
    Hypnowave = 21659, // Boss->self, 3.0s cast, range 30 120-degree cone, causes sleep
    HydrotaphVisual = 21661, // Boss->self, 4.0s cast, single-target
    Hydrotaph = 21662, // Helper->self, 4.0s cast, range 40 circle
    Hydrofan = 21663, // Bubble->self, 5.0s cast, range 44 30-degree cone
    Hydropins = 21660, // Boss->self, 2.5s cast, range 12 width 4 rect
    AquaGlobe = 21664, // AqueousAether->location, 3.0s cast, range 8 circle

    Pollen = 6452, // SecretQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // SecretOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // SecretTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // SecretEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // SecretGarlic->self, 3.5s cast, range 6+R circle
    Mash = 21767, // KeeperOfKeys->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // KeeperOfKeys->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // KeeperOfKeys->self, 4.0s cast, range 11 circle
    Scoop = 21768, // KeeperOfKeys->self, 4.0s cast, range 15 120-degree cone
    Telega = 9630 // Mandragoras/KeeperOfKeys->self, no cast, single-target, bonus adds disappear
}

class Hydrofan(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Hydrofan), new AOEShapeCone(44, 15.Degrees()));
class Hypnowave(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Hypnowave), new AOEShapeCone(30, 60.Degrees()));
class Hydropins(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Hydropins), new AOEShapeRect(12, 2));
class AquaGlobe(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.AquaGlobe), 8);
class Hydrowhirl(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Hydrowhirl), new AOEShapeCircle(8));
class Hydrotaph(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Hydrotaph));

abstract class Mandragoras(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(6.84f));
class PluckAndPrune(BossModule module) : Mandragoras(module, AID.PluckAndPrune);
class TearyTwirl(BossModule module) : Mandragoras(module, AID.TearyTwirl);
class HeirloomScream(BossModule module) : Mandragoras(module, AID.HeirloomScream);
class PungentPirouette(BossModule module) : Mandragoras(module, AID.PungentPirouette);
class Pollen(BossModule module) : Mandragoras(module, AID.Pollen);

class Spin(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCircle(11));
class Mash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Mash), new AOEShapeRect(13, 2));
class Scoop(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Scoop), new AOEShapeCone(15, 60.Degrees()));

class SecretUndineStates : StateMachineBuilder
{
    public SecretUndineStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hydrofan>()
            .ActivateOnEnter<Hypnowave>()
            .ActivateOnEnter<Hydropins>()
            .ActivateOnEnter<AquaGlobe>()
            .ActivateOnEnter<Hydrowhirl>()
            .ActivateOnEnter<Hydrotaph>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .Raw.Update = () => Module.WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9790)]
public class SecretUndine(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.AqueousAether).Concat([PrimaryActor]));
        Arena.Actors(Enemies(OID.SecretEgg).Concat(Enemies(OID.SecretTomato)).Concat(Enemies(OID.SecretQueen)).Concat(Enemies(OID.SecretGarlic)).Concat(Enemies(OID.SecretOnion)
        .Concat(Enemies(OID.KeeperOfKeys))), Colors.Vulnerable);

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
                OID.SecretQueen or OID.KeeperOfKeys => 2,
                OID.AqueousAether => 1,
                _ => 0
            };
        }
    }
}
