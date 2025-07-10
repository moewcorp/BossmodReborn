namespace BossMod.Shadowbringers.Alliance.A24TheCompound2P;

sealed class CentrifugalSlice(BossModule module) : Components.RaidwideCast(module, (uint)AID.CentrifugalSlice);
class ThreePartsDisdainStack(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.ThreePartsDisdain1, 6, 8);
class R012LaserLoc(BossModule module) : Components.SimpleAOEs(module, (uint)AID.R012LaserLoc, 6);
class R012LaserSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.R012LaserSpread, 6);
class R012LaserTankBuster(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.R012LaserTankBuster, 6);
class R011LaserLine(BossModule module) : Components.SimpleAOEs(module, (uint)AID.R011LaserLine, new AOEShapeRect(70, 7.5f));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9646)]
public sealed class A24TheCompound2P(WorldState ws, Actor primary) : BossModule(ws, primary, new(200f, -700f), new ArenaBoundsSquare(30f))
{
    public Actor? BossP2;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        if (BossP2 == null)
        {
            var b = Enemies((uint)OID.Compound2P);
            BossP2 = b.Count != 0 ? b[0] : null;
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        switch (StateMachine.ActivePhaseIndex)
        {
            case -1:
            case 0:
                Arena.Actor(PrimaryActor);
                break;
            case 1:
                Arena.Actor(BossP2);
                break;
        }
    }
}
