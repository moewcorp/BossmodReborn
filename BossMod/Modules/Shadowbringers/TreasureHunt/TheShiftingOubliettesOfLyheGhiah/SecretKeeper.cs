namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretKeeper;

public enum OID : uint
{
    Boss = 0x3023, //R=3.99
    ResinVoidzone = 0x1E8FC7,
    KeeperOfKeys = 0x3034, // R3.23
    FuathTrickster = 0x3033, // R0.75
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/KeeperOfKeys->player, no cast, single-target

    Buffet = 21680, // Boss->self, 3.0s cast, range 11 120-degree cone
    HeavyScrapline = 21681, // Boss->self, 4.0s cast, range 11 circle
    MoldyPhlegm = 21679, // Boss->location, 3.0s cast, range 6 circle
    InhaleBoss = 21677, // Boss->self, 4.0s cast, range 20 120-degree cone
    MoldySneeze = 21678, // Boss->self, no cast, range 12 120-degree cone, heavy dmg, 20 knockback away from source

    Telega = 9630, // KeeperOfKeys/FuathTrickster->self, no cast, single-target, bonus adds disappear
    Mash = 21767, // KeeperOfKeys->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // KeeperOfKeys->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // KeeperOfKeys->self, 4.0s cast, range 11 circle
    Scoop = 21768 // KeeperOfKeys->self, 4.0s cast, range 15 120-degree cone
}

class Buffet(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Buffet), new AOEShapeCone(11, 60.Degrees()));
class Inhale(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.InhaleBoss), new AOEShapeCone(20, 60.Degrees()));
class InhalePull(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.InhaleBoss), 20, false, 1, new AOEShapeCone(20, 60.Degrees()), Kind.TowardsOrigin, default, true);
class HeavyScrapline(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HeavyScrapline), new AOEShapeCircle(11));
class MoldyPhlegm(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, ActionID.MakeSpell(AID.MoldyPhlegm), m => m.Enemies(OID.ResinVoidzone).Where(z => z.EventState != 7), 1.4f);
class MoldySneeze(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.MoldySneeze), new AOEShapeCone(12, 60.Degrees()), (uint)OID.Boss);

class Spin(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCircle(11));
class Mash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Mash), new AOEShapeRect(13, 2));
class Scoop(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Scoop), new AOEShapeCone(15, 60.Degrees()));

class SecretKeeperStates : StateMachineBuilder
{
    public SecretKeeperStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Buffet>()
            .ActivateOnEnter<Inhale>()
            .ActivateOnEnter<InhalePull>()
            .ActivateOnEnter<HeavyScrapline>()
            .ActivateOnEnter<MoldyPhlegm>()
            .ActivateOnEnter<MoldySneeze>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .Raw.Update = () => Module.WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9807)]
public class SecretKeeper(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.KeeperOfKeys).Concat(Enemies(OID.FuathTrickster)), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.FuathTrickster => 2,
                OID.KeeperOfKeys => 1,
                _ => 0
            };
        }
    }
}
