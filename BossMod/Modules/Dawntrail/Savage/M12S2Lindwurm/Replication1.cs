namespace BossMod.Dawntrail.Savage.M12S2Lindwurm;

sealed class SnakingKick(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SnakingKick, new AOEShapeCone(40, 90.Degrees()));

sealed class WingedScourge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WingedScourge, new AOEShapeCone(50, 15.Degrees()));

sealed class MightyMagicTopTierSlamFirstBait(BossModule module)
    : Components.UniformStackSpread(module, 5, 5, minStackSize: 2, maxStackSize: 2)
{
    record struct Caster(Actor Actor, bool Stack, DateTime Activation);

    readonly List<Caster> _casters = [];

    public int NumFire;
    public int NumDark;

    public override void Update()
    {
        Spreads.Clear();
        Stacks.Clear();

        var casters = CollectionsMarshal.AsSpan(_casters);
        var raid = Raid.WithoutSlot();
        var raidLen = raid.Length;

        for (var i = 0; i < casters.Length; ++i)
        {
            ref var entry = ref casters[i];
            var sourcePos = entry.Actor.Position.Quantized();
            var activation = entry.Activation;

            var needed = entry.Stack ? 1 : 2;

            Actor? first = null;
            Actor? second = null;
            var firstDist = float.MaxValue;
            var secondDist = float.MaxValue;

            for (var r = 0; r < raidLen; ++r)
            {
                var target = raid[r];
                var dist = (target.Position.Quantized() - sourcePos).LengthSq();

                if (dist < firstDist)
                {
                    second = first;
                    secondDist = firstDist;
                    first = target;
                    firstDist = dist;
                }
                else if (needed > 1 && dist < secondDist)
                {
                    second = target;
                    secondDist = dist;
                }
            }

            if (first != null)
            {
                if (entry.Stack)
                    AddStack(first, activation);
                else
                    AddSpread(first, activation);
            }

            if (!entry.Stack && second != null)
                AddSpread(second, activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TopTierSlamCast:
                _casters.Add(new(caster, true, Module.CastFinishAt(spell, 1)));
                break;

            case AID.MightyMagicCast:
                _casters.Add(new(caster, false, Module.CastFinishAt(spell, 1)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TopTierSlamStack:
                for (var i = _casters.Count - 1; i >= 0; --i)
                    if (_casters[i].Stack)
                        _casters.RemoveAt(i);
                ++NumFire;
                break;

            case AID.MightyMagicSpread:
                for (var i = _casters.Count - 1; i >= 0; --i)
                    if (!_casters[i].Stack)
                        _casters.RemoveAt(i);
                ++NumDark;
                break;
        }
    }
}

class Replication1SecondBait(BossModule module) : BossComponent(module)
{
    public enum Assignment
    {
        None,
        Fire,
        Dark
    }

    readonly Assignment[] _assignments = new Assignment[8];

    public record struct Clone(Actor Actor, Assignment Element);
    public readonly List<Clone> Clones = [];

    int _numFire;
    int _numDark;

    bool _highlightClone;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var assignment = _assignments[slot];
        if (assignment != Assignment.None)
            hints.Add($"Assignment: {assignment}", false);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_numFire >= 1 && _numDark >= 2)
            return;

        switch ((AID)spell.Action.ID)
        {
            case AID.TopTierSlamCast:
                Clones.Add(new(caster, Assignment.Fire));
                break;

            case AID.MightyMagicCast:
                Clones.Add(new(caster, Assignment.Dark));
                break;

            case AID.WingedScourgeCastVertical:
            case AID.WingedScourgeCastHorizontal:
                Clones.Add(new(caster, Assignment.None));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TopTierSlamStack:
                ++_numFire;
                Assign();
                break;

            case AID.MightyMagicSpread:
                ++_numDark;
                Assign();
                break;

            case AID.Dash:
                _highlightClone = true;
                break;
        }
    }

    public override void OnActorPlayActionTimelineSync(Actor actor, List<(ulong InstanceID, ushort ID)> events)
    {
        var clones = CollectionsMarshal.AsSpan(Clones);

        for (var i = 0; i < clones.Length; ++i)
        {
            if (clones[i].Actor == actor)
            {
                var element = clones[i].Element;
                Clones.RemoveAt(i);

                var evs = CollectionsMarshal.AsSpan(events);
                for (var e = 0; e < evs.Length; ++e)
                {
                    ref var ev = ref evs[e];
                    if (ev.ID == 0x11D3)
                    {
                        var child = WorldState.Actors.Find(ev.InstanceID);
                        if (child != null)
                            Clones.Add(new(child, element));
                    }
                }
                return;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var clones = CollectionsMarshal.AsSpan(Clones);
        var playerAssignment = _assignments[pcSlot];

        for (var i = 0; i < clones.Length; ++i)
        {
            ref var clone = ref clones[i];
            var element = clone.Element;

            if (element == Assignment.None)
                continue;

            uint color = 0;

            if (playerAssignment != Assignment.None)
            {
                if (element == playerAssignment)
                    color = Colors.Object;
            }
            else
            {
                color = element == Assignment.Fire
                    ? Colors.Object
                    : Colors.Vulnerable;
            }

            if (color != 0)
            {
                Arena.ActorInsideBounds(clone.Actor.Position, clone.Actor.Rotation, color);

                if (_highlightClone)
                    Arena.AddCircle(clone.Actor.Position, 1.25f, Colors.Danger);
            }
        }
    }

    void Assign()
    {
        if (_numFire != 1 || _numDark != 2)
            return;

        var party = Raid.WithSlot();
        var len = party.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var entry = ref party[i];
            var slot = entry.Item1;
            var actor = entry.Item2;

            var hasDarkRes = actor.FindStatus(SID.DarkResistanceDownII, DateTime.MinValue) != null;
            _assignments[slot] = hasDarkRes ? Assignment.Fire : Assignment.Dark;
        }
    }
}

class WingedScourgeSecond(BossModule module) : Components.GenericAOEs(module)
{
    static readonly AOEShapeCone _shape = new(50, 15.Degrees());

    Angle _orientation;
    bool _recorded;
    bool _draw;

    readonly List<Actor> _clones = [];
    readonly List<(WPos Source, DateTime Activation)> _sources = [];
    readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_draw || _sources.Count == 0)
            return [];

        _aoes.Clear();

        var sources = CollectionsMarshal.AsSpan(_sources);
        var count = sources.Length;

        for (var i = 0; i < count; ++i)
        {
            ref var s = ref sources[i];

            _aoes.Add(new AOEInstance(_shape, s.Source, _orientation, s.Activation));
            _aoes.Add(new AOEInstance(_shape, s.Source, _orientation + 180.Degrees(), s.Activation));
        }

        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var aid = (AID)spell.Action.ID;

        if (!_recorded && (aid == AID.WingedScourgeCastVertical || aid == AID.WingedScourgeCastHorizontal))
        {
            _clones.Add(caster);
            _orientation = aid == AID.WingedScourgeCastVertical
                ? default
                : 90.Degrees();
        }
    }

    public override void OnActorPlayActionTimelineSync(Actor actor, List<(ulong InstanceID, ushort ID)> events)
    {
        var clones = CollectionsMarshal.AsSpan(_clones);

        for (var i = 0; i < clones.Length; ++i)
        {
            if (clones[i] == actor)
            {
                _clones.RemoveAt(i);
                _recorded = true;

                var evs = CollectionsMarshal.AsSpan(events);
                for (var e = 0; e < evs.Length; ++e)
                {
                    ref var ev = ref evs[e];
                    if (ev.ID == 0x11D3)
                    {
                        var clone = WorldState.Actors.Find(ev.InstanceID);
                        if (clone != null)
                            _clones.Add(clone);
                    }
                }
                return;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.Dash)
            return;

        var clones = CollectionsMarshal.AsSpan(_clones);

        for (var i = 0; i < clones.Length; ++i)
        {
            if (clones[i] == caster)
            {
                _draw = true;
                _sources.Add((spell.TargetXZ, WorldState.FutureTime(7.2f)));
                return;
            }
        }
    }
}
class MightyMagicTopTierSlamSecondBait(BossModule module)
    : Components.UniformStackSpread(module, 5, 5, minStackSize: 2, maxStackSize: 2)
{
    readonly Replication1SecondBait _rep1Bait = module.FindComponent<Replication1SecondBait>()!;

    record struct Source(WPos Position, bool Stack, DateTime Activation);

    readonly List<Source> _sources = [];

    public int NumFire;
    public int NumDark;

    BitMask _fireVuln;
    BitMask _darkVuln;

    public override void Update()
    {
        Spreads.Clear();
        Stacks.Clear();

        var sources = CollectionsMarshal.AsSpan(_sources);
        var raid = Raid.WithoutSlot();
        var raidLen = raid.Length;

        for (var i = 0; i < sources.Length; ++i)
        {
            ref var s = ref sources[i];

            var origin = s.Position.Quantized();
            var activation = s.Activation;
            var needed = s.Stack ? 1 : 2;

            Actor? first = null;
            Actor? second = null;
            var firstDist = float.MaxValue;
            var secondDist = float.MaxValue;

            for (var r = 0; r < raidLen; ++r)
            {
                var target = raid[r];
                var dist = (target.Position.Quantized() - origin).LengthSq();

                if (dist < firstDist)
                {
                    second = first;
                    secondDist = firstDist;
                    first = target;
                    firstDist = dist;
                }
                else if (needed > 1 && dist < secondDist)
                {
                    second = target;
                    secondDist = dist;
                }
            }

            if (first != null)
            {
                if (s.Stack)
                    AddStack(first, activation, _fireVuln);
                else
                    AddSpread(first, activation);
            }

            if (!s.Stack && second != null)
                AddSpread(second, activation);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_fireVuln[slot] && IsStackTarget(actor))
            hints.Add("Avoid baiting fire!");

        if (_darkVuln[slot] && IsSpreadTarget(actor))
            hints.Add("Avoid baiting dark!");
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot < 0)
            return;

        switch ((SID)status.ID)
        {
            case SID.FireResistanceDownII:
                _fireVuln.Set(slot);
                break;

            case SID.DarkResistanceDownII:
                _darkVuln.Set(slot);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Dash:
                {
                    var clones = CollectionsMarshal.AsSpan(_rep1Bait.Clones);

                    for (var i = 0; i < clones.Length; ++i)
                    {
                        ref var clone = ref clones[i];
                        if (clone.Actor == caster && clone.Element != Replication1SecondBait.Assignment.None)
                        {
                            var isStack = clone.Element == Replication1SecondBait.Assignment.Fire;
                            _sources.Add(new(spell.TargetXZ, isStack, WorldState.FutureTime(10))); // FIXME timing
                            break;
                        }
                    }
                    break;
                }

            case AID.TopTierSlamStack:
                ++NumFire;
                for (var i = _sources.Count - 1; i >= 0; --i)
                    if (_sources[i].Stack)
                        _sources.RemoveAt(i);
                break;

            case AID.MightyMagicSpread:
                ++NumDark;
                for (var i = _sources.Count - 1; i >= 0; --i)
                    if (!_sources[i].Stack)
                        _sources.RemoveAt(i);
                break;
        }
    }
}

sealed class DoubleSobatBuster(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(40, 90.Degrees()), (uint)IconID.DoubleSobat)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DoubleSobatBuster1:
            case AID.DoubleSobatBuster2:
            case AID.DoubleSobatBuster3:
            case AID.DoubleSobatBuster4:
                ++NumCasts;
                CurrentBaits.Clear();
                break;
        }
    }
}

sealed class DoubleSobatRepeat(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DoubleSobatRepeat, new AOEShapeCone(40, 90.Degrees()));

sealed class EsotericFinisher : Components.GenericBaitAway
{
    static readonly AOEShapeCircle _shape = new(10);

    public EsotericFinisher(BossModule module)
        : base(module, (uint)AID.EsotericFinisher, centerAtTarget: true)
    {
        var party = module.Raid.WithoutSlot();
        var len = party.Length;

        Actor? first = null;
        Actor? second = null;

        // First pass: pick tanks
        for (var i = 0; i < len; ++i)
        {
            var p = party[i];
            if (p.Role == Role.Tank)
            {
                if (first == null)
                    first = p;
                else
                {
                    second = p;
                    break;
                }
            }
        }

        if (second == null)
        {
            for (var i = 0; i < len; ++i)
            {
                var p = party[i];

                if (p == first)
                    continue;

                if (first == null)
                {
                    first = p;
                }
                else
                {
                    second = p;
                    break;
                }
            }
        }

        var activation = WorldState.FutureTime(10);

        if (first != null)
            CurrentBaits.Add(new(module.PrimaryActor, first, _shape, activation));

        if (second != null)
            CurrentBaits.Add(new(module.PrimaryActor, second, _shape, activation));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.EsotericFinisher)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}