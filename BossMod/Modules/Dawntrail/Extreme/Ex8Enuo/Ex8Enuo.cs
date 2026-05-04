namespace BossMod.Modules.Dawntrail.Extreme.Ex8Enuo;

sealed class Meteorain(BossModule module) : Components.RaidwideCast(module, (uint)AID.Meteorain);

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{

}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
StatesType = typeof(Ex8EnuoStates),
ConfigType = null, // replace null with typeof(EnuoConfig) if applicable
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
StatusIDType = null, // replace null with typeof(SID) if applicable
TetherIDType = null, // replace null with typeof(TetherID) if applicable
IconIDType = null, // replace null with typeof(IconID) if applicable
PrimaryActorOID = (uint)OID.Enuo,
Contributors = "HerStolenLight",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.Extreme,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1116u,
NameID = 14749u,
SortOrder = 1,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class Ex8Enuo(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f));

