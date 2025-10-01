namespace BossMod;

[SkipLocalsInit]
public sealed class SDIntersection : ShapeDistance // max distance func
{
    private readonly ShapeDistance[] zones;
    private readonly int length;

    public SDIntersection(ShapeDistance[] Zones)
    {
        zones = Zones;
        length = zones.Length;
    }

    public override float Distance(in WPos p)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p) => Distance(p) <= 0f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDUnion : ShapeDistance // min distance func
{
    private readonly ShapeDistance[] zones;
    private readonly int length;

    public SDUnion(ShapeDistance[] Zones)
    {
        zones = Zones;
        length = zones.Length;
    }

    public override float Distance(in WPos p)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var array = zones;
        for (var i = 0; i < length; ++i)
        {
            if (array[i].Contains(p))
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
public sealed class SDInvertedUnion : ShapeDistance // -min distance func
{
    private readonly ShapeDistance[] zones;
    private readonly int length;

    public SDInvertedUnion(ShapeDistance[] Zones)
    {
        zones = Zones;
        length = zones.Length;
    }

    public override float Distance(in WPos p)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var array = zones;
        for (var i = 0; i < length; ++i)
        {
            if (array[i].Contains(p))
            {
                return false;
            }
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

[SkipLocalsInit]
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

    public override float Distance(in WPos p)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var array = zones;
        for (var i = 0; i < length; ++i)
        {
            if (array[i].Distance(p) < offset)
            {
                return false;
            }
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

// outside of a union of shapes, useful for preventing stacking spread markers
[SkipLocalsInit]
public sealed class SDOutsideOfUnion : ShapeDistance
{
    private readonly ShapeDistance[] zones;
    private readonly int length;

    public SDOutsideOfUnion(ShapeDistance[] Zones)
    {
        zones = Zones;
        length = zones.Length;
    }

    public override float Distance(in WPos p)
    {
        var insideCount = 0;
        var minAbs = float.MaxValue;
        var array = zones;
        for (var i = 0; i < length; ++i)
        {
            var d = array[i].Distance(p);
            if (d >= 0f)
            {
                ++insideCount;
            }

            var ad = Math.Abs(d);
            if (ad < minAbs)
            {
                minAbs = ad;
            }
        }

        // exactly one shape contains the point => inside (negative)
        return (insideCount == 1) ? minAbs : -minAbs;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var array = zones;
        for (var i = 0; i < length; ++i)
        {
            if (array[i].Contains(p))
            {
                return false;
            }
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}
