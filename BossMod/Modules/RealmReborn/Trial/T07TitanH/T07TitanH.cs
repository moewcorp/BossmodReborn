﻿namespace BossMod.RealmReborn.Trial.T07TitanH;

public enum OID : uint
{
    Boss = 0xF7, // x1
    GraniteGaol = 0x5E1, // spawn during fight
    BombBoulder = 0x5A3, // spawn during fight
    TitansHeart = 0x5E4, // Part type, spawn during fight
    Helper = 0x1B2
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    RockBuster = 1360, // Boss->self, no cast, range 6+R 120-degree cone cleave
    Tumult = 1361, // Boss->self, no cast, multihit raidwide (2 hits until 1st geocrush, 3 hits until earthen fury, 4 hits until kill)
    WeightOfTheLand = 1362, // Boss->self, 2.0s cast, single-target, visual
    WeightOfTheLandAOE = 1363, // Helper->location, 2.5s cast, range 6 circle puddle
    Landslide = 1364, // Boss->self, 2.2s cast, range 35+R width 6 rect aoe with knockback 15
    Geocrush = 1365, // Boss->self, 3.0s cast, raidwide with ? falloff

    Bury = 1051, // BombBoulder->self, no cast, range 3+R circle, small damage when bomb appears
    Burst = 1052, // BombBoulder->self, 5.0s cast, range 5+R circle
    RockThrow = 645, // Boss->player, no cast, single-target, visual for granite gaol spawn
    GraniteSepulchre = 1477, // GraniteGaol->self, 15.0s cast, oneshot target if gaol not killed

    EarthenFury = 1366, // Boss->self, no cast, wipe if heart not killed, otherwise just a raidwide
    MountainBuster = 643 // Boss->self, no cast, range 16+R ?-degree cone cleave
}

class Hints(BossModule module) : BossComponent(module)
{
    private DateTime _heartSpawn;

    public override void AddGlobalHints(GlobalHints hints)
    {
        var heartExists = ((T07TitanH)Module).ActiveHeart.Any();
        if (_heartSpawn == default && heartExists)
        {
            _heartSpawn = WorldState.CurrentTime;
        }
        if (_heartSpawn != default && heartExists)
        {
            hints.Add($"Heart enrage in: {Math.Max(62 - (WorldState.CurrentTime - _heartSpawn).TotalSeconds, 0.0f):f1}s");
        }
    }
}

// also handles rockbuster, which is just a smaller cleave...
class MountainBuster(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.MountainBuster), new AOEShapeCone(21.25f, 60f.Degrees())); // TODO: verify angle

class WeightOfTheLand(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WeightOfTheLandAOE), 6f);
class Landslide(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Landslide), new AOEShapeRect(40.25f, 3f));

class Geocrush(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.Geocrush))
{
    private int _currentCast;
    private AOEShapeDonut? _outer;
    private AOEShapeCircle? _inner;
    private DateTime _innerFinish;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_outer != null)
            return new AOEInstance[1] { new(_outer, Arena.Center) };
        if (_inner != null)
            return new AOEInstance[1] { new(_inner, Arena.Center, new(), _innerFinish) };
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var outerRadius = ++_currentCast switch
            {
                1 => 23,
                2 => 20,
                3 => 15,
                _ => Arena.Bounds.Radius
            };
            _outer = new AOEShapeDonut(outerRadius, Arena.Bounds.Radius);
            _inner = new AOEShapeCircle(outerRadius - 2); // TODO: check falloff...
            _innerFinish = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _inner = null;
        }
    }
}

class Burst(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Burst), new AOEShapeCircle(6.3f))
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // pattern 1: one-by-one explosions every ~0.4-0.5s, 8 clockwise then center
        // pattern 2: center -> 4 cardinals at small offset ~1s later -> 4 intercardinals at bigger offset ~1s later
        // pattern 3: 3 in center line -> 3 in side line ~1.5s later -> 3 in other side line ~1.5s later
        // showing casts that end within 2.25s seems to deal with all patterns reasonably well
        var count = Casters.Count;
        if (count == 0)
            return [];

        var deadline = Casters[0].Activation.AddSeconds(2.25d);

        var index = 0;
        while (index < count && Casters[index].Activation < deadline)
            ++index;

        return CollectionsMarshal.AsSpan(Casters)[..index];
    }
}

class T07TitanHStates : StateMachineBuilder
{
    public T07TitanHStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hints>()
            .ActivateOnEnter<MountainBuster>()
            .ActivateOnEnter<WeightOfTheLand>()
            .ActivateOnEnter<Landslide>()
            .ActivateOnEnter<Geocrush>()
            .ActivateOnEnter<Burst>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 60, NameID = 1801)]
public class T07TitanH : BossModule
{
    private readonly List<Actor> _heart;
    public IEnumerable<Actor> ActiveHeart => _heart.Where(h => h.IsTargetable && !h.IsDead);

    public T07TitanH(WorldState ws, Actor primary) : base(ws, primary, default, new ArenaBoundsCircle(25))
    {
        _heart = Enemies((uint)OID.TitansHeart);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.GraniteGaol => 3,
                (uint)OID.TitansHeart => 2,
                (uint)OID.Boss => 1,
                _ => 0
            };
            e.AttackStrength = (OID)e.Actor.OID == OID.Boss ? e.Actor.HPMP.CurHP < 0.6f * e.Actor.HPMP.MaxHP ? 0.3f : 0.1f : 0;
        }
    }
}
