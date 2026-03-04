namespace BossMod.Modules.Dawntrail.Advanced.Ad01TheMerchantsTale.Ad011PariofPlenty;

sealed class HeatBurst(BossModule module) : Components.RaidwideCast(module, (uint)AID.HeatBurst);

sealed class RightFireFlight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RightFireflight, new AOEShapeRect(40f, 2f));

[ModuleInfo(BossModuleInfo.Maturity.WIP,
StatesType = typeof(PariOfPlentyStates),
ConfigType = null, // replace null with typeof(PariOfPlentyConfig) if applicable
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
StatusIDType = typeof(AID), // replace null with typeof(SID) if applicable
TetherIDType = typeof(TetherID), // replace null with typeof(TetherID) if applicable
IconIDType = null, // replace null with typeof(IconID) if applicable
PrimaryActorOID = (uint)OID.PariOfPlenty,
Contributors = "HerStolenLight",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.Advanced,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1084u,
NameID = 14274u,
SortOrder = 1,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class PariOfPlenty(WorldState ws, Actor primary) : BossModule(ws, primary, new(-760f, -805f), new ArenaBoundsSquare(20f));
