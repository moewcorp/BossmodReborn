namespace BossMod.Endwalker.VariantCriterion.C01ASS.C011Silkie;

abstract class FizzlingDuster(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), C011Silkie.ShapeYellow);
class NFizzlingDuster(BossModule module) : FizzlingDuster(module, AID.NFizzlingDusterAOE);
class SFizzlingDuster(BossModule module) : FizzlingDuster(module, AID.SFizzlingDusterAOE);

abstract class DustBluster(BossModule module, AID aid) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(aid), 16f);
class NDustBluster(BossModule module) : DustBluster(module, AID.NDustBluster);
class SDustBluster(BossModule module) : DustBluster(module, AID.SDustBluster);

abstract class SqueakyCleanE(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(60f, 112.5f.Degrees()));
class NSqueakyCleanE(BossModule module) : SqueakyCleanE(module, AID.NSqueakyCleanAOE3E);
class SSqueakyCleanE(BossModule module) : SqueakyCleanE(module, AID.SSqueakyCleanAOE3E);

abstract class SqueakyCleanW(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(60f, 112.5f.Degrees()));
class NSqueakyCleanW(BossModule module) : SqueakyCleanW(module, AID.NSqueakyCleanAOE3W);
class SSqueakyCleanW(BossModule module) : SqueakyCleanW(module, AID.SSqueakyCleanAOE3W);

abstract class ChillingDusterPuff(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), C011Silkie.ShapeBlue);
class NChillingDusterPuff(BossModule module) : ChillingDusterPuff(module, AID.NChillingDusterPuff);
class SChillingDusterPuff(BossModule module) : ChillingDusterPuff(module, AID.SChillingDusterPuff);

abstract class BracingDusterPuff(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), C011Silkie.ShapeGreen);
class NBracingDusterPuff(BossModule module) : BracingDusterPuff(module, AID.NBracingDusterPuff);
class SBracingDusterPuff(BossModule module) : BracingDusterPuff(module, AID.SBracingDusterPuff);

abstract class FizzlingDusterPuff(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), C011Silkie.ShapeYellow);
class NFizzlingDusterPuff(BossModule module) : FizzlingDusterPuff(module, AID.NFizzlingDusterPuff);
class SFizzlingDusterPuff(BossModule module) : FizzlingDusterPuff(module, AID.SFizzlingDusterPuff);

public abstract class C011Silkie(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(-335f, -155f);
    public static readonly AOEShapeCross ShapeBlue = new(60f, 5f);
    public static readonly AOEShapeDonut ShapeGreen = new(5f, 60f);
    public static readonly AOEShapeCone ShapeYellow = new(60f, 22.5f.Degrees());
    public static readonly ArenaBoundsSquare StartingBounds = new(29.5f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20f);
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 878, NameID = 11369, SortOrder = 2, PlanLevel = 90)]
public class C011NSilkie(WorldState ws, Actor primary) : C011Silkie(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 879, NameID = 11369, SortOrder = 2, PlanLevel = 90)]
public class C011SSilkie(WorldState ws, Actor primary) : C011Silkie(ws, primary);
