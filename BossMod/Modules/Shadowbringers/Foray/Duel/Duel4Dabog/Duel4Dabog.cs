namespace BossMod.Shadowbringers.Foray.Duel.Duel4Dabog;

abstract class RightArmBlaster(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(100, 3));
class RightArmBlasterFragment(BossModule module) : RightArmBlaster(module, AID.RightArmBlasterFragment);
class RightArmBlasterBoss(BossModule module) : RightArmBlaster(module, AID.RightArmBlasterBoss);

class LeftArmSlash(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LeftArmSlash), new AOEShapeCone(10, 90.Degrees())); // TODO: verify angle
class LeftArmWave(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LeftArmWaveAOE), 24);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.BozjaDuel, GroupID = 778, NameID = 19)] // bnpcname=9958
public class Duel4Dabog(WorldState ws, Actor primary) : BossModule(ws, primary, new(250, 710), new ArenaBoundsCircle(20));
