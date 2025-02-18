﻿namespace BossMod.Components;

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

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // future AOEs
        foreach (var s in Sequences)
        {
            var num = Math.Min(s.NumRemainingCasts, s.MaxShownAOEs);
            var rot = s.Rotation;
            var time = s.NextActivation > WorldState.CurrentTime ? s.NextActivation : WorldState.CurrentTime;
            for (var i = 1; i < num; ++i)
            {
                rot += s.Increment;
                time = time.AddSeconds(s.SecondsBetweenActivations);
                yield return new(s.Shape, s.Origin, rot, time, FutureColor);
            }
        }

        // imminent AOEs
        foreach (var s in Sequences)
            if (s.NumRemainingCasts > 0)
                yield return new(s.Shape, s.Origin, s.Rotation, s.NextActivation, ImminentColor);
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
        var index = Sequences.FindIndex(s => s.Origin.AlmostEqual(origin, 1) && s.Rotation.AlmostEqual(rotation, 0.05f));
        if (index < 0)
            return false;
        AdvanceSequence(index, currentTime, removeWhenFinished);
        return true;
    }
}
