namespace BossMod.Shadowbringers.Alliance.A32HanselGretel;

class Wail1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Wail1));
class Wail2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Wail2));

class CripplingBlow1(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.CripplingBlow1));
class CripplingBlow2(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.CripplingBlow2));

abstract class BloodySweep(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(50, 12.5f));
class BloodySweep1(BossModule module) : BloodySweep(module, AID.BloodySweep1);
class BloodySweep2(BossModule module) : BloodySweep(module, AID.BloodySweep2);
class BloodySweep3(BossModule module) : BloodySweep(module, AID.BloodySweep3);
class BloodySweep4(BossModule module) : BloodySweep(module, AID.BloodySweep4);

class SeedOfMagicAlpha(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.SeedOfMagicAlpha), 5);
class RiotOfMagic(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.RiotOfMagic), 5, 8);

class PassingLance(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.PassingLance), new AOEShapeRect(50, 12));
class Explosion(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Explosion), new AOEShapeRect(4, 25));
class UnevenFooting(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.UnevenFooting), 17);

abstract class HungryLance(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(40, 60.Degrees()));
class HungryLance1(BossModule module) : HungryLance(module, AID.HungryLance1);
class HungryLance2(BossModule module) : HungryLance(module, AID.HungryLance2);

class Breakthrough(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Breakthrough), new AOEShapeRect(53, 16));
class SeedOfMagicBeta(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SeedOfMagicBeta), 5);
class Lamentation(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Lamentation));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.Gretel, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9990)]
public class A32HanselGretel(WorldState ws, Actor primary) : BossModule(ws, primary, new(-800, -951), new ArenaBoundsCircle(25))
{
    private Actor? _hansel;

    public Actor? Gretel() => PrimaryActor;
    public Actor? Hansel() => _hansel;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _hansel ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.Hansel).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_hansel);
    }
}
