namespace BossMod.Endwalker.VariantCriterion.V01SildihnSubterrane.V12Silkie;

sealed class CarpetBeater(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CarpetBeater);
sealed class TotalWash(BossModule module) : Components.RaidwideCast(module, (uint)AID.TotalWash);
class DustBlusterKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.DustBlusterKnockback, 16f);
class WashOutKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.WashOutKnockback, 35f, kind: Kind.DirForward);

sealed class BracingDuster(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BracingDuster1, (uint)AID.BracingDuster2, (uint)AID.BracingDuster3], new AOEShapeDonut(5f, 60f));
sealed class ChillingDuster(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ChillingDuster1, (uint)AID.ChillingDuster2, (uint)AID.ChillingDuster3], new AOEShapeCross(60f, 5f));

sealed class SlipperySoap(BossModule module) : Components.ChargeAOEs(module, (uint)AID.SlipperySoap, 5f);
class SpotRemover(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpotRemover, 5f);

sealed class PuffAndTumble(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PuffAndTumble1, (uint)AID.PuffAndTumble2], 4f);

sealed class SqueakyCleanConeSmall(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SqueakyClean1E, (uint)AID.SqueakyClean2E,
(uint)AID.SqueakyClean1W, (uint)AID.SqueakyClean2W], new AOEShapeCone(60f, 45f.Degrees()));
sealed class SqueakyCleanConeBig(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SqueakyClean3E, (uint)AID.SqueakyClean3W], new AOEShapeCone(60f, 112.5f.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.Boss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 868, NameID = 11369, SortOrder = 2)]
public class V12Silkie(WorldState ws, Actor primary) : BossModule(ws, primary, new(-335f, -155f), new ArenaBoundsSquare(20f));
