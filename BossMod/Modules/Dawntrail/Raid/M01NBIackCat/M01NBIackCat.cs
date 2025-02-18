namespace BossMod.Dawntrail.Raid.M01NBlackCat;

class BloodyScratch(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BloodyScratch));
class BiscuitMaker(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.BiscuitMaker));
class Clawful(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.Clawful), 5f, 8, 8);
class Shockwave(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Shockwave), 18, stopAfterWall: true);
class GrimalkinGale(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.GrimalkinGale), 5f);
class Overshadow(BossModule module) : Components.LineStack(module, ActionID.MakeSpell(AID.OverShadowMarker), ActionID.MakeSpell(AID.Overshadow), 5.3f, 60f, 2.5f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 985, NameID = 12686)]
public class M01NBlackCat(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChanges.ArenaCenter, ArenaChanges.DefaultBounds);
