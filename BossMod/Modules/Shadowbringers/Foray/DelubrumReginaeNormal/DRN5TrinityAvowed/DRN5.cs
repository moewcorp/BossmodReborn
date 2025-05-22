namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN5TrinityAvowed;

class WrathOfBozja(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.WrathOfBozja, new AOEShapeCone(60f, 45f.Degrees())); // TODO: verify angle

class ElementalImpact1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElementalImpact1, 20f);
class ElementalImpact2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElementalImpact2, 20f);
class GleamingArrow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GleamingArrow, new AOEShapeRect(60f, 5f));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 760, NameID = 9853)]
public class DRN5TrinityAvowed(WorldState ws, Actor primary) : TrinityAvowed(ws, primary);
