namespace BossMod.Endwalker.VariantCriterion.C01ASS.C013Shadowcaster;

abstract class FiresteelFracture(BossModule module, AID aid) : Components.Cleave(module, ActionID.MakeSpell(aid), new AOEShapeCone(40f, 30f.Degrees()));
class NFiresteelFracture(BossModule module) : FiresteelFracture(module, AID.NFiresteelFracture);
class SFiresteelFracture(BossModule module) : FiresteelFracture(module, AID.SFiresteelFracture);

abstract class PureFire(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 6f);
class NPureFire(BossModule module) : PureFire(module, AID.NPureFireAOE);
class SPureFire(BossModule module) : PureFire(module, AID.SPureFireAOE);

public abstract class C013Shadowcaster(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(289f, -105f);
    public static readonly ArenaBoundsRect StartingBounds = new(24.5f, 29.5f);
    public static readonly ArenaBoundsRect DefaultBounds = new(15f, 20f);
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 878, NameID = 11393, SortOrder = 5, PlanLevel = 90)]
public class C013NShadowcaster(WorldState ws, Actor primary) : C013Shadowcaster(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 879, NameID = 11393, SortOrder = 5, PlanLevel = 90)]
public class C013SShadowcaster(WorldState ws, Actor primary) : C013Shadowcaster(ws, primary);
