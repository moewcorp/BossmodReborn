namespace BossMod.Dawntrail.Savage.M03SBruteBomber;

class BrutalImpact(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.BrutalImpactAOE));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 990, NameID = 13356, PlanLevel = 100)]
public class M03SBruteBomber(WorldState ws, Actor primary) : Raid.M03NBruteBomber.M03NBruteBomber(ws, primary);
