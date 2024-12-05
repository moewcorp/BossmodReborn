namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretPegasus;

public enum OID : uint
{
    Boss = 0x3016, //R=2.5
    Thunderhead = 0x3017, //R=1.0
    KeeperOfKeys = 0x3034, // R3.23
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/KeeperOfKeys->player, no cast, single-target

    BurningBright = 21667, // Boss->self, 3.0s cast, range 47 width 6 rect
    Nicker = 21668, // Boss->self, 4.0s cast, range 12 circle
    CloudCall = 21666, // Boss->self, 3.0s cast, single-target, calls clouds
    Gallop = 21665, // Boss->players, no cast, width 10 rect charge, seems to target random player 5-6s after CloudCall
    LightningBolt = 21669, // Thunderhead->self, 3.0s cast, range 8 circle

    Telega = 9630, // KeeperOfKeys->self, no cast, single-target, bonus adds disappear
    Mash = 21767, // KeeperOfKeys->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // KeeperOfKeys->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // KeeperOfKeys->self, 4.0s cast, range 11 circle
    Scoop = 21768 // KeeperOfKeys->self, 4.0s cast, range 15 120-degree cone
}

class BurningBright(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BurningBright), new AOEShapeRect(47, 3));
class Nicker(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Nicker), new AOEShapeCircle(12));
class CloudCall(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.CloudCall), "Calls thunderclouds");
class LightningBolt(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LightningBolt), new AOEShapeCircle(8));

class Spin(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCircle(11));
class Mash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Mash), new AOEShapeRect(13, 2));
class Scoop(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Scoop), new AOEShapeCone(15, 60.Degrees()));

class SecretPegasusStates : StateMachineBuilder
{
    public SecretPegasusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BurningBright>()
            .ActivateOnEnter<Nicker>()
            .ActivateOnEnter<CloudCall>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .Raw.Update = () => Module.WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9793)]
public class SecretPegasus(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.KeeperOfKeys), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.KeeperOfKeys => 1,
                _ => 0
            };
        }
    }
}
