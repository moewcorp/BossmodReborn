namespace BossMod.Stormblood.Extreme.Ex8Seiryu;

[ModuleInfo(BossModuleInfo.Maturity.WIP, PrimaryActorOID = (uint)OID.Seiryu, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 638, NameID = 7922, Category = BossModuleInfo.Category.Extreme, Expansion = BossModuleInfo.Expansion.Stormblood)]
public sealed class Ex8Seiryu(WorldState ws, Actor primary) : Trial.T09Seiryu.Seiryu(ws, primary);
