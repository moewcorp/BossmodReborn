namespace BossMod.Components;

// generic 'chasing AOE' component - these are AOEs that follow the target for a set amount of casts
public class GenericChasingAOEs(BossModule module, float moveDistance, ActionID aid = default, string warningText = "GTFO from chasing aoe!") : GenericAOEs(module, aid, warningText)
{
    private readonly float MoveDistance = moveDistance;

    public class Chaser(AOEShape shape, Actor target, WPos prevPos, float moveDist, int numRemaining, DateTime nextActivation, float secondsBetweenActivations)
    {
        public AOEShape Shape = shape;
        public Actor Target = target;
        public WPos PrevPos = prevPos;
        public float MoveDist = moveDist;
        public int NumRemaining = numRemaining;
        public DateTime NextActivation = nextActivation;
        public float SecondsBetweenActivations = secondsBetweenActivations;

        public WPos PredictedPosition()
        {
            var offset = Target.Position - PrevPos;
            var distance = offset.Length();
            var pos = distance > MoveDist ? PrevPos + MoveDist * offset / distance : Target.Position;
            return WPos.ClampToGrid(pos);
        }
    }

    public List<Chaser> Chasers = [];

    public bool IsChaserTarget(Actor? actor)
    {
        var count = Chasers.Count;
        for (var i = 0; i < count; ++i)
        {
            if (Chasers[i].Target == actor)
                return true;
        }
        return false;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Chasers.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var c = Chasers[i];
            var pos = c.PredictedPosition();
            var off = pos - c.PrevPos;
            aoes[i] = new(c.Shape, pos, off.LengthSq() > 0 ? Angle.FromDirection(off) : default, c.NextActivation);
        }
        return aoes;
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => Chasers.Any(c => c.Target == player) ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    // return false if chaser was not found
    public bool Advance(WPos pos, float moveDistance, DateTime currentTime, bool removeWhenFinished = true)
    {
        ++NumCasts;
        var c = Chasers.MinBy(c => (c.PredictedPosition() - pos).LengthSq());
        if (c == null)
            return false;

        if (--c.NumRemaining <= 0 && removeWhenFinished)
        {
            Chasers.Remove(c);
        }
        else
        {
            c.PrevPos = pos;
            c.MoveDist = moveDistance;
            c.NextActivation = currentTime.AddSeconds(c.SecondsBetweenActivations);
        }
        return true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Chasers.Count > 0)
        {
            AddForbiddenZones(actor, hints, true);
            AddForbiddenZones(actor, hints, false);
        }
    }

    private void AddForbiddenZones(Actor actor, AIHints hints, bool isTarget)
    {
        // sort of a hack to prevent the AI from getting "stuck" inside the AOE because all paths to safety have equal distance
        foreach (var c in Chasers.Where(c => c.Target == actor == isTarget))
        {
            var circle = (AOEShapeCircle)c.Shape;
            var radius = isTarget ? MoveDistance + circle.Radius : circle.Radius + 1;
            var position = isTarget ? c.PredictedPosition() - circle.Radius * actor.Rotation.ToDirection() : c.PredictedPosition();
            hints.AddForbiddenZone(ShapeDistance.Circle(position, radius), c.NextActivation);
        }
    }
}

// standard chasing aoe; first cast is long - assume it is baited on the nearest allowed target; successive casts are instant
public class StandardChasingAOEs(BossModule module, AOEShape shape, ActionID actionFirst, ActionID actionRest, float moveDistance, float secondsBetweenActivations, int maxCasts, bool resetExcludedTargets = false, uint icon = default, float activationDelay = 5.1f) : GenericChasingAOEs(module, moveDistance)
{
    public readonly AOEShape Shape = shape;
    public readonly ActionID ActionFirst = actionFirst;
    public readonly ActionID ActionRest = actionRest;
    public readonly float MoveDistance = moveDistance;
    public readonly float SecondsBetweenActivations = secondsBetweenActivations;
    public int MaxCasts = maxCasts;
    public BitMask ExcludedTargets; // any targets in this mask aren't considered to be possible targets
    public readonly uint Icon = icon;
    public readonly float ActivationDelay = activationDelay;
    public readonly bool ResetExcludedTargets = resetExcludedTargets;
    public readonly List<Actor> Actors = []; // to keep track of the icon before mechanic starts for handling custom forbidden zones
    public DateTime Activation;

