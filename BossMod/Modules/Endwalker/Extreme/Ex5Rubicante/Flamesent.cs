﻿namespace BossMod.Endwalker.Extreme.Ex5Rubicante;

class GreaterFlamesent(BossModule module) : Components.Adds(module, (uint)OID.GreaterFlamesent);
class FlamesentNS(BossModule module) : Components.Adds(module, (uint)OID.FlamesentNS);
class FlamesentSS(BossModule module) : Components.Adds(module, (uint)OID.FlamesentSS);
class FlamesentNC(BossModule module) : Components.Adds(module, (uint)OID.FlamesentNC);
class GhastlyTorch(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.GhastlyTorch));
class ShatteringHeatAdd(BossModule module) : Components.TankbusterTether(module, ActionID.MakeSpell(AID.ShatteringHeatAdd), (uint)TetherID.ShatteringHeatAdd, 3f);
class GhastlyWind(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(40f, 15f.Degrees()), (uint)TetherID.GhastlyWind, ActionID.MakeSpell(AID.GhastlyWind)); // TODO: verify angle
class GhastlyFlame(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GhastlyFlameAOE), 5f);
