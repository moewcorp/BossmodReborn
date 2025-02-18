﻿namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class DividingWings(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(60, 60.Degrees()), (uint)TetherID.DividingWings, ActionID.MakeSpell(AID.DividingWingsAOE));
class PandaemonsHoly(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.PandaemonsHoly), 36);

// note: origin seems to be weird?
class CirclesOfPandaemonium(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CirclesOfPandaemonium), new AOEShapeDonut(12, 40));

class Imprisonment(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ImprisonmentAOE), 4);
class Cannonspawn(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CannonspawnAOE), new AOEShapeDonut(3, 8));
class PealOfDamnation(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.PealOfDamnation), new AOEShapeRect(50, 3.5f));
class PandaemoniacPillars(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.Bury), 2);
class Touchdown(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TouchdownAOE), 20);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 939, NameID = 12354, PlanLevel = 90)]
public class P10SPandaemonium(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultArena.Center, DefaultArena)
{
    private static readonly Rectangle[] union = [new(new(100, 100), 13, 15), new(new(125, 85), 4, 15), new(new(75, 85), 4, 15)];
    private static readonly Rectangle[] bridgeL = [new(new(83, 92.5f), 4, 1)];
    private static readonly Rectangle[] bridgeR = [new(new(117, 92.5f), 4, 1)];
    public static readonly ArenaBoundsComplex DefaultArena = new(union);
    public static readonly ArenaBoundsComplex ArenaL = new([.. union, .. bridgeL]);
    public static readonly ArenaBoundsComplex ArenaR = new([.. union, .. bridgeR]);
    public static readonly ArenaBoundsComplex ArenaLR = new([.. union, .. bridgeL, .. bridgeR]);
}
