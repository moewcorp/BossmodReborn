namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN2Dahu;

class FallingRock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FallingRock, 4f);
class HotCharge(BossModule module) : Components.ChargeAOEs(module, (uint)AID.HotCharge, 4f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (NumCasts % 2 == 0)
            return;
        hints.GoalZones.Add(hints.GoalSingleTarget(Module.PrimaryActor.CastInfo?.LocXZ ?? Arena.Center, 5f, 5f)); // follow the charge
    }
}

class Firebreathe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Firebreathe, new AOEShapeCone(60f, 45f.Degrees()));
class HeadDown(BossModule module) : Components.ChargeAOEs(module, (uint)AID.HeadDown, 2f);
class HuntersClaw(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HuntersClaw, 8f);
class HeatBreath(BossModule module) : Components.BaitAwayCast(module, (uint)AID.HeatBreath, new AOEShapeCone(10f, 45f.Degrees()), endsOnCastEvent: true, tankbuster: true);
class RipperClaw(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RipperClaw, new AOEShapeCone(10f, 45f.Degrees()));
class TailSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HuntersClaw, 10f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 760, NameID = 9751)]
public class DRN2Dahu(WorldState ws, Actor primary) : Dahu(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies((uint)OID.Marchosias));
    }
}
