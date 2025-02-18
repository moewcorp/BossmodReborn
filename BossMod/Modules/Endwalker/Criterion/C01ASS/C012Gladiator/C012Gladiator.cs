namespace BossMod.Endwalker.VariantCriterion.C01ASS.C012Gladiator;

abstract class RushOfMightFront(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(60f, 90f.Degrees()));
class NRushOfMightFront(BossModule module) : RushOfMightFront(module, AID.NRushOfMightFront);
class SRushOfMightFront(BossModule module) : RushOfMightFront(module, AID.SRushOfMightFront);

abstract class RushOfMightBack(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(60f, 90f.Degrees()));
class NRushOfMightBack(BossModule module) : RushOfMightBack(module, AID.NRushOfMightBack);
class SRushOfMightBack(BossModule module) : RushOfMightBack(module, AID.SRushOfMightBack);

public abstract class C012Gladiator(WorldState ws, Actor primary) : BossModule(ws, primary, new(-35f, -271f), new ArenaBoundsSquare(19.5f));

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 878, NameID = 11387, SortOrder = 4, PlanLevel = 90)]
public class C012NGladiator(WorldState ws, Actor primary) : C012Gladiator(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 879, NameID = 11387, SortOrder = 4, PlanLevel = 90)]
public class C012SGladiator(WorldState ws, Actor primary) : C012Gladiator(ws, primary);
