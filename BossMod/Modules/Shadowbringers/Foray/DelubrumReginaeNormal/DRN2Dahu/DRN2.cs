namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN2Dahu;

class FallingRock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FallingRock, 4f);
class HotCharge(BossModule module) : Components.ChargeAOEs(module, (uint)AID.HotCharge, 4f);
class Firebreathe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Firebreathe, new AOEShapeCone(60f, 45f.Degrees()));
class HeadDown(BossModule module) : Components.ChargeAOEs(module, (uint)AID.HeadDown, 2f);
class HuntersClaw(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HuntersClaw, 8f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 760, NameID = 9751)]
public class DRN2Dahu(WorldState ws, Actor primary) : BossModule(ws, primary, new(82f, 138f), new ArenaBoundsCircle(30f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies((uint)OID.Marchosias));
    }
}
