namespace BossMod.Dawntrail.Raid.M03NBruteBomber;

class BrutalImpact(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BrutalImpactFirst));
class KnuckleSandwich(BossModule module) : Components.CastSharedTankbuster(module, ActionID.MakeSpell(AID.KnuckleSandwich), 6);

abstract class BrutalLariat(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(50, 17));
class BrutalLariat1(BossModule module) : BrutalLariat(module, AID.BrutalLariat1);
class BrutalLariat2(BossModule module) : BrutalLariat(module, AID.BrutalLariat2);

class MurderousMist(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MurderousMist), new AOEShapeCone(40, 135.Degrees()));
class BrutalBurn(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.BrutalBurn), 6, 8, 8);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 989, NameID = 13356)]
public class M03NBruteBomber(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(15));
