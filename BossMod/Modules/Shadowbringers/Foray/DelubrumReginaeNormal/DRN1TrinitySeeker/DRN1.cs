namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN1TrinitySeeker;

class MercifulBreeze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MercifulBreeze, new AOEShapeRect(50f, 2.5f));
class BalefulSwathe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BalefulSwathe, new AOEShapeCone(50f, 90f.Degrees()));
class ActOfMercy(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ActOfMercy, new AOEShapeCross(50f, 4f));
class MercifulBlooms(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MercifulBlooms, 20f);
class MercifulArc(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(12f, 45f.Degrees()), (uint)IconID.MercifulArc, (uint)AID.MercifulArc, tankbuster: true);

class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningChains, (uint)AID.ScorchingShackle);
class IronImpact(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.IronImpactMarker, (uint)AID.IronImpact, 5.9f);

class DeadIron(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(50f, 15f.Degrees()), (uint)IconID.DeadIron, (uint)AID.DeadIronAOE, 4.6d);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 760, NameID = 9834)]
public class DRN1TrinitySeeker(WorldState ws, Actor primary) : TrinitySeeker(ws, primary);
