namespace BossMod.Components;

// generic 'rotating aoes' component - a sequence of aoes (typically cones) with same origin and increasing rotation
public class GenericRotatingAOE(BossModule module) : GenericAOEs(module)
{
    public record struct Sequence
    (
        AOEShape Shape,
        WPos Origin,
        Angle Rotation,
        Angle Increment,
        DateTime NextActivation,
        float SecondsBetweenActivations,
        int NumRemainingCasts,
        int MaxShownAOEs = 2
    );

    public readonly List<Sequence> Sequences = [];
    public virtual uint ImminentColor { get; set; } = Colors.Danger;
    public uint FutureColor = Colors.AOE;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Sequences.Count;
        if (count == 0)
            return [];

        var aoes = new List<AOEInstance>();
        var curTime = WorldState.CurrentTime;
        for (var j = 0; j < count; ++j)
        {
            var s = Sequences[j];
            var remaining = s.NumRemainingCasts;
            var num = Math.Min(remaining, s.MaxShownAOEs);
            var rot = s.Rotation;
            var nextAct = s.NextActivation;
            var time = nextAct > curTime ? nextAct : curTime;
            var shape = s.Shape;
            var origin = s.Origin;
            // future AOEs
            if (num > 0)
            {
                var timeBetween = s.SecondsBetweenActivations;
                var inc = s.Increment;

                for (var i = 1; i < num; ++i)
                {
                    rot += inc;
                    time = time.AddSeconds(timeBetween);
                    aoes.Add(new(shape, origin, rot, time, FutureColor));
                }
            }
            // imminent AOEs
            if (remaining != 0)
                aoes.Add(new(shape, origin, s.Rotation, nextAct, remaining > 1 ? ImminentColor : FutureColor));
        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    public void AdvanceSequence(int index, DateTime currentTime, bool removeWhenFinished = true)
    {
        ++NumCasts;

        if (index < 0 || index >= Sequences.Count)
            return;

        var s = Sequences[index];
        if (--s.NumRemainingCasts <= 0 && removeWhenFinished)
        {
            Sequences.RemoveAt(index);
        }
        else
        {
            s.Rotation += s.Increment;
            s.NextActivation = currentTime.AddSeconds(s.SecondsBetweenActivations);
            Sequences[index] = s;
        }
    }

    // return false if sequence was not found
    public bool AdvanceSequence(WPos origin, Angle rotation, DateTime currentTime, bool removeWhenFinished = true)
    {
        var count = Sequences.Count;
        for (var i = 0; i < count; ++i)
        {
            var s = Sequences[i];
            if (s.Origin.AlmostEqual(origin, 1f) && s.Rotation.AlmostEqual(rotation, 0.05f))
            {
                AdvanceSequence(i, currentTime, removeWhenFinished);
                return true;
            }
        }
        return false;
    }
}
