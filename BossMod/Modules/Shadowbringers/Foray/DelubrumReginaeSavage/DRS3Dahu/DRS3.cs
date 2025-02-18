namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS3Dahu;

class FallingRock(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FallingRock), 4);
class HotCharge(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.HotCharge), 4);
class Firebreathe(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Firebreathe), new AOEShapeCone(60, 45.Degrees()));
class HeadDown(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.HeadDown), 2);
class HuntersClaw(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HuntersClaw), 8);

class Burn(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(30), (uint)IconID.Burn, ActionID.MakeSpell(AID.Burn), 8.2f, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 761, NameID = 9751, PlanLevel = 80)]
public class DRS3(WorldState ws, Actor primary) : BossModule(ws, primary, new(82, 138), new ArenaBoundsCircle(29.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies(OID.CrownedMarchosias));
    }
}
