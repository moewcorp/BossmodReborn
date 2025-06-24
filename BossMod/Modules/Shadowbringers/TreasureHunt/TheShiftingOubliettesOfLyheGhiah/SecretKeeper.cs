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

class Buffet(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Buffet, new AOEShapeCone(11f, 60f.Degrees()));
class Inhale(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InhaleBoss, new AOEShapeCone(20f, 60f.Degrees()));
class InhalePull(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.InhaleBoss, 20f, false, 1, new AOEShapeCone(20f, 60f.Degrees()), Kind.TowardsOrigin, default, true);
class HeavyScrapline(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavyScrapline, 11f);
class MoldyPhlegm(BossModule module) : Components.VoidzoneAtCastTarget(module, 6f, (uint)AID.MoldyPhlegm, GetVoidzones, 1.4f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.ResinVoidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}
class MoldySneeze(BossModule module) : Components.Cleave(module, (uint)AID.MoldySneeze, new AOEShapeCone(12f, 60f.Degrees()));

class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
class Mash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mash, new AOEShapeRect(13f, 2f));
class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));

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
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(SecretKeeper.All);
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9807)]
public class SecretKeeper(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.FuathTrickster, (uint)OID.KeeperOfKeys];
    public static readonly uint[] All = [(uint)OID.Boss, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
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
                (uint)OID.FuathTrickster => 2,
                (uint)OID.KeeperOfKeys => 1,
                _ => 0
            };
        }
    }
}
