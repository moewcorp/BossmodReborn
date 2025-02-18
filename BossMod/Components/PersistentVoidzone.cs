// TODO: revise and refactor voidzone components; 'persistent' part of name is redundant
namespace BossMod.Components;

// voidzone (circle aoe that stays active for some time) centered at each existing object with specified OID, assumed to be persistent voidzone center
// for moving 'voidzones', the hints can mark the area in front of each source as dangerous
// TODO: typically sources are either eventobj's with eventstate != 7 or normal actors that are non dead; other conditions are much rarer
public class PersistentVoidzone(BossModule module, float radius, Func<BossModule, IEnumerable<Actor>> sources, float moveHintLength = 0) : GenericAOEs(module, default, "GTFO from voidzone!")
{
    public readonly AOEShape Shape = moveHintLength == 0 ? new AOEShapeCircle(radius) : new AOEShapeCapsule(radius, moveHintLength);
    public readonly Func<BossModule, IEnumerable<Actor>> Sources = sources;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = new List<AOEInstance>();
        foreach (var source in Sources(Module))
        {
            aoes.Add(new(Shape, WPos.ClampToGrid(source.Position), source.Rotation));
        }
        return aoes;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Sources(Module).Any())
            return;
        var forbidden = new List<Func<WPos, float>>();
        foreach (var s in Sources(Module))
            forbidden.Add(Shape.Distance(WPos.ClampToGrid(s.Position), s.Rotation));
        hints.AddForbiddenZone(ShapeDistance.Union(forbidden));
    }
}

// voidzone that appears with some delay at cast target
// note that if voidzone is predicted by cast start rather than cast event, we have to account for possibility of cast finishing without event (e.g. if actor dies before cast finish)
// TODO: this has problems when target moves - castevent and spawn position could be quite different
// TODO: this has problems if voidzone never actually spawns after castevent, eg because of phase changes
public class PersistentVoidzoneAtCastTarget(BossModule module, float radius, ActionID aid, Func<BossModule, IEnumerable<Actor>> sources, float castEventToSpawn) : GenericAOEs(module, aid, "GTFO from voidzone!")
{
    public readonly AOEShapeCircle Shape = new(radius);
    public readonly Func<BossModule, IEnumerable<Actor>> Sources = sources;
    public readonly float CastEventToSpawn = castEventToSpawn;
    private readonly List<(WPos pos, DateTime time)> _predictedByEvent = [];
    private readonly List<(Actor caster, DateTime time)> _predictedByCast = [];

    public bool HaveCasters => _predictedByCast.Count > 0;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var p in _predictedByEvent)
            yield return new(Shape, WPos.ClampToGrid(p.pos), default, p.time);
        foreach (var p in _predictedByCast)
            yield return new(Shape, p.caster.CastInfo!.LocXZ, default, p.time);
        foreach (var z in Sources(Module))
            yield return new(Shape, WPos.ClampToGrid(z.Position));
    }

    public override void Update()
    {
        if (_predictedByEvent.Count > 0)
            foreach (var s in Sources(Module))
                _predictedByEvent.RemoveAll(p => p.pos.InCircle(s.Position, 6));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _predictedByCast.Add((caster, Module.CastFinishAt(spell, CastEventToSpawn)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _predictedByCast.RemoveAll(p => p.caster == caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            _predictedByEvent.Add((spell.TargetXZ, WorldState.FutureTime(CastEventToSpawn)));
    }
}

// these are normal voidzones that could be 'inverted' (e.g. when you need to enter a voidzone at specific time to avoid some mechanic)
// TODO: i'm not sure whether these should be considered actual voidzones (if so, should i merge them with base component? what about cast prediction?) or some completely other type of mechanic (maybe drawing differently)
// TODO: might want to have per-player invertability
public class PersistentInvertibleVoidzone(BossModule module, float radius, Func<BossModule, IEnumerable<Actor>> sources, ActionID aid = default) : CastCounter(module, aid)
{
    public readonly AOEShapeCircle Shape = new(radius);
    public readonly Func<BossModule, IEnumerable<Actor>> Sources = sources;
    public DateTime InvertResolveAt;

    public bool Inverted => InvertResolveAt != default;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var inVoidzone = Sources(Module).Any(s => Shape.Check(actor.Position, s));
        if (Inverted)
            hints.Add(inVoidzone ? "Stay in voidzone" : "Go to voidzone!", !inVoidzone);
        else if (inVoidzone)
            hints.Add("GTFO from voidzone!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var shapes = new List<Func<WPos, float>>();

        foreach (var source in Sources(Module))
        {
            var shape = Shape.Distance(WPos.ClampToGrid(source.Position), source.Rotation);
            shapes.Add(shape);
        }
        if (shapes.Count == 0)
            return;

        hints.AddForbiddenZone(Inverted ? ShapeDistance.InvertedUnion(shapes) : ShapeDistance.Union(shapes), InvertResolveAt);
    }

    // TODO: reconsider - draw foreground circles instead?
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var color = Inverted ? Colors.SafeFromAOE : 0;
        foreach (var s in Sources(Module))
            Shape.Draw(Arena, s.Position, s.Rotation, color);
    }
}

// invertible voidzone that is inverted when specific spell is being cast; resolved when cast ends
public class PersistentInvertibleVoidzoneByCast(BossModule module, float radius, Func<BossModule, IEnumerable<Actor>> sources, ActionID aid) : PersistentInvertibleVoidzone(module, radius, sources, aid)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            InvertResolveAt = Module.CastFinishAt(spell);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            InvertResolveAt = default;
    }
}
