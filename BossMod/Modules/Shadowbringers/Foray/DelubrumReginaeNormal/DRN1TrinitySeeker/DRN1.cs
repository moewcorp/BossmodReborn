namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN1TrinitySeeker;

class MercifulBreeze(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MercifulBreeze), new AOEShapeRect(50, 2.5f));
class MercifulBlooms(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MercifulBlooms), 20);
class MercifulArc(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(12, 45.Degrees()), (uint)IconID.MercifulArc, ActionID.MakeSpell(AID.MercifulArc)); // TODO: verify angle

class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningChains, ActionID.MakeSpell(AID.ScorchingShackle));

class IronImpact(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.IronImpact));
class IronRose(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.IronRose), new AOEShapeRect(50, 4));

class DeadIron : Components.BaitAwayTethers
{
    public DeadIron(BossModule module) : base(module, new AOEShapeCone(50, 15.Degrees()), (uint)TetherID.DeadIron, ActionID.MakeSpell(AID.DeadIronAOE)) { DrawTethers = false; }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 760, NameID = 9834)]
public class DRN1TrinitySeeker(WorldState ws, Actor primary) : TrinitySeeker(ws, primary);
