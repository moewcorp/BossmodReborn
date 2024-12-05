namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretDjinn;

public enum OID : uint
{
    Boss = 0x300F, //R=3.48
    SecretRabbitsTail = 0x3010, //R=1.32
    KeeperOfKeys = 0x3034, // R3.23
    FuathTrickster = 0x3033, // R0.75
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 23185, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // SecretRabbitsTail/KeeperOfKeys->player, no cast, single-target

    Gust = 21655, // Boss->location, 3.0s cast, range 6 circle
    ChangelessWinds = 21657, // Boss->self, 3.0s cast, range 40 width 8 rect, knockback 10, source forward
    WhirlingGaol = 21654, // Boss->self, 4.0s cast, range 40 circle, knockback 25 away from source
    Whipwind = 21656, // Boss->self, 5.0s cast, range 55 width 40 rect, knockback 25, source forward
    GentleBreeze = 21653, // SecretRabbitsTail->self, 3.0s cast, range 15 width 4 rect

    Telega = 9630, // KeeperOfKeys/FuathTrickster->self, no cast, single-target, bonus adds disappear
    Mash = 21767, // KeeperOfKeys->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // KeeperOfKeys->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // KeeperOfKeys->self, 4.0s cast, range 11 circle
    Scoop = 21768 // KeeperOfKeys->self, 4.0s cast, range 15 120-degree cone
}

class Gust(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Gust), 6);
class ChangelessWinds(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ChangelessWinds), new AOEShapeRect(40, 4));
class ChangelessWindsKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.ChangelessWinds), 10, shape: new AOEShapeRect(40, 4), kind: Kind.DirForward, stopAtWall: true);
class Whipwind(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Whipwind), new AOEShapeRect(54, 20, 1));
class WhipwindKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Whipwind), 25, shape: new AOEShapeRect(55, 20, 1), kind: Kind.DirForward, stopAtWall: true);
class GentleBreeze(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GentleBreeze), new AOEShapeRect(15, 2));
class WhirlingGaol(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.WhirlingGaol), "Raidwide + Knockback");
class WhirlingGaolKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.WhirlingGaol), 25, stopAtWall: true);

class Spin(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCircle(11));
class Mash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Mash), new AOEShapeRect(13, 2));
class Scoop(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Scoop), new AOEShapeCone(15, 60.Degrees()));

class SecretDjinnStates : StateMachineBuilder
{
    public SecretDjinnStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Gust>()
            .ActivateOnEnter<ChangelessWinds>()
            .ActivateOnEnter<ChangelessWindsKB>()
            .ActivateOnEnter<Whipwind>()
            .ActivateOnEnter<WhipwindKB>()
            .ActivateOnEnter<GentleBreeze>()
            .ActivateOnEnter<WhirlingGaol>()
            .ActivateOnEnter<WhirlingGaolKB>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .Raw.Update = () => Module.WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9788)]
public class SecretDjinn(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.SecretRabbitsTail).Concat([PrimaryActor]));
        Arena.Actors(Enemies(OID.KeeperOfKeys).Concat(Enemies(OID.FuathTrickster)), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.FuathTrickster => 3,
                OID.KeeperOfKeys => 2,
                OID.SecretRabbitsTail => 1,
                _ => 0
            };
        }
    }
}
