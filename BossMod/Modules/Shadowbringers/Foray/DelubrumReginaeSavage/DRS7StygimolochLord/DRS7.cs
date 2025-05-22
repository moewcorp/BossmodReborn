namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

class FoeSplitter(BossModule module) : Components.Cleave(module, (uint)AID.FoeSplitter, new AOEShapeCone(9f, 45f.Degrees())); // TODO: verify angle
class ThunderousDischarge(BossModule module) : Components.CastCounter(module, (uint)AID.ThunderousDischargeAOE);
class ThousandTonzeSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThousandTonzeSwing, 20f);
class Whack(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WhackAOE, new AOEShapeCone(40f, 30f.Degrees()));
class DevastatingBoltOuter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DevastatingBoltOuter, new AOEShapeDonut(25f, 30f));
class DevastatingBoltInner(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DevastatingBoltInner, new AOEShapeDonut(12f, 17f));
class Electrocution(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Electrocution, 3f);

// TODO: ManaFlame component - show reflect hints
[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 761, NameID = 9759, PlanLevel = 80)]
public class DRS7StygimolochLord(WorldState ws, Actor primary) : BossModule(ws, primary, Border.DefaultBounds.Center, Border.DefaultBounds)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies((uint)OID.StygimolochMonk));
        Arena.Actors(Enemies((uint)OID.BallOfEarth), Colors.Object);
        Arena.Actors(Enemies((uint)OID.BallOfFire), Colors.Object);
    }
}
