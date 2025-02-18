namespace BossMod.Dawntrail.Savage.M03SBruteBomber;

class BrutalImpact(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.BrutalImpactAOE));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 990, NameID = 13356, PlanLevel = 100)]
public class M03SBruteBomber(WorldState ws, Actor primary) : BossModule(ws, primary, arenaCenter, DefaultBounds)
{
    private static readonly WPos arenaCenter = new(100, 100);
    public static readonly ArenaBoundsSquare DefaultBounds = new(15);
    public static readonly ArenaBoundsComplex FuseFieldBounds = new([new Square(arenaCenter, 15)], [new Polygon(arenaCenter, 5, 80)]);
}
