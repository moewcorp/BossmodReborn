namespace BossMod.DawnTrail.Raid.M10NDaringDevils;


[ModuleInfo(BossModuleInfo.Maturity.WIP,
StatesType = typeof(M10NDaringDevilsStates),
ConfigType = null, // replace null with typeof(RedHotConfig) if applicable
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
TetherIDType = typeof(TetherID), // replace null with typeof(TetherID) if applicable
IconIDType = null, // replace null with typeof(IconID) if applicable
PrimaryActorOID = (uint)OID.RedHot,
Contributors = "JoeSparkx",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.Raid,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1070u,
NameID = 14370u)]
[SkipLocalsInit]
public sealed class M10NDaringDevils(WorldState ws, Actor primary) : BossModule(ws, primary, new(100.125f, 102.163f), new ArenaBoundsSquare(20f))
{
    public Actor? DeepBlue;

    protected override void UpdateModule()
    {
        DeepBlue ??= GetActor((uint)OID.DeepBlue);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(DeepBlue);
        Arena.Actor(PrimaryActor);
    }
}
