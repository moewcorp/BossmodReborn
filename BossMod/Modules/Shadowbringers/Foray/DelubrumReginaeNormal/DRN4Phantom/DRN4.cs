namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN4Phantom;

class VileWave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VileWave, new AOEShapeCone(45f, 60f.Degrees()));
class CreepingMiasma(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CreepingMiasma, new AOEShapeRect(50f, 6f));
class MaledictionOfAgony(BossModule module) : Components.RaidwideCast(module, (uint)AID.MaledictionOfAgony);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 760, NameID = 9755)]
public class DRN4Phantom(WorldState ws, Actor primary) : Phantom(ws, primary);
