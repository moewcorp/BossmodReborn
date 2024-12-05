using Clipper2Lib;

namespace BossMod;

public abstract record class Shape
{
    public const float MaxApproxError = CurveApprox.ScreenError;
    public const float Half = 0.5f;
    protected WDir[]? Points;

    public abstract List<WDir> Contour(WPos center);

    public RelSimplifiedComplexPolygon ToPolygon(WPos center) => new((List<RelPolygonWithHoles>)[new(Contour(center))]);
}

public record class Circle(WPos Center, float Radius) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        Points ??= CurveApprox.Circle(Radius, MaxApproxError);
        var len = Points.Length;
        var result = new List<WDir>(len);
        var offset = Center - center;
        for (var i = 0; i < len; ++i)
            result.Add(Points[i] + offset);
        return result;
    }
    public override string ToString() => $"Circle:{Center.X},{Center.Z},{Radius}";
}

// for custom polygons defined by an IEnumerable of vertices
public record class PolygonCustom(IEnumerable<WPos> Vertices) : Shape
{
    private readonly WPos[] points = [.. Vertices];
    public override List<WDir> Contour(WPos center)
    {
        var len = points.Length;
        var result = new List<WDir>(len);
        for (var i = 0; i < len; ++i)
            result.Add(points[i] - center);
        return result;
    }

    public override string ToString()
    {
        var len = points.Length;
        var sb = new StringBuilder($"PolygonCustom:");
        for (var i = 0; i < len; ++i)
        {
            sb.Append(points[i].X).Append(',').Append(points[i].Z);
            if (i < len - 1)
                sb.Append(',');
        }
        return sb.ToString();
    }
}

// for custom polygons defined by an IEnumerable of vertices with an offset, eg to account for hitbox radius
public record class PolygonCustomO(IEnumerable<WPos> Vertices, float Offset) : Shape
{
    private readonly WPos[] points = [.. Vertices];
    private Path64? path;

    public override List<WDir> Contour(WPos center)
    {
        if (path == null)
        {
            var originalPath = new Path64();
            for (var i = 0; i < points.Length; ++i)
            {
                var v = points[i];
                originalPath.Add(new Point64((long)(v.X * PolygonClipper.Scale), (long)(v.Z * PolygonClipper.Scale)));
            }

            ClipperOffset co = new();
            co.AddPath(originalPath, JoinType.Miter, EndType.Polygon);
            var solution = new Paths64();
            co.Execute(Offset * PolygonClipper.Scale, solution);
            path = solution[0];
        }

        var count = path.Count;
        var offsetContour = new List<WDir>(count);

        for (var i = 0; i < count; ++i)
        {
            var p = path[i];
            offsetContour.Add(new WDir((float)(p.X * PolygonClipper.InvScale - center.X), (float)(p.Y * PolygonClipper.InvScale - center.Z)));
        }

        return offsetContour;
    }

    public override string ToString()
    {
        var len = points.Length;
        var sb = new StringBuilder($"PolygonCustomO:");
        for (var i = 0; i < len; ++i)
        {
            sb.Append(points[i].X).Append(',').Append(points[i].Z);
            if (i < len - 1)
                sb.Append(',');
        }
        sb.Append(',').Append(Offset);
        return sb.ToString();
    }
}

public record class Donut(WPos Center, float InnerRadius, float OuterRadius) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        Points ??= CurveApprox.Donut(InnerRadius, OuterRadius, MaxApproxError);
        var len = Points.Length;
        var result = new List<WDir>(len);
        var offset = Center - center;
        for (var i = 0; i < len; ++i)
            result.Add(Points[i] + offset);
        return result;
    }
    public override string ToString() => $"Donut:{Center.X},{Center.Z},{InnerRadius},{OuterRadius}";
}

// for rectangles defined by a center, halfwidth, halfheight and optionally rotation
public record class Rectangle(WPos Center, float HalfWidth, float HalfHeight, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        if (Points == null)
        {
            var (sin, cos) = ((float, float))Math.SinCos(Rotation.Rad);
            var rotatedHalfWidth = new WDir(HalfWidth * cos, HalfWidth * sin);
            var rotatedHalfHeight = new WDir(-HalfHeight * sin, HalfHeight * cos);
            Points =
            [
                rotatedHalfWidth + rotatedHalfHeight,
                rotatedHalfWidth - rotatedHalfHeight,
                -rotatedHalfWidth - rotatedHalfHeight,
                -rotatedHalfWidth + rotatedHalfHeight
            ];
        }
        var offset = Center - center;
        var result = new List<WDir>(4);
        for (var i = 0; i < 4; ++i)
            result.Add(Points[i] + offset);
        return result;
    }
    public override string ToString() => $"Rectangle:{Center.X},{Center.Z},{HalfWidth},{HalfHeight},{Rotation}";
}

