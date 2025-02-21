namespace BossMod.Endwalker.VariantCriterion.V02MR.V024Shishio;

class NoblePursuit(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.NoblePursuit), 6f);
class Enkyo(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Enkyo));

abstract class CloudToCloud : Components.SimpleAOEs
{
    protected CloudToCloud(BossModule module, AID aid, float halfWidth, int dangerCount) : base(module, ActionID.MakeSpell(aid), new AOEShapeRect(100f, halfWidth)) { MaxDangerColor = dangerCount; }
}
class CloudToCloud1(BossModule module) : CloudToCloud(module, AID.CloudToCloud1, 1f, 6);
class CloudToCloud2(BossModule module) : CloudToCloud(module, AID.CloudToCloud2, 3f, 4);
class CloudToCloud3(BossModule module) : CloudToCloud(module, AID.CloudToCloud3, 6f, 2);

abstract class Thunder(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 6f);
class ThunderOnefold(BossModule module) : Thunder(module, AID.ThunderOnefold);
class ThunderTwofold(BossModule module) : Thunder(module, AID.ThunderTwofold);
class ThunderThreefold(BossModule module) : Thunder(module, AID.ThunderThreefold);

class SplittingCry(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(60f, 7f), (uint)IconID.Tankbuster, ActionID.MakeSpell(AID.SplittingCry), 5f)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count != 0)
            hints.Add("Tankbuster cleave");
    }
}

class ThunderVortex(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ThunderVortex), new AOEShapeDonut(8f, 30f));

class Circles(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 6f);
class UnsagelySpin(BossModule module) : Circles(module, AID.UnsagelySpin);
class Yoki(BossModule module) : Circles(module, AID.Yoki);

class Rush(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.Rush), 4f);
class Vasoconstrictor(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Vasoconstrictor), 5f);

class Swipe(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(40f, 90f.Degrees()));
class RightSwipe(BossModule module) : Swipe(module, AID.RightSwipe);
class LeftSwipe(BossModule module) : Swipe(module, AID.LeftSwipe);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12428, SortOrder = 5)]
public class V024Shishio(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, NormalBounds)
{
    public static readonly WPos ArenaCenter = new(-40f, -300f);
    public static readonly ArenaBoundsSquare NormalBounds = new(19.5f);
    public static readonly ArenaBoundsComplex CircleBounds = new([new Circle(ArenaCenter, 20f)], [new Rectangle(ArenaCenter + new WDir(-20f, default), 0.5f, 20f),
    new Rectangle(ArenaCenter + new WDir(20f, default), 0.5f, 20f), new Rectangle(ArenaCenter + new WDir(default, 20f), 20f, 0.5f), new Rectangle(ArenaCenter + new WDir(default, -20f), 20f, 0.5f)]);
}
