namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN6Queen;

class NorthswainsGlowPawnOff(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.NorthswainsGlowAOE, (uint)AID.PawnOffReal], 20f);
class GodsSaveTheQueen(BossModule module) : Components.RaidwideCast(module, (uint)AID.GodsSaveTheQueen);
class CleansingSlash(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CleansingSlash, "Tankbuster with doom");
class JudgmentBlade(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.JudgmentBladeL, (uint)AID.JudgmentBladeR], new AOEShapeRect(70f, 15f));
class OptimalPlaySword(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OptimalPlaySword, 10f);
class OptimalPlayShield(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OptimalPlayShield, new AOEShapeDonut(5f, 60f));
class Doom(BossModule module) : Components.CleansableDebuff(module, (uint)SID.Doom);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 760, NameID = 9863)]
public class DRN6Queen(WorldState ws, Actor primary) : Queen(ws, primary);
