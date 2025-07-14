namespace BossMod.Shadowbringers.Alliance.A33RedGirl;

sealed class WaveWhite(BossModule module) : Components.CastHint(module, (uint)AID.WaveWhite, "Be white to avoid damage!");
sealed class WaveBlack(BossModule module) : Components.CastHint(module, (uint)AID.WaveBlack, "Be black to avoid damage!");
sealed class BigExplosion(BossModule module) : Components.CastHint(module, (uint)AID.BigExplosion, "Pylons explode!", true);
