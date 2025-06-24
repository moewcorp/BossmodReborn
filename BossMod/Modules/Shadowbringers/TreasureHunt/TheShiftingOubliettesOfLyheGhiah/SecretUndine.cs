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

class Hydrofan(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hydrofan, new AOEShapeCone(44f, 15f.Degrees()));
class Hypnowave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hypnowave, new AOEShapeCone(30f, 60f.Degrees()));
class Hydropins(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hydropins, new AOEShapeRect(12f, 2f));
class AquaGlobe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AquaGlobe, 8f);
class Hydrowhirl(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hydrowhirl, 8f);
class Hydrotaph(BossModule module) : Components.RaidwideCast(module, (uint)AID.Hydrotaph);

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
class Mash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mash, new AOEShapeRect(13f, 2f));
class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));

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
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(SecretUndine.All);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (!enemies[i].IsDeadOrDestroyed)
                        return false;
                }
                return true;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9790)]
public class SecretUndine(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.SecretEgg, (uint)OID.SecretGarlic, (uint)OID.SecretOnion, (uint)OID.SecretTomato,
    (uint)OID.SecretQueen, (uint)OID.KeeperOfKeys];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.AqueousAether, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.AqueousAether));
        Arena.Actors(Enemies(bonusAdds), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.SecretOnion => 6,
                (uint)OID.SecretEgg => 5,
                (uint)OID.SecretGarlic => 4,
                (uint)OID.SecretTomato => 3,
                (uint)OID.SecretQueen or (uint)OID.KeeperOfKeys => 2,
                (uint)OID.AqueousAether => 1,
                _ => 0
            };
        }
    }
}
