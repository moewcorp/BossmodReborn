namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE44FamiliarFace;

public enum OID : uint
{
    Boss = 0x2DD2, // R9.45
    PhantomHashmal = 0x3321, // R9.45
    ArenaFeatures = 0x1EA1A1, // R2.0
    Tower = 0x1EB17E, // R0.5
    FallingTower = 0x1EB17D, // R0.5, rotation at spawn determines fall direction?..
    Hammer = 0x1EB17F, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    TectonicEruption = 23826, // Helper->location, 4.0s cast, range 6 circle puddle
    RockCutter = 23827, // Boss->player, 5.0s cast, single-target, tankbuster
    AncientQuake = 23828, // Boss->self, 5.0s cast, single-target, visual
    AncientQuakeAOE = 23829, // Helper->self, no cast, ??? (x2 ~0.8s after visual)
    Sanction = 23817, // Boss->self, no cast, single-target, visual (light raidwide)
    SanctionAOE = 23832, // Helper->self, no cast, ??? (x2 ~0.8s after visual)
    Roxxor = 23823, // Helper->players, 5.0s cast, range 6 circle spread

    ControlTowerAppear = 23830, // Helper->self, 4.0s cast, range 6 circle aoe around appearing towers
    TowerRound = 23831, // Boss->self, 4.0s cast, single-target, visual (spawns 2 towers + light raidwide)
    TowerRoundAOE = 23834, // Helper->self, no cast, ??? (x2 ~0.8s after visual)
    ControlTower = 23816, // Boss->self, 4.0s cast, single-target, visual (spawns 3 towers + light raidwide)
    ControlTowerAOE = 23833, // Helper->self, no cast, ??? (x2 ~0.8s after visual)
    Towerfall = 23818, // Helper->self, 7.0s cast, range 40 width 10 rect

    PhantomOrder = 24702, // Boss->self, 4.0s cast, single-target, visual
    ExtremeEdgeR = 23821, // PhantomHashmal->self, 8.0s cast, range 60 width 36 rect
    ExtremeEdgeL = 23822, // PhantomHashmal->self, 8.0s cast, range 60 width 36 rect

    IntractableLand = 24576, // Boss->self, 5.0s cast, single-target, visual (double exaflares)
    IntractableLandFirst = 23819, // Helper->self, 5.3s cast, range 8 circle
    IntractableLandRest = 23820, // Helper->location, no cast, range 8 circle

    HammerRound = 23824, // Boss->self, 5.0s cast, single-target, visual
    Hammerfall = 23825 // Helper->self, 8.0s cast, range 37 circle
}

class TectonicEruption(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TectonicEruption, 6f);
class RockCutter(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.RockCutter);
class AncientQuake(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.AncientQuake, (uint)AID.AncientQuakeAOE, 0.8f);
class Roxxor(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Roxxor, 6f);
class ControlTowerAppear(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ControlTowerAppear, 6f);

// note: we could predict aoes way in advance, when FallingTower actors are created - they immediately have correct rotation
// if previous cast was TowerRound, delay is ~24.4s; otherwise if previous cast was ControlTower, delay is ~9.6s; otherwise it is ~13s
// however, just watching casts normally gives more than enough time to avoid aoes and does not interfere with mechanics that resolve earlier
class Towerfall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Towerfall, new AOEShapeRect(40f, 5f));

class ExtremeEdge(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ExtremeEdgeL, (uint)AID.ExtremeEdgeR], new AOEShapeRect(60f, 18f));

class IntractableLand(BossModule module) : Components.Exaflare(module, 8f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.IntractableLandFirst)
        {
            Lines.Add(new() { Next = caster.Position, Advance = 8f * spell.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 0.8f, ExplosionsLeft = 8, MaxShownExplosions = 4 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.IntractableLandFirst or (uint)AID.IntractableLandRest)
        {
            var count = Lines.Count;
            var pos = spell.Action.ID == (uint)AID.IntractableLandFirst ? caster.Position : spell.TargetXZ;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
            ReportError($"Failed to find entry for {caster.InstanceID:X}");
        }
    }
}

class Hammerfall(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);
    private static readonly AOEShapeCircle circle = new(37f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Hammer)
            _aoes.Add(new(circle, WPos.ClampToGrid(actor.Position), default, WorldState.FutureTime(12.6d)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.Hammerfall)
            _aoes.RemoveAt(0);
    }
}

class CE44FamiliarFaceStates : StateMachineBuilder
{
    public CE44FamiliarFaceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TectonicEruption>()
            .ActivateOnEnter<RockCutter>()
            .ActivateOnEnter<AncientQuake>()
            .ActivateOnEnter<Roxxor>()
            .ActivateOnEnter<ControlTowerAppear>()
            .ActivateOnEnter<Towerfall>()
            .ActivateOnEnter<ExtremeEdge>()
            .ActivateOnEnter<IntractableLand>()
            .ActivateOnEnter<Hammerfall>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 778, NameID = 29)] // bnpcname=9693
public class CE44FamiliarFace(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(330f, 390f), 29.5f, 32)]);

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 30f);
}
