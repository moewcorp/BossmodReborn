namespace BossMod.Dawntrail.Savage.M01SBlackCat;

class BiscuitMaker(BossModule module) : Components.TankSwap(module, ActionID.MakeSpell(AID.BiscuitMaker), ActionID.MakeSpell(AID.BiscuitMaker), ActionID.MakeSpell(AID.BiscuitMakerSecond), 2, null, true);
class QuadrupleSwipeBoss(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.QuadrupleSwipeBossAOE), 4, 2, 2);
class DoubleSwipeBoss(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DoubleSwipeBossAOE), 5, 4, 4);
class QuadrupleSwipeShade(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.QuadrupleSwipeShadeAOE), 4, 2, 2);
class DoubleSwipeShade(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DoubleSwipeShadeAOE), 5, 4, 4);
class Nailchipper(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.NailchipperAOE), 5);
class TempestuousTear(BossModule module) : Components.LineStack(module, ActionID.MakeSpell(AID.TempestuousTearTargetSelect), ActionID.MakeSpell(AID.TempestuousTearAOE), 5, 100, 3, 1, 4);
class Overshadow(BossModule module) : Components.LineStack(module, ActionID.MakeSpell(AID.OvershadowTargetSelect), ActionID.MakeSpell(AID.OvershadowAOE), 5.1f, 100, 2.5f, 7, 8);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 986, NameID = 12686, PlanLevel = 100)]
public class M01SBlackCat(WorldState ws, Actor primary) : Raid.M01NBlackCat.M01NBlackCat(ws, primary);
