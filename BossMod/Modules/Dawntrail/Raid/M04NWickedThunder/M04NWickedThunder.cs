namespace BossMod.Dawntrail.Raid.M04NWickedThunder;

class WickedJolt(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.WickedJolt), new AOEShapeRect(60, 2.5f), endsOnCastEvent: true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count != 0)
            hints.Add("Tankbuster cleave");
    }
}

class WickedBolt(BossModule module) : Components.StackWithIcon(module, (uint)IconID.WickedBolt, ActionID.MakeSpell(AID.WickedBolt), 5, 5, 8, 8, 5);
class SoaringSoulpress(BossModule module) : Components.StackWithIcon(module, (uint)IconID.SoaringSoulpress, ActionID.MakeSpell(AID.SoaringSoulpress), 6, 5.4f, 8, 8);
class WrathOfZeus(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.WrathOfZeus));
class BewitchingFlight(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BewitchingFlight), new AOEShapeRect(40, 2.5f));
class Thunderslam(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Thunderslam), 5);
class Thunderstorm(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Thunderstorm), 6);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 991, NameID = 13057)]
public class M04NWickedThunder(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChanges.DefaultCenter, ArenaChanges.DefaultBounds);
