namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

class Teleport(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.Teleport));
class TeraSlash(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.TeraSlash));
class DoomArc(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DoomArc));
class UnbridledRage(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(100, 4), (uint)IconID.UnbridledRage, ActionID.MakeSpell(AID.UnbridledRageAOE), 5.9f);
class DarkNova(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.DarkNova), 6);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13653, SortOrder = 8, PlanLevel = 100)]
public class A14ShadowLord(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultBounds)
{
    private const int RadiusSmall = 8;
    private const int HalfWidth = 2;
    private const int Edges = 64;
    public static readonly WPos ArenaCenter = new(150f, 800f);
    public static readonly ArenaBoundsCircle DefaultBounds = new(30f);
    public static readonly Polygon[] Circles = [new(new(166.251f, 800f), RadiusSmall, Edges), new(new(133.788f, 800f), RadiusSmall, Edges),
    new(new(150f, 816.227f), RadiusSmall, Edges), new(new(150f, 783.812f), RadiusSmall, Edges)]; // the circle coordinates are not perfectly placed for some reason, got these from analyzing the collision data
    private static readonly RectangleSE[] rects = [new(Circles[1].Center, Circles[2].Center, HalfWidth), new(Circles[1].Center, Circles[3].Center, HalfWidth),
    new(Circles[3].Center, Circles[0].Center, HalfWidth), new(Circles[0].Center, Circles[2].Center, HalfWidth)];
    public static readonly Shape[] Combined = [.. Circles, .. rects];
    public static readonly ArenaBoundsComplex ComplexBounds = new(Combined);
}
