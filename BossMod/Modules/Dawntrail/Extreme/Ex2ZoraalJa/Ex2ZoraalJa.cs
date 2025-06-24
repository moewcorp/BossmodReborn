﻿namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

sealed class MultidirectionalDivide(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MultidirectionalDivide, new AOEShapeCross(30f, 2f));
sealed class MultidirectionalDivideMain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MultidirectionalDivideMain, new AOEShapeCross(30f, 4f));
sealed class MultidirectionalDivideExtra(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MultidirectionalDivideExtra, new AOEShapeCross(40f, 2f));
sealed class RegicidalRage(BossModule module) : Components.TankbusterTether(module, (uint)AID.RegicidalRageAOE, (uint)TetherID.RegicidalRage, 8f);
sealed class BitterWhirlwind(BossModule module) : Components.TankSwap(module, (uint)AID.BitterWhirlwind, (uint)AID.BitterWhirlwindAOEFirst, (uint)AID.BitterWhirlwindAOERest, 3.1f, new AOEShapeCircle(5f), true);
sealed class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningChains, (uint)AID.BurningChainsAOE);
sealed class HalfCircuitRect(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HalfCircuitAOERect, new AOEShapeRect(60f, 60f));
sealed class HalfCircuitDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HalfCircuitAOEDonut, new AOEShapeDonut(10f, 30f));
sealed class HalfCircuitCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HalfCircuitAOECircle, 10f);
sealed class DutysEdge(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.DutysEdgeTarget, (uint)AID.DutysEdgeAOE, 5.3f, 100f, 4f, 8, 8, 4, false);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 996, NameID = 12882, PlanLevel = 100)]
public sealed class Ex2ZoraalJa(WorldState ws, Actor primary) : Trial.T02ZoraalJa.ZoraalJa(ws, primary)
{
    private static readonly Angle a135 = 135f.Degrees();
    private static readonly WDir dir135 = 15f * a135.ToDirection();
    private static readonly WDir dirM135 = 15f * (-a135).ToDirection();

    public static readonly ArenaBoundsComplex NWPlatformBounds = new([new Square(ArenaCenter - dir135, 10f, a135), new Square(ArenaCenter + dir135, 10f, a135)], ScaleFactor: 1.24f);
    public static readonly ArenaBoundsComplex NEPlatformBounds = new([new Square(ArenaCenter - dirM135, 10f, -a135), new Square(ArenaCenter + dirM135, 10f, -a135)], ScaleFactor: 1.24f);
}
