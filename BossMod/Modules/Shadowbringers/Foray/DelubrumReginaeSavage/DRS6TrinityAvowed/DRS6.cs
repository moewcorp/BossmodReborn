namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS6TrinityAvowed;

class WrathOfBozja(BossModule module) : Components.CastSharedTankbuster(module, ActionID.MakeSpell(AID.WrathOfBozja), new AOEShapeCone(60f, 45f.Degrees())); // TODO: verify angle
class WrathOfBozjaBow(BossModule module) : Components.CastSharedTankbuster(module, ActionID.MakeSpell(AID.WrathOfBozjaBow), new AOEShapeCone(60f, 45f.Degrees())); // TODO: verify angle

// note: it is combined with different AOEs (bow1, bow2, staff1)
class QuickMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace);

class ElementalImpact1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ElementalImpact1), 20f);
class ElementalImpact2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ElementalImpact2), 20f);
class GleamingArrow(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GleamingArrow), new AOEShapeRect(60f, 5f));

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 761, NameID = 9853, PlanLevel = 80)]
public class DRS6(WorldState ws, Actor primary) : BossModule(ws, primary, new(-272, -82), new ArenaBoundsSquare(25));
