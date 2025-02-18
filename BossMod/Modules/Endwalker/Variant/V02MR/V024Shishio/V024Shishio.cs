namespace BossMod.Endwalker.VariantCriterion.V02MR.V024Shishio;

class NoblePursuit(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.NoblePursuit), 6);
class Enkyo(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Enkyo));

abstract class CloudToCloud : Components.SimpleAOEs
{
    protected CloudToCloud(BossModule module, AID aid, int halfWidth, int dangerCount) : base(module, ActionID.MakeSpell(aid), new AOEShapeRect(100, halfWidth)) { MaxDangerColor = dangerCount; }
}
class CloudToCloud1(BossModule module) : CloudToCloud(module, AID.CloudToCloud1, 1, 6);
class CloudToCloud2(BossModule module) : CloudToCloud(module, AID.CloudToCloud2, 3, 4);
class CloudToCloud3(BossModule module) : CloudToCloud(module, AID.CloudToCloud3, 6, 2);

abstract class Thunder(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 6);
class ThunderOnefold(BossModule module) : Thunder(module, AID.ThunderOnefold);
class ThunderTwofold(BossModule module) : Thunder(module, AID.ThunderTwofold);
class ThunderThreefold(BossModule module) : Thunder(module, AID.ThunderThreefold);

class SplittingCry(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(60, 7), (uint)IconID.Tankbuster, ActionID.MakeSpell(AID.SplittingCry), 5)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count != 0)
            hints.Add("Tankbuster cleave");
    }
}

class ThunderVortex(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ThunderVortex), new AOEShapeDonut(8, 30));

class Circles(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 6);
class UnsagelySpin(BossModule module) : Circles(module, AID.UnsagelySpin);
class Yoki(BossModule module) : Circles(module, AID.Yoki);

class Rush(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.Rush), 4);
class Vasoconstrictor(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Vasoconstrictor), 5);

class Swipe(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(40, 90.Degrees()));
class RightSwipe(BossModule module) : Swipe(module, AID.RightSwipe);
class LeftSwipe(BossModule module) : Swipe(module, AID.LeftSwipe);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12428, SortOrder = 5)]
public class V024Shishio(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, NormalBounds)
{
    public static readonly WPos ArenaCenter = new(-40, -300);
    public static readonly ArenaBoundsSquare NormalBounds = new(19.5f);
    public static readonly ArenaBoundsComplex CircleBounds = new([new Circle(ArenaCenter, 20)], [new Rectangle(ArenaCenter + new WDir(-20, 0), 0.5f, 20),
    new Rectangle(ArenaCenter + new WDir(20, 0), 0.5f, 20), new Rectangle(ArenaCenter + new WDir(0, 20), 20, 0.5f), new Rectangle(ArenaCenter + new WDir(0, -20), 20, 0.5f)]);
}
