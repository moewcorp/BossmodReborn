namespace BossMod.Endwalker.Alliance.A34Eulogia;

class ByregotStrikeJump(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ByregotStrike), 8f);
class ByregotStrikeKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.ByregotStrikeKnockback), 20f);
class ByregotStrikeCone(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ByregotStrikeAOE), new AOEShapeCone(90f, 22.5f.Degrees()));
