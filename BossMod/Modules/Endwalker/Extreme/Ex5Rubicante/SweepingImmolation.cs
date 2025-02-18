namespace BossMod.Endwalker.Extreme.Ex5Rubicante;

abstract class SweepingImmolation(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(20, 90.Degrees()));
class SweepingImmolationSpread(BossModule module) : SweepingImmolation(module, AID.SweepingImmolationSpread);
class SweepingImmolationStack(BossModule module) : SweepingImmolation(module, AID.SweepingImmolationStack);

class PartialTotalImmolation(BossModule module) : Components.CastStackSpread(module, ActionID.MakeSpell(AID.TotalImmolation), ActionID.MakeSpell(AID.PartialImmolation), 6, 5, 8, 8, true);
class ScaldingSignal(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ScaldingSignal), 10);
class ScaldingRing(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ScaldingRing), new AOEShapeDonut(10, 20));
class ScaldingFleetFirst(BossModule module) : Components.BaitAwayEveryone(module, module.PrimaryActor, new AOEShapeRect(40, 3), ActionID.MakeSpell(AID.ScaldingFleetFirst));

// note: it seems to have incorrect target, but acts like self-targeted
class ScaldingFleetSecond(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ScaldingFleetSecond), new AOEShapeRect(60, 3));
