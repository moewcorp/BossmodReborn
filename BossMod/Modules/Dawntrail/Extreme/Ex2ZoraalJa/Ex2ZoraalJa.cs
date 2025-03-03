namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

class MultidirectionalDivide(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MultidirectionalDivide), new AOEShapeCross(30f, 2f));
class MultidirectionalDivideMain(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MultidirectionalDivideMain), new AOEShapeCross(30f, 4f));
class MultidirectionalDivideExtra(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MultidirectionalDivideExtra), new AOEShapeCross(40f, 2f));
class RegicidalRage(BossModule module) : Components.TankbusterTether(module, ActionID.MakeSpell(AID.RegicidalRageAOE), (uint)TetherID.RegicidalRage, 8f);
class BitterWhirlwind(BossModule module) : Components.TankSwap(module, ActionID.MakeSpell(AID.BitterWhirlwind), ActionID.MakeSpell(AID.BitterWhirlwindAOEFirst), ActionID.MakeSpell(AID.BitterWhirlwindAOERest), 3.1f, new AOEShapeCircle(5f), true);
class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningChains, ActionID.MakeSpell(AID.BurningChainsAOE));
class HalfCircuitRect(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HalfCircuitAOERect), new AOEShapeRect(60f, 60f));
class HalfCircuitDonut(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HalfCircuitAOEDonut), new AOEShapeDonut(10f, 30f));
class HalfCircuitCircle(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HalfCircuitAOECircle), 10f);
class DutysEdge(BossModule module) : Components.LineStack(module, ActionID.MakeSpell(AID.DutysEdgeTarget), ActionID.MakeSpell(AID.DutysEdgeAOE), 5.3f, 100f, 4f, 8, 8, 4, false);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 996, NameID = 12882, PlanLevel = 100)]
public class Ex2ZoraalJa(WorldState ws, Actor primary) : Trial.T02ZoraalJa.ZoraalJa(ws, primary)
{
    private static readonly Angle a135 = 135f.Degrees();
    private static readonly WDir dir135 = 15f * a135.ToDirection();
    private static readonly WDir dirM135 = 15f * (-a135).ToDirection();

    public static readonly ArenaBoundsComplex NWPlatformBounds = new([new Square(ArenaCenter - dir135, 10f, a135), new Square(ArenaCenter + dir135, 10f, a135)], ScaleFactor: 1.24f);
    public static readonly ArenaBoundsComplex NEPlatformBounds = new([new Square(ArenaCenter - dirM135, 10f, -a135), new Square(ArenaCenter + dirM135, 10f, -a135)], ScaleFactor: 1.24f);
}
