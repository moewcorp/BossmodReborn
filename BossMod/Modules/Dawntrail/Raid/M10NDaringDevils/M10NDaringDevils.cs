using System.Linq;
using BossMod.Components;

namespace BossMod.DawnTrail.Raid.M10NDaringDevils;

// Setup dual boss module for Red Hot and Deep Blue, element here at end of page and rest on states.cs
// TODO: set correct IconID values if available

static class ActorExt
{
    public static bool HasStatus(this Actor a, uint sid) => a.FindStatus(sid) != null;
    public static bool HasIcon(this Actor a, uint iconID) => a.IncomingEffects.Any(e => e.Action.ID == iconID);
}

sealed class HotImpact : Components.CastSharedTankbuster
{
    public HotImpact(BossModule module) : base(module, (uint)AID.HotImpact, 6f) { }
}

sealed class DeepImpact : Components.GenericBaitAway
{
    public DeepImpact(BossModule module) : base(module, (uint)AID.DeepImpact, CenterAtTarget: true, tankbuster: true) { }

    public struct Bait(AOEShape shape)
    {
        public AOEShapeCircle Shape = new(6f);
    }
}

sealed class DiversDare : Components.RaidwideCast
{
    public DiversDare(BossModule module) : base(module, (uint)AID.DiversDare) { }
}

sealed class DeepVarial1 : Components.SimpleAOEs
{
    public DeepVarial1(BossModule module) : base(module, 0u, new AOEShapeCone(60f, 120.Degrees())) { }
}

sealed class SickestTakeOff1 : Components.SimpleAOEs
{
    public SickestTakeOff1(BossModule module) : base(module, 0u, new AOEShapeRect(50f, 15f)) { }
}

sealed class SickSwell1 : Components.SimpleKnockbacks
{
    public SickSwell1(BossModule module) : base(module, 0u, 10f, ignoreImmunes: false, maxCasts: int.MaxValue, shape: new AOEShapeRect(50f, 50f), kind: GenericKnockback.Kind.AwayFromOrigin, stopAtWall: true) { }


}

sealed class XtremeSpectacular3 : Components.RaidwideCast
{
    public XtremeSpectacular3(BossModule module) : base(module, (uint)AID.XtremeSpectacular3, hint: "Raidwide") { }
}

sealed class XtremeSpectacular4 : Components.RaidwideCast
{
    public XtremeSpectacular4(BossModule module) : base(module, (uint)AID.XtremeSpectacular4, hint: "Raidwide") { }
}

sealed class Pyrotation : Components.StackTogether
{
    public Pyrotation(BossModule module) : base(module, (uint)IconID.Pyrotation, 5f, 6f) { }
}

sealed class Pyrotation1 : Components.SimpleAOEs
{
    public Pyrotation1(BossModule module) : base(module, 0u, new AOEShapeCircle(6f)) { }

}


[ModuleInfo(BossModuleInfo.Maturity.WIP,
StatesType = typeof(M10NDaringDevilsStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = typeof(TetherID),
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.RedHot,
Contributors = "JoeSparkx",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.Raid,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1070u,
NameID = 14370u)]
[SkipLocalsInit]
public sealed class M10NDaringDevils : BossModule
{
    public M10NDaringDevils(WorldState ws, Actor primary) : base(ws, primary, new(100.125f, 102.163f), new ArenaBoundsSquare(20f)) { }

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