    public override void Update()
    {
        var count = Chasers.Count;
        if (count == 0)
            return;

        for (var i = count - 1; i >= 0; --i)
        {
            var c = Chasers[i];
            if ((c.Target.IsDestroyed || c.Target.IsDead) && c.NumRemaining < MaxCasts)
                Chasers.RemoveAt(i);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = Chasers.Count;
        if (Chasers.Count == 0)
            return;

        for (var i = 0; i < count; ++i)
        {
            var c = Chasers[i];
            Arena.AddLine(c.PrevPos, c.Target.Position);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == ActionFirst)
        {
            var pos = spell.LocXZ;
            var (slot, target) = Raid.WithSlot().ExcludedFromMask(ExcludedTargets).MinBy(ip => (ip.Item2.Position - pos).LengthSq());
            if (target != null)
            {
                Actors.Remove(target);
                Chasers.Add(new(Shape, target, pos, 0, MaxCasts, Module.CastFinishAt(spell), SecondsBetweenActivations)); // initial cast does not move anywhere
                ExcludedTargets[slot] = true;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == ActionFirst || spell.Action == ActionRest)
        {
            var pos = spell.MainTargetID == caster.InstanceID ? caster.Position : WorldState.Actors.Find(spell.MainTargetID)?.Position ?? spell.TargetXZ;
            Advance(WPos.ClampToGrid(pos), MoveDistance, WorldState.CurrentTime);
            if (Chasers.Count == 0 && ResetExcludedTargets)
            {
                ExcludedTargets = default;
                NumCasts = 0;
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == Icon)
        {
            Activation = WorldState.FutureTime(ActivationDelay);
            Actors.Add(actor);
        }
    }
}

// since open world players don't count towards party, we need to make a new component
public abstract class OpenWorldChasingAOEs(BossModule module, AOEShape shape, ActionID actionFirst, ActionID actionRest, float moveDistance, float secondsBetweenActivations, int maxCasts, bool resetExcludedTargets = false, uint icon = default, float activationDelay = 5.1f) : StandardChasingAOEs(module, shape, actionFirst, actionRest, moveDistance, secondsBetweenActivations, maxCasts, resetExcludedTargets, icon, activationDelay)
{
    public new HashSet<Actor> ExcludedTargets = []; // any targets in this hashset aren't considered to be possible targets

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == ActionFirst)
        {
            var pos = spell.TargetID == caster.InstanceID ? caster.Position : WorldState.Actors.Find(spell.TargetID)?.Position ?? spell.LocXZ;
            Actor? target = null;
            var minDistanceSq = float.MaxValue;

            foreach (var actor in WorldState.Actors)
            {
                if (actor.OID == 0 && !ExcludedTargets.Contains(actor))
                {
                    var distanceSq = (actor.Position - pos).LengthSq();
                    if (distanceSq < minDistanceSq)
                    {
                        minDistanceSq = distanceSq;
                        target = actor;
                    }
                }
            }
            if (target != null)
            {
                Actors.Remove(target);
                Chasers.Add(new(Shape, target, pos, 0, MaxCasts, Module.CastFinishAt(spell), SecondsBetweenActivations)); // initial cast does not move anywhere
                ExcludedTargets.Add(target);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == ActionFirst || spell.Action == ActionRest)
        {
            var pos = spell.MainTargetID == caster.InstanceID ? caster.Position : WorldState.Actors.Find(spell.MainTargetID)?.Position ?? spell.TargetXZ;
            Advance(WPos.ClampToGrid(pos), MoveDistance, WorldState.CurrentTime);
            if (Chasers.Count == 0 && ResetExcludedTargets)
            {
                ExcludedTargets.Clear();
                NumCasts = 0;
            }
        }
    }
}
