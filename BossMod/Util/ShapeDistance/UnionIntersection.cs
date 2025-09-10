namespace BossMod;

public sealed class SDIntersection : ShapeDistance // max distance func
{
    private readonly ShapeDistance[] zones;
    private readonly int length;

    public SDIntersection(ShapeDistance[] Zones)
    {
        zones = Zones;
        length = zones.Length;
    }

    public override float Distance(WPos p)
    {
        var array = zones;
        var max = float.MinValue;
        var point = p;

        for (var i = 0; i < length; ++i)
        {
            var d = array[i].Distance(point);
            if (d > max)
            {
                max = d;
            }
        }
        return max;
    }
}

public sealed class SDUnion : ShapeDistance // min distance func
{
    private readonly ShapeDistance[] zones;
    private readonly int length;

    public SDUnion(ShapeDistance[] Zones)
    {
        zones = Zones;
        length = zones.Length;
    }

    public override float Distance(WPos p)
    {
        var array = zones;
        var min = float.MaxValue;
        var point = p;

        for (var i = 0; i < length; ++i)
        {
            var d = array[i].Distance(point);
            if (d < min)
            {
                min = d;
            }
        }
        return min;
    }
}

public sealed class SDInvertedUnion : ShapeDistance // -min distance func
{
    private readonly ShapeDistance[] zones;
    private readonly int length;

    public SDInvertedUnion(ShapeDistance[] Zones)
    {
        zones = Zones;
        length = zones.Length;
    }

    public override float Distance(WPos p)
    {
        var array = zones;
        var min = float.MaxValue;
        var point = p;

        for (var i = 0; i < length; ++i)
        {
            var d = array[i].Distance(point);
            if (d < min)
            {
                min = d;
            }
        }
        return -min;
    }
}

public sealed class SDInvertedUnionOffset : ShapeDistance // -min distance func
{
    private readonly ShapeDistance[] zones;
    private readonly int length;
    private readonly float offset;

    public SDInvertedUnionOffset(ShapeDistance[] Zones, float Offset)
    {
        zones = Zones;
        length = zones.Length;
        offset = Offset;
    }

    public override float Distance(WPos p)
    {
        var array = zones;
        var min = float.MaxValue;
        var point = p;

        for (var i = 0; i < length; ++i)
        {
            var d = array[i].Distance(point);
            if (d < min)
            {
                min = d;
            }
        }
        return -min + offset;
    }
}