// for rectangles defined by a start point, end point and halfwidth
public record class RectangleSE(WPos Start, WPos End, float HalfWidth) : Rectangle(
    Center: new((Start.X + End.X) * Half, (Start.Z + End.Z) * Half),
    HalfWidth: HalfWidth,
    HalfHeight: (End - Start).Length() * Half,
    Rotation: new Angle(MathF.Atan2(End.Z - Start.Z, End.X - Start.X)) + 90.Degrees()
);

public record class Square(WPos Center, float HalfSize, Angle Rotation = default) : Rectangle(Center, HalfSize, HalfSize, Rotation);

public record class Cross(WPos Center, float Length, float HalfWidth, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        if (Points == null)
        {
            var dx = Rotation.ToDirection();
            var dy = dx.OrthoL();
            var dx1 = dx * Length;
            var dx2 = dx * HalfWidth;
            var dy1 = dy * Length;
            var dy2 = dy * HalfWidth;

            Points =
            [
                dx1 + dy2,
                dx2 + dy2,
                dx2 + dy1,
                -dx2 + dy1,
                -dx2 + dy2,
                -dx1 + dy2,
                -dx1 - dy2,
                -dx2 - dy2,
                -dx2 - dy1,
                dx2 - dy1,
                dx2 - dy2,
                dx1 - dy2
            ];
        }
        var offset = Center - center;
        var result = new List<WDir>(12);
        for (var i = 0; i < 12; ++i)
            result.Add(Points[i] + offset);
        return result;
    }
    public override string ToString() => $"Cross:{Center.X},{Center.Z},{Length},{HalfWidth},{Rotation}";
}

// Equilateral triangle defined by center, sidelength and rotation
public record class TriangleE(WPos Center, float SideLength, Angle Rotation = default) : Shape
{
    private static readonly float heightFactor = MathF.Sqrt(3) * Half;

    public override List<WDir> Contour(WPos center)
    {
        if (Points == null)
        {
            var height = SideLength * heightFactor;
            var halfSideLength = SideLength * Half;
            var halfHeight = height * Half;
            var (sin, cos) = ((float, float))Math.SinCos(Rotation.Rad);
            var halfSideCos = halfSideLength * cos;
            var halfSideSin = halfSideLength * sin;
            var halfHeightSin = halfHeight * sin;
            var halfHeightCos = halfHeight * cos;
            Points =
            [
                new WDir(halfSideCos - halfHeightSin, halfSideSin + halfHeightCos),
                new WDir(-halfSideCos - halfHeightSin, -halfSideSin + halfHeightCos),
                new WDir(halfHeightSin, -halfHeight * cos)
            ];
        }
        var offset = Center - center;
        var result = new List<WDir>(3);
        for (var i = 0; i < 3; ++i)
            result.Add(Points[i] + offset);
        return result;
    }
    public override string ToString() => $"TriangleE:{Center.X},{Center.Z},{SideLength},{Rotation}";
}

// for polygons with edge count number of lines of symmetry, eg. pentagons, hexagons and octagons
public record class Polygon(WPos Center, float Radius, int Edges, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        if (Points == null)
        {
            var angleIncrement = Angle.DoublePI / Edges;
            var initialRotation = Rotation.Rad;
            var vertices = new List<WDir>(Edges);
            for (var i = Edges - 1; i >= 0; --i)
            {
                var (sin, cos) = ((float, float))Math.SinCos(i * angleIncrement + initialRotation);
                vertices.Add(new(Center.X + Radius * cos, Center.Z + Radius * sin));
            }
            Points = [.. vertices];
        }
        var len = Points.Length;
        var result = new List<WDir>(len);
        for (var i = 0; i < len; ++i)
            result.Add(Points[i] - center);
        return result;
    }
    public override string ToString() => $"Polygon:{Center.X},{Center.Z},{Radius},{Edges},{Rotation}";
}

// for cones defined by radius, start angle and end angle
public record class Cone(WPos Center, float Radius, Angle StartAngle, Angle EndAngle) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        if (Points == null)
        {
            var points = CurveApprox.CircleSector(Center, Radius, StartAngle, EndAngle, MaxApproxError);
            var length = points.Length;
            var vertices = new List<WDir>(length);
            for (var i = 0; i < length; ++i)
                vertices.Add(points[i] - new WPos());
            Points = [.. vertices];
        }
        var len = Points.Length;
        var result = new List<WDir>(len);
        for (var i = 0; i < len; ++i)
            result.Add(Points[i] - center);
        return result;
    }
    public override string ToString() => $"Cone:{Center.X},{Center.Z},{Radius},{StartAngle},{EndAngle}";
}

