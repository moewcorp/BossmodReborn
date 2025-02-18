namespace BossMod.Endwalker.Extreme.Ex6Golbez;

class Terrastorm(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TerrastormAOE), 16);
class LingeringSpark(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LingeringSparkAOE), 5);
abstract class PhasesOfTheBlade(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(22, 90.Degrees()));
class PhasesOfTheBladeFront(BossModule module) : PhasesOfTheBlade(module, AID.PhasesOfTheBlade);
class PhasesOfTheBladeBack(BossModule module) : PhasesOfTheBlade(module, AID.PhasesOfTheBladeBack);
class PhasesOfTheShadowFront(BossModule module) : PhasesOfTheBlade(module, AID.PhasesOfTheShadow);
class PhasesOfTheShadowBack(BossModule module) : PhasesOfTheBlade(module, AID.PhasesOfTheShadowBack);

class ArcticAssault(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ArcticAssaultAOE), new AOEShapeRect(15, 7.5f));
class RisingBeacon(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.RisingBeaconAOE), 10);
class RisingRing(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.RisingRingAOE), new AOEShapeDonut(6, 22));
class BurningShade(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.BurningShade), 5);
class ImmolatingShade(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.ImmolatingShade), 6, 4, 4);
class VoidBlizzard(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.VoidBlizzard), 6, 4, 4);
class VoidAero(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.VoidAero), 3, 2, 2);
class VoidTornado(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.VoidTornado), 6, 4, 4);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 950, NameID = 12365, PlanLevel = 90)]
public class Ex6Golbez(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(15));
