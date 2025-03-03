﻿namespace BossMod;

// for raid members, we support both indexed and non-indexed enumeration
public static class ActorEnumeration
{
    // build a mask with set bits corresponding to slots in range
    public static BitMask Mask(this IEnumerable<(int, Actor)> range)
    {
        BitMask mask = new();
        foreach ((var i, _) in range)
            mask.Set(i);
        return mask;
    }

    // convert slot+actor range into actor range
    public static IEnumerable<Actor> Actors(this IEnumerable<(int, Actor)> range) => range.Select(indexActor => indexActor.Item2);

    // filter range with slot+actor by slot or by actor
    public static IEnumerable<(int, Actor)> WhereSlot(this IEnumerable<(int, Actor)> range, Func<int, bool> predicate)
    {
        return range.Where(indexActor => predicate(indexActor.Item1));
    }

    public static IEnumerable<(int, Actor)> WhereActor(this IEnumerable<(int, Actor)> range, Func<Actor, bool> predicate)
    {
        return range.Where(indexActor => predicate(indexActor.Item2));
    }

    // exclude specified actor from enumeration
    public static IEnumerable<Actor> Exclude(this IEnumerable<Actor> range, Actor? actor)
    {
        return range.Where(x => x != actor);
    }

    public static IEnumerable<(int, Actor)> Exclude(this IEnumerable<(int, Actor)> range, Actor? actor)
    {
        return range.WhereActor(x => x != actor);
    }

    public static IEnumerable<(int, Actor)> Exclude(this IEnumerable<(int, Actor)> range, int slot)
    {
        return range.WhereSlot(i => i != slot);
    }

    public static IEnumerable<Actor> Exclude(this IEnumerable<Actor> range, IEnumerable<Actor> actors)
    {
        var actorSet = new HashSet<Actor>(actors);
        return range.Where(x => !actorSet.Contains(x));
    }

    public static IEnumerable<(int, Actor)> Exclude(this IEnumerable<(int, Actor)> range, IEnumerable<Actor> actors)
    {
        var actorSet = new HashSet<Actor>(actors);
        return range.WhereActor(x => !actorSet.Contains(x));
    }

    // select actors that have their corresponding bit in mask set
    public static IEnumerable<(int, Actor)> IncludedInMask(this IEnumerable<(int, Actor)> range, BitMask mask)
    {
        return range.WhereSlot(i => mask[i]);
    }

    // select actors that have their corresponding bit in mask cleared
    public static IEnumerable<(int, Actor)> ExcludedFromMask(this IEnumerable<(int, Actor)> range, BitMask mask)
    {
        return range.WhereSlot(i => !mask[i]);
    }

    // select actors in specified radius from specified point
    public static IEnumerable<Actor> InRadius(this IEnumerable<Actor> range, WPos origin, float radius)
    {
        return range.Where(actor => actor.Position.InCircle(origin, radius));
    }

    public static IEnumerable<(int, Actor)> InRadius(this IEnumerable<(int, Actor)> range, WPos origin, float radius)
    {
        return range.WhereActor(actor => actor.Position.InCircle(origin, radius));
    }

    // select actors outside specified radius from specified point
    public static IEnumerable<Actor> OutOfRadius(this IEnumerable<Actor> range, WPos origin, float radius)
    {
        return range.Where(actor => !actor.Position.InCircle(origin, radius));
    }

    public static IEnumerable<(int, Actor)> OutOfRadius(this IEnumerable<(int, Actor)> range, WPos origin, float radius)
    {
        return range.WhereActor(actor => !actor.Position.InCircle(origin, radius));
    }

    // select actors in specified radius from specified actor, excluding actor itself
    public static IEnumerable<Actor> InRadiusExcluding(this IEnumerable<Actor> range, Actor origin, float radius)
    {
        return range.Exclude(origin).InRadius(origin.Position, radius);
    }

    public static IEnumerable<(int, Actor)> InRadiusExcluding(this IEnumerable<(int, Actor)> range, Actor origin, float radius)
    {
        return range.Exclude(origin).InRadius(origin.Position, radius);
    }

    // select actors in specified shape
    public static List<Actor> InShape(this IEnumerable<Actor> range, AOEShape shape, Actor origin)
    {
        List<Actor> result = [];

        foreach (var actor in range)
        {
            if (shape.Check(actor.Position, origin))
            {
                result.Add(actor);
            }
        }

        return result;
    }

    public static List<(int, Actor)> InShape(this IEnumerable<(int, Actor)> range, AOEShape shape, Actor origin)
    {
        List<(int, Actor)> result = [];

        foreach (var tuple in range)
        {
            if (shape.Check(tuple.Item2.Position, origin))
            {
                result.Add(tuple);
            }
        }

        return result;
    }

    public static List<Actor> InShape(this IEnumerable<Actor> range, AOEShape shape, WPos origin, Angle rotation)
    {
        List<Actor> result = [];

        foreach (var actor in range)
        {
            if (shape.Check(actor.Position, origin, rotation))
            {
                result.Add(actor);
            }
        }

        return result;
    }

    public static List<(int, Actor)> InShape(this IEnumerable<(int, Actor)> range, AOEShape shape, WPos origin, Angle rotation)
    {
        List<(int, Actor)> result = [];

        foreach (var tuple in range)
        {
            if (shape.Check(tuple.Item2.Position, origin, rotation))
            {
                result.Add(tuple);
            }
        }

        return result;
    }

    // select actors that have tether with specific ID
    public static IEnumerable<Actor> Tethered<ID>(this IEnumerable<Actor> range, ID id) where ID : Enum
    {
        var castID = (uint)(object)id;
        return range.Where(actor => actor.Tether.ID == castID);
    }

    public static IEnumerable<(int, Actor)> Tethered<ID>(this IEnumerable<(int, Actor)> range, ID id) where ID : Enum
    {
        var castID = (uint)(object)id;
        return range.WhereActor(actor => actor.Tether.ID == castID);
    }

    // sort range by distance from point
    public static IEnumerable<Actor> SortedByRange(this IEnumerable<Actor> range, WPos origin)
    {
        var actors = new List<(Actor actor, float distanceSq)>();

        foreach (var a in range)
        {
            var distanceSq = (a.Position - origin).LengthSq();
            actors.Add((a, distanceSq));
        }

        actors.Sort((a, b) => a.distanceSq.CompareTo(b.distanceSq));

        for (var i = 0; i < actors.Count; ++i)
        {
            yield return actors[i].actor;
        }
    }

    public static IEnumerable<(int, Actor)> SortedByRange(this IEnumerable<(int, Actor)> range, WPos origin)
    {
        var actors = new List<(int index, Actor actor, float distanceSq)>();

        foreach (var a in range)
        {
            var distanceSq = (a.Item2.Position - origin).LengthSq();
            actors.Add((a.Item1, a.Item2, distanceSq));
        }

        actors.Sort((a, b) => a.distanceSq.CompareTo(b.distanceSq));

        for (var i = 0; i < actors.Count; ++i)
        {
            var actor = actors[i];
            yield return (actor.index, actor.actor);
        }
    }

    // Find closest actor to a given point
    public static Actor? Closest(this IEnumerable<Actor> range, WPos origin)
    {
        Actor? closest = null;
        var minDistSq = float.MaxValue;

        foreach (var actor in range)
        {
            var distSq = (actor.Position - origin).LengthSq();
            if (distSq < minDistSq)
            {
                minDistSq = distSq;
                closest = actor;
            }
        }
        return closest;
    }

    // Find closest actor (with index) to a given point
    public static (int, Actor) Closest(this IEnumerable<(int, Actor)> range, WPos origin)
    {
        (int, Actor)? closest = null;
        var minDistSq = float.MaxValue;

        foreach (var (index, actor) in range)
        {
            var distSq = (actor.Position - origin).LengthSq();
            if (distSq < minDistSq)
            {
                minDistSq = distSq;
                closest = (index, actor);
            }
        }
        return closest!.Value;
    }

    // Find farthest actor from a given point
    public static Actor? Farthest(this IEnumerable<Actor> range, WPos origin)
    {
        Actor? farthest = null;
        var maxDistSq = float.MinValue;

        foreach (var actor in range)
        {
            var distSq = (actor.Position - origin).LengthSq();
            if (distSq > maxDistSq)
            {
                maxDistSq = distSq;
                farthest = actor;
            }
        }
        return farthest;
    }

    // count num actors matching and not matching a condition
    public static (int match, int mismatch) CountByCondition(this IEnumerable<Actor> range, Func<Actor, bool> condition)
    {
        int match = 0, mismatch = 0;
        foreach (var a in range)
        {
            if (condition(a))
                ++match;
            else
                ++mismatch;
        }
        return (match, mismatch);
    }

    // find the centroid of actor positions
    public static WPos PositionCentroid(this IEnumerable<Actor> range)
    {
        WDir sum = default;
        int count = 0;
        foreach (var a in range)
        {
            sum += a.Position.ToWDir();
            ++count;
        }
        if (count > 0)
            sum /= count;
        return sum.ToWPos();
    }

    public static (int, Actor)[] ExcludedFromMask(this List<(int, Actor)> range, BitMask mask)
    {
        var count = range.Count;
        var result = new List<(int, Actor)>(count);
        for (var i = 0; i < count; ++i)
        {
            var indexActor = range[i];
            if (!mask[indexActor.Item1])
            {
                result.Add(indexActor);
            }
        }
        return [.. result];
    }

    public static (int, Actor)[] WhereSlot(this List<(int, Actor)> range, Func<int, bool> predicate)
    {
        var count = range.Count;
        var result = new List<(int, Actor)>(count);
        for (var i = 0; i < count; ++i)
        {
            var indexActor = range[i];
            if (predicate(indexActor.Item1))
            {
                result.Add(indexActor);
            }
        }
        return [.. result];
    }

    public static (int, Actor)[] InRadius(this List<(int, Actor)> range, WPos origin, float radius)
    {
        var count = range.Count;
        var result = new List<(int, Actor)>(count);
        for (var i = 0; i < count; ++i)
        {
            var indexActor = range[i];
            if (indexActor.Item2.Position.InCircle(origin, radius))
            {
                result.Add(indexActor);
            }
        }
        return [.. result];
    }
}
