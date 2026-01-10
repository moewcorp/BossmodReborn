using System.Linq;
using BossMod.Components;

namespace BossMod.DawnTrail.Raid.M10NDaringDevils;

// Setup dual boss module for Red Hot and Deep Blue, element here at end of page and rest on states.cs
// TODO: set correct IconID values if available


// Working -----------------------VVVV
sealed class HotImpact : Components.CastSharedTankbuster
{
    public HotImpact(BossModule module) : base(module, (uint)AID.HotImpact, 6f) { }
}

sealed class DeepImpact : Components.BaitAwayCast
{
    public DeepImpact(BossModule module) : base(module, (uint)AID.DeepImpact, 6f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster) { }
}
sealed class SickestTakeOff1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SickestTakeOff1, new AOEShapeRect(50f, 7.5f));

sealed class SickSwell1(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.SickSwell1, 10f, ignoreImmunes: false, maxCasts: int.MaxValue, kind: GenericKnockback.Kind.DirForward, stopAtWall: true);

sealed class DiversDare : Components.RaidwideCast
{
    public DiversDare(BossModule module) : base(module, (uint)AID.DiversDare) { }
}
// Needs Work -----------------------VVVV
// Deep Varial Cone AOE - Sort of works, need to tweak shape parameters
sealed class DeepVarial1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeepVarial1, new AOEShapeCone(20f, 120f.Degrees()));

// Xtreme Spectacular Raidwide - Needs to be split into 4 separate casts with different timings - maybe. Yet to test.
sealed class XtremeSpectacular3 : Components.RaidwideCast
{
    public XtremeSpectacular3(BossModule module) : base(module, (uint)AID.XtremeSpectacular3, hint: "Raidwide") { }
}

sealed class XtremeSpectacular4 : Components.RaidwideCast
{
    public XtremeSpectacular4(BossModule module) : base(module, (uint)AID.XtremeSpectacular4, hint: "Raidwide") { }
}
// Currently working on this bit ------------------------VVVV
sealed class PyrotationStack(BossModule module) : Components.StackTogether(module, (uint)AID.Pyrotation, activationDelay: 5.0f, radius: 6f);

sealed class PyrotationAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pyrotation1, new AOEShapeCircle(6f));

// Module -----------------------VVVV


[ModuleInfo(BossModuleInfo.Maturity.WIP,
StatesType = typeof(M10NDaringDevilsStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = typeof(TetherID),
IconIDType = null,
PrimaryActorOID = (uint)OID.RedHot,
Contributors = "JoeSparkx",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.Raid,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1070u,
NameID = 14370u)]
// Double Boss Info: Red Hot (Primary) + Deep Blue -----------VVVV
[SkipLocalsInit]
public sealed class M10NDaringDevils : BossModule
{
    public M10NDaringDevils(WorldState ws, Actor primary) : base(ws, primary, new(100.125f, 102.163f), new ArenaBoundsSquare(19f)) { }

    public Actor? DeepBlue;

    protected override void UpdateModule()
    {
        DeepBlue ??= GetActor((uint)OID.DeepBlue);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(DeepBlue);
        Arena.Actor(PrimaryActor);
    }
}
static class ActorExt
{
    public static bool HasStatus(this Actor a, uint sid) => a.FindStatus(sid) != null;
    public static bool HasIcon(this Actor a, uint iconID) => a.IncomingEffects.Any(e => e.Action.ID == iconID);
}