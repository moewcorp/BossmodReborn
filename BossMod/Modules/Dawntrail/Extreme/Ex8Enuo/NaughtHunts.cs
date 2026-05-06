namespace BossMod.Modules.Dawntrail.Extreme.Ex8Enuo;

// May want some additional signaling to the secondary target?  But this seems to work.

sealed class NaughtHunts(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(6f), (uint)AID.EndlessChaseCast, (uint)AID.EndlessChaseInstant, 2.9f, 0.7d, 12, icon: (uint)IconID.NaughtHuntTarget, activationDelay: 6d)
{
    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.NaughtHuntFirst)
        {
            var p = WorldState.Actors.Find(tether.Target);
            if (p != null)
            {
                Chasers.Add(new(Shape, p, source.Position, 0, MaxCasts, WorldState.FutureTime(ActivationDelay), SecondsBetweenActivations));
            }
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == ActionFirst)
        {
            var pos = spell.LocXZ;
            var minDistance = float.MaxValue;

            var count = Targets.Count;
            for (var i = 0; i < count; ++i)
            {
                var t = Targets[i];
                var distanceSq = (t.Position - pos).LengthSq();
                if (distanceSq < minDistance)
                {
                    minDistance = distanceSq;
                }
            }
            // Overriding to remove the 'add' behaviour here.
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell) // Don't count the first action so that we can have the same count between the two.
    {
        if (spell.Action.ID is var id && id == ActionRest)
        {
            var pos = spell.MainTargetID == caster.InstanceID ? caster.Position.Quantized() : WorldState.Actors.Find(spell.MainTargetID)?.Position ?? spell.TargetXZ;
            Advance(pos, MoveDistance, WorldState.CurrentTime);
            if (Chasers.Count == 0 && ResetTargets)
            {
                Targets.Clear();
                NumCasts = 0;
            }
        }
    }
}