// for cones defined by radius, direction and half angle
public record class ConeHA(WPos Center, float Radius, Angle CenterDir, Angle HalfAngle) : Cone(Center, Radius, CenterDir - HalfAngle, CenterDir + HalfAngle);

// for donut segments defined by inner and outer radius, direction, start angle and end angle
public record class DonutSegment(WPos Center, float InnerRadius, float OuterRadius, Angle StartAngle, Angle EndAngle) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        Points ??= CurveApprox.DonutSector(InnerRadius, OuterRadius, StartAngle, EndAngle, MaxApproxError);
        var len = Points.Length;
        var result = new List<WDir>(len);
        var offset = Center - center;
        for (var i = 0; i < len; ++i)
        {
            result.Add(Points[i] + offset);
        }
        return result;
    }
    public override string ToString() => $"DonutSegment:{Center.X},{Center.Z},{InnerRadius},{OuterRadius},{StartAngle},{EndAngle}";
}

// for donut segments defined by inner and outer radius, direction and half angle
public record class DonutSegmentHA(WPos Center, float InnerRadius, float OuterRadius, Angle CenterDir, Angle HalfAngle) : DonutSegment(Center, InnerRadius, OuterRadius,
CenterDir - HalfAngle, CenterDir + HalfAngle);

// Approximates a cone with a customizable number of edges for the circle arc
public record class ConeV(WPos Center, float Radius, Angle CenterDir, Angle HalfAngle, int Edges) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        if (Points == null)
        {
            var angleIncrement = 2 * HalfAngle.Rad / Edges;
            var startAngle = CenterDir.Rad - HalfAngle.Rad;
            var vertices = new List<WDir>(Edges + 1);

            for (var i = 0; i < Edges + 1; ++i)
            {
                var (sin, cos) = ((float, float))Math.SinCos(startAngle + i * angleIncrement);
                vertices.Add(new(Center.X + Radius * cos, Center.Z + Radius * sin));
            }
            vertices.Add(Center - new WPos());
            Points = [.. vertices];
        }
        var len = Points.Length;
        var result = new List<WDir>(len);
        for (var i = 0; i < len; ++i)
            result.Add(Points[i] - center);
        return result;
    }

    public override string ToString() => $"ConeV:{Center.X},{Center.Z},{Radius},{CenterDir},{HalfAngle},{Edges}";
}

// Approximates a donut segment with a customizable number of edges per circle arc
public record class DonutSegmentV(WPos Center, float InnerRadius, float OuterRadius, Angle CenterDir, Angle HalfAngle, int Edges) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        if (Points == null)
        {
            var angleIncrement = 2 * HalfAngle.Rad / Edges;
            var startAngle = CenterDir.Rad - HalfAngle.Rad;
            var vertices = new List<WDir>(2 * (Edges + 1));

            for (var i = Edges; i >= 0; --i)
            {
                var (sin, cos) = ((float, float))Math.SinCos(startAngle + i * angleIncrement);
                vertices.Add(new(Center.X + OuterRadius * cos, Center.Z + OuterRadius * sin));
            }

            for (var i = 0; i < Edges + 1; ++i)
            {
                var (sin, cos) = ((float, float))Math.SinCos(startAngle + i * angleIncrement);
                vertices.Add(new(Center.X + InnerRadius * cos, Center.Z + InnerRadius * sin));
            }
            Points = [.. vertices];
        }
        var len = Points.Length;
        var result = new List<WDir>(len);
        for (var i = 0; i < len; ++i)
            result.Add(Points[i] - center);
        return result;
    }

    public override string ToString() => $"DonutSegmentV:{Center.X},{Center.Z},{InnerRadius},{OuterRadius},{CenterDir},{HalfAngle},{Edges}";
}

public record class DonutV(WPos Center, float InnerRadius, float OuterRadius, int Edges) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        if (Points == null)
        {
            var angleIncrement = Angle.DoublePI / Edges;
            var vertices = new List<WDir>(2 * (Edges + 1));

            for (var i = 0; i <= Edges; ++i)
            {
                var (sin, cos) = ((float, float))Math.SinCos(i * angleIncrement);
                vertices.Add(new(Center.X + OuterRadius * cos, Center.Z + OuterRadius * sin));
            }

            for (var i = Edges; i >= 0; --i)
            {
                var (sin, cos) = ((float, float))Math.SinCos(i * angleIncrement);
                vertices.Add(new(Center.X + InnerRadius * cos, Center.Z + InnerRadius * sin));
            }
            Points = [.. vertices];
        }
        var len = Points.Length;
        var result = new List<WDir>(len);
        for (var i = 0; i < len; ++i)
            result.Add(Points[i] - center);
        return result;
    }

    public override string ToString() => $"DonutV:{Center.X},{Center.Z},{InnerRadius},{OuterRadius},{Edges}";
}
