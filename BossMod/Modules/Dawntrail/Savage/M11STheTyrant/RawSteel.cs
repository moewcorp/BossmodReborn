namespace BossMod.Dawntrail.Savage.M11STheTyrant;

sealed class RawSteelTrophyAxe(BossModule module) : Components.GenericAOEs(module)
{
    private const float CircleRadius = 6f;   // Stack and spread are same size

    private static readonly AOEShapeCircle StackCircle = new(CircleRadius);
    private static readonly AOEShapeCircle SpreadCircle = new(CircleRadius);

    private Actor? _tankTarget;
    private readonly List<Actor> _spreadTargets = [];
    private readonly List<AOEInstance> _aoes = [];
    private DateTime _finish;

    public override void OnCastStarted(Actor caster, ActorCastInfo cast)
    {
        if (cast.Action.ID != (uint)AID.RawSteelTrophy1)
            return;

        _finish = Module.CastFinishAt(cast);
        _tankTarget = null;
        _spreadTargets.Clear();

        // --- Choose one tank for the stack (closest to boss) ---
        float bestDist = float.MaxValue;
        foreach (var p in Module.WorldState.Party.WithoutSlot())
        {
            if (p.IsDead || p.Role != Role.Tank)
                continue;

            float dist = (p.Position - caster.Position).LengthSq();
            if (dist < bestDist)
            {
                bestDist = dist;
                _tankTarget = p;
            }
        }

        // --- All non-tanks get spreads ---
        foreach (var p in Module.WorldState.Party.WithoutSlot())
        {
            if (!p.IsDead && p.Role != Role.Tank)
                _spreadTargets.Add(p);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RawSteel4 or (uint)AID.Impact)
        {
            _tankTarget = null;
            _spreadTargets.Clear();
            _aoes.Clear();
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_tankTarget == null && _spreadTargets.Count == 0)
            return default;

        _aoes.Clear();

        // --- Tank stack (SAFE) ---
        if (_tankTarget is { IsDead: false })
        {
            _aoes.Add(new(
                StackCircle,
                _tankTarget.Position,
                default,
                _finish,
                Colors.Safe
            ));
        }

        // --- Party spreads (DANGER) ---
        foreach (var p in _spreadTargets)
        {
            if (p.IsDead)
                continue;

            _aoes.Add(new(
                SpreadCircle,
                p.Position,
                default,
                _finish
            ));
        }

        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        // Tank stack hint
        if (_tankTarget == actor)
            hints.Add("Tank stack");

        // Spread hint for non-tanks
        if (actor.Role != Role.Tank)
            hints.Add("Spread");
    }
}
sealed class RawSteelTrophyScythe(BossModule module) : Components.GenericAOEs(module)
{
    private const float ConeLength = 60f;
    private static readonly Angle TankConeHalfAngle = 45.Degrees();
    private static readonly Angle HealerConeHalfAngle = 15.Degrees();
    private static readonly AOEShapeCone TankCone = new(ConeLength, TankConeHalfAngle);
    private static readonly AOEShapeCone HealerCone = new(ConeLength, HealerConeHalfAngle);

    private readonly List<Actor> _tankTargets = [];
    private Actor? _healerTarget;
    private readonly List<AOEInstance> _aoes = [];
    private DateTime _finish;

    public override void OnCastStarted(Actor caster, ActorCastInfo cast)
    {
        if (cast.Action.ID != (uint)AID.RawSteelTrophy)
            return;

        _finish = Module.CastFinishAt(cast);
        _tankTargets.Clear();
        _healerTarget = null;

        // tanks (assume MT + OT)
        foreach (var p in Module.WorldState.Party.WithoutSlot())
        {
            if (!p.IsDead && p.Role == Role.Tank)
                _tankTargets.Add(p);
        }

        // healer stack target: closest healer to boss
        float bestDist = float.MaxValue;
        foreach (var p in Module.WorldState.Party.WithoutSlot())
        {
            if (p.IsDead || p.Role != Role.Healer)
                continue;

            float dist = (p.Position - caster.Position).LengthSq();
            if (dist < bestDist)
            {
                bestDist = dist;
                _healerTarget = p;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RawSteel2 or (uint)AID.HeavyHitter)
        {
            _tankTargets.Clear();
            _healerTarget = null;
            _aoes.Clear();
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_tankTargets.Count == 0 && _healerTarget == null)
            return default;

        _aoes.Clear();
        var caster = Module.PrimaryActor;

        // tank cleaves (danger)
        foreach (var t in _tankTargets)
        {
            if (t.IsDead)
                continue;

            var dir = (t.Position - caster.Position).Normalized();
            _aoes.Add(new(
                TankCone,
                caster.Position,
                Angle.FromDirection(dir),
                _finish
            ));
        }

        // healer stack cone (SAFE)
        if (_healerTarget is { IsDead: false })
        {
            var dir = (_healerTarget.Position - caster.Position).Normalized();
            _aoes.Add(new(
                HealerCone,
                caster.Position,
                Angle.FromDirection(dir),
                _finish,
                Colors.Safe
            ));
        }

        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_healerTarget == null)
            return;

        // encourage stacking in healer cone
        var bossPos = Module.PrimaryActor.Position;
        var dir = Angle.FromDirection(
    (_healerTarget.Position - bossPos).Normalized());

        if (!actor.IsDead && (actor.Position - bossPos).LengthSq() <= ConeLength * ConeLength &&
        actor.Position.InCone(bossPos, dir, HealerConeHalfAngle))
        {
            hints.Add("Stack in healer cone");
        }
    }
}