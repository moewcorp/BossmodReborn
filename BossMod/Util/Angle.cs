namespace BossMod;

// wrapper around float, stores angle in radians, provides type-safety and convenience
// when describing rotation in world, common convention is 0 for 'south'/'down'/(0, -1) and increasing counterclockwise - so +90 is 'east'/'right'/(1, 0)
public record struct Angle(float Rad)
{
    public const float RadToDeg = (float)(180 / Math.PI);
    public const float DegToRad = (float)(Math.PI / 180);
    public const float HalfPi = (float)(Math.PI / 2);
    public const float DoublePI = (float)(2 * Math.PI);

    public static readonly Angle[] AnglesIntercardinals = [-45.003f.Degrees(), 44.998f.Degrees(), 134.999f.Degrees(), -135.005f.Degrees()];
    public static readonly Angle[] AnglesCardinals = [-90.004f.Degrees(), -0.003f.Degrees(), 180.Degrees(), 89.999f.Degrees()];

    public readonly float Deg => Rad * RadToDeg;

    public static Angle FromDirection(WDir dir) => new(MathF.Atan2(dir.X, dir.Z));
    public readonly WDir ToDirection()
    {
        var (sin, cos) = ((float, float))Math.SinCos(Rad);
        return new(sin, cos);
    }

    public static Angle operator +(Angle a, Angle b) => new(a.Rad + b.Rad);
    public static Angle operator -(Angle a, Angle b) => new(a.Rad - b.Rad);
    public static Angle operator -(Angle a) => new(-a.Rad);
    public static Angle operator *(Angle a, float b) => new(a.Rad * b);
    public static Angle operator *(float a, Angle b) => new(a * b.Rad);
    public static Angle operator /(Angle a, float b) => new(a.Rad / b);
    public static bool operator >(Angle a, Angle b) => a.Rad > b.Rad;
    public static bool operator <(Angle a, Angle b) => a.Rad < b.Rad;
    public static bool operator >=(Angle a, Angle b) => a.Rad >= b.Rad;
    public static bool operator <=(Angle a, Angle b) => a.Rad <= b.Rad;

    public readonly Angle Abs() => new(Math.Abs(Rad));
    public readonly float Sin() => (float)Math.Sin(Rad);
    public readonly float Cos() => (float)Math.Cos(Rad);
    public readonly float Tan() => (float)Math.Tan(Rad);
    public static Angle Asin(float x) => new((float)Math.Asin(x));
    public static Angle Acos(float x) => new((float)Math.Acos(x));

    public readonly Angle Normalized()
    {
        var r = Rad;
        while (r < -MathF.PI)
            r += DoublePI;
        while (r > MathF.PI)
            r -= DoublePI;
        return new(r);
    }

    public readonly bool AlmostEqual(Angle other, float epsRad) => Math.Abs((this - other).Normalized().Rad) <= epsRad;

    public override readonly string ToString() => Deg.ToString("f3");
}

public static class AngleExtensions
{
    public static Angle Radians(this float radians) => new(radians);
    public static Angle Degrees(this float degrees) => new(degrees * Angle.DegToRad);
    public static Angle Degrees(this int degrees) => new(degrees * Angle.DegToRad);
}

public static class CosPI
{
    public static readonly float Pi8th = (float)(1 / Math.Cos(Math.PI / 8));
    public static readonly float Pi16th = (float)(1 / Math.Cos(Math.PI / 16));
    public static readonly float Pi20th = (float)(1 / Math.Cos(Math.PI / 20));
    public static readonly float Pi28th = (float)(1 / Math.Cos(Math.PI / 28));
    public static readonly float Pi32th = (float)(1 / Math.Cos(Math.PI / 32));
    public static readonly float Pi36th = (float)(1 / Math.Cos(Math.PI / 36));
    public static readonly float Pi40th = (float)(1 / Math.Cos(Math.PI / 40));
    public static readonly float Pi48th = (float)(1 / Math.Cos(Math.PI / 48));
    public static readonly float Pi60th = (float)(1 / Math.Cos(Math.PI / 60));
}
