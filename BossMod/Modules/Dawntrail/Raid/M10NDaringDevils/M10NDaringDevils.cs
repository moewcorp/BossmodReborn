namespace BossMod.DawnTrail.Raid.M10NDaringDevils;
//WORKING LIST OF MECHANICS - Hotimpact, DeepImpact, DiversDare, CutbackBlaze Persistent AOEs, AlleyOopInferno Spread + Puddles, DeepVarial Cone, SickestTakeOff Line, SickSwell Knockback, Pyrotation Stack
//NOT WORKING - Cutback Blaze Bait (needs fixing), AlleyOopMaelstrom Cones (to be refined, maybe using maxCasts?), InsaneAir (not implemented yet), XtremeSpectacular, HotAerial, SteamBurst

// =========================
// Core mechanic components
// =========================

// Shared tankbuster (Red Hot -> players, 5s cast, range 6 circle)
sealed class HotImpact(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.HotImpact, 6f);

// Tankbuster bait (Deep Blue -> player, 5s cast, range 6 circle)
sealed class DeepImpact(BossModule module) : Components.BaitAwayCast(
    module,
    (uint)AID.DeepImpact,
    6f,
    centerAtTarget: true,
    endsOnCastEvent: true,
    tankbuster: true,
    damageType: AIHints.PredictedDamageType.Tankbuster);

// Raidwides
sealed class DiversDare(BossModule module) : Components.RaidwideCast(module, (uint)AID.DiversDare);
sealed class DiversDareBlue(BossModule module) : Components.RaidwideCast(module, (uint)AID.DiversDare1);

// Deep Varial cone (helper cast, 6.8s, range 60, 120-degree cone)
sealed class DeepVarialCone(BossModule module) : Components.SimpleAOEs(
    module,
    (uint)AID.DeepVarial1,
    new AOEShapeCone(60f, 60f.Degrees()));

// Sickest Take Off line (helper cast, range 50 width 15 rect)
sealed class SickestTakeOffLine(BossModule module) : Components.SimpleAOEs(
    module,
    (uint)AID.SickestTakeOff1,
    new AOEShapeRect(50f, 7.5f));

// Sick Swell: log says rect (range 50 width 50), 
sealed class SickSwellKB(BossModule module) : Components.SimpleKnockbacks(
    module,
    (uint)AID.SickSwell1,   // boss cast is the clean trigger
    distance: 15f,
    kind: Components.SimpleKnockbacks.Kind.DirForward,
    stopAtWall: true);
// Pyrotation stack marker (helper->players, no cast, range 6 circle).
sealed class PyrotationStack(BossModule module) : Components.StackWithIcon(
    module,
    (uint)IconID.FireStack, // 659
    (uint)AID.Pyrotation1,
    activationDelay: 5f,
    radius: 6f,
    minStackSize: 2,
    maxStackSize: 8);

    // Persistent puddles created when Pyrotation resolves; cleared by Divers' Dare.
sealed class PyrotationPuddles(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _puddles = [];
    private static readonly AOEShapeCircle Shape = new(6f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => CollectionsMarshal.AsSpan(_puddles);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Pyrotation1: // helper hit that corresponds to the stack resolving
            {
                foreach (var t in spell.Targets)
                {
                    var target = WorldState.Actors.Find(t.ID);
                    if (target != null)
                        _puddles.Add(new(Shape, target.Position, default, WorldState.CurrentTime, Colors.AOE, true));
                }
                break;
            }

            case (uint)AID.DiversDare:
            case (uint)AID.DiversDare1:
                _puddles.Clear();
                break;
        }
    }
}

// public class SimpleAOEs(BossModule module, uint aid, AOEShape shape, int maxCasts = int.MaxValue, double riskyWithSecondsLeft = default) : GenericAOEs(module, aid)
sealed class SteamBurst(BossModule module) : Components.SimpleAOEs(
    module,
    (uint)AID.SteamBurst,
    new AOEShapeCircle(9f));
// Xtreme Spectacular “no cast” hits (these appear as event casts).
// There isn’t a built-in “instant raidwide” component in this repo, so we count them and provide a global hint.
// Xtreme Spectacular edge - Proximity AOE so set it to 18 to mark the last safe spot in the 40-width rect.
sealed class XtremeSpectacularEdge(BossModule module) : Components.SimpleAOEs(
    module,
    (uint)AID.XtremeSpectacular2,
    new AOEShapeRect(50f, 18f));
sealed class XtremeSpectacularHits(BossModule module) : Components.CastCounterMulti(module, new uint[] { (uint)AID.XtremeSpectacular3, (uint)AID.XtremeSpectacular4 })
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        
        if (NumCasts > 0)
            hints.Add("Raidwide damage (Xtreme Spectacular)");
    }
}

// =========================
// Module
// =========================

[ModuleInfo(
    BossModuleInfo.Maturity.WIP,
    StatesType = typeof(M10NDaringDevilsStates),
    ConfigType = null,
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.RedHot,
    Contributors = "",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Raid,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1070u,
    NameID = 14370u)]
public sealed class M10NDaringDevils(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f))
{
    public Actor? DeepBlue { get; private set; }

    protected override void UpdateModule()
    {
        // cache secondary boss
        DeepBlue ??= GetActor((uint)OID.DeepBlue);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (DeepBlue != null)
            Arena.Actor(DeepBlue);
        Arena.Actor(PrimaryActor);
    }
}

static class ActorExt
{
    public static bool HasStatus(this Actor a, uint sid) => a.FindStatus(sid) != null;
    public static bool HasIcon(this Actor a, uint iconID) => a.IncomingEffects.Any(e => e.Action.ID == iconID);
}
