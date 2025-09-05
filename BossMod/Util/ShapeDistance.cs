namespace BossMod;

// shapes can be defined by distance from point to shape's border; distance is positive for points outside shape and negative for points inside shape
// union is min, intersection is max

public abstract class ShapeDistance
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract float Distance(WPos p);
}

public sealed class SDHalfPlane : ShapeDistance
{
    private readonly float pointX, pointZ, normalX, normalZ;

    public SDHalfPlane(WPos Point, WDir Normal)
    {
        var point = Point;
        pointX = point.X;
        pointZ = point.Z;
        var normal = Normal;
        normalX = normal.X;
        normalZ = normal.Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p) => normalX * (p.X - pointX) + normalZ * (p.Z - pointZ);
}

public sealed class SDCircle : ShapeDistance
{
    private readonly float originX, originZ, radius;

    public SDCircle(WPos Origin, float Radius)
    {
        var origin = Origin;
        originX = origin.X;
        originZ = origin.Z;
        radius = Radius;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        return MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ) - radius;
    }
}

public sealed class SDInvertedCircle : ShapeDistance
{
    private readonly float originX, originZ, radius;

    public SDInvertedCircle(WPos Origin, float Radius)
    {
        var origin = Origin;
        originX = origin.X;
        originZ = origin.Z;
        radius = Radius;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        return radius - MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ);
    }
}

public sealed class SDDonut : ShapeDistance
{
    private readonly float originX, originZ, innerRadius, outerRadius;

    public SDDonut(WPos Origin, float InnerRadius, float OuterRadius)
    {
        var origin = Origin;
        originX = origin.X;
        originZ = origin.Z;
        innerRadius = InnerRadius;
        outerRadius = OuterRadius;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        // intersection of outer circle and inverted inner circle
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        var distOrigin = MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ);
        var distOuter = distOrigin - outerRadius;
        var distInner = innerRadius - distOrigin;
        return distOuter > distInner ? distOuter : distInner;
    }
}

public sealed class SDInvertedDonut : ShapeDistance
{
    private readonly float originX, originZ, innerRadius, outerRadius;

    public SDInvertedDonut(WPos Origin, float InnerRadius, float OuterRadius)
    {
        var origin = Origin;
        originX = origin.X;
        originZ = origin.Z;
        innerRadius = InnerRadius;
        outerRadius = OuterRadius;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        // intersection of outer circle and inverted inner circle
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        var distOrigin = MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ);
        var distOuter = distOrigin - outerRadius;
        var distInner = innerRadius - distOrigin;
        return distOuter > distInner ? -distOuter : -distInner;
    }
}

public sealed class SDCone : ShapeDistance
{
    private readonly float originX, originZ, coneFactor, radius, nlX, nlZ, nrX, nrZ;

    public SDCone(WPos Origin, float Radius, Angle CenterDir, Angle HalfAngle)
    {
        originX = Origin.X;
        originZ = Origin.Z;
        radius = Radius;
        var halfAngle = HalfAngle;
        coneFactor = halfAngle.Rad > Angle.HalfPi ? -1f : 1f;
        var centerDir = CenterDir;
        var nl = coneFactor * (centerDir + halfAngle).ToDirection().OrthoL();
        var nr = coneFactor * (centerDir - halfAngle).ToDirection().OrthoR();
        nlX = nl.X;
        nlZ = nl.Z;
        nrX = nr.X;
        nrZ = nr.Z;
    }

    // for <= 180-degree cone: result = intersection of circle and two half-planes with normals pointing outside cone sides
    // for > 180-degree cone: result = intersection of circle and negated intersection of two half-planes with inverted normals
    // both normals point outside
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        var distOrigin = MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ);
        var distOuter = distOrigin - radius;
        var distLeft = pXoriginX * nlX + pZoriginZ * nlZ;
        var distRight = pXoriginX * nrX + pZoriginZ * nrZ;

        var maxSideDist = distLeft > distRight ? distLeft : distRight;
        var conef = coneFactor * maxSideDist;
        return distOuter > conef ? distOuter : conef;
    }
}

public sealed class SDInvertedCone : ShapeDistance
{
    private readonly float originX, originZ, coneFactor, radius, nlX, nlZ, nrX, nrZ;

    public SDInvertedCone(WPos Origin, float Radius, Angle CenterDir, Angle HalfAngle)
    {
        originX = Origin.X;
        originZ = Origin.Z;
        radius = Radius;
        var halfAngle = HalfAngle;
        coneFactor = halfAngle.Rad > Angle.HalfPi ? -1f : 1f;
        var centerDir = CenterDir;
        var nl = coneFactor * (centerDir + halfAngle).ToDirection().OrthoL();
        var nr = coneFactor * (centerDir - halfAngle).ToDirection().OrthoR();
        nlX = nl.X;
        nlZ = nl.Z;
        nrX = nr.X;
        nrZ = nr.Z;
    }

    // for <= 180-degree cone: result = intersection of circle and two half-planes with normals pointing outside cone sides
    // for > 180-degree cone: result = intersection of circle and negated intersection of two half-planes with inverted normals
    // both normals point outside
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        var distOrigin = MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ);
        var distOuter = distOrigin - radius;
        var distLeft = pXoriginX * nlX + pZoriginZ * nlZ;
        var distRight = pXoriginX * nrX + pZoriginZ * nrZ;

        var maxSideDist = distLeft > distRight ? distLeft : distRight;
        var conef = coneFactor * maxSideDist;
        return distOuter > conef ? -distOuter : -conef;
    }
}

public sealed class SDDonutSector : ShapeDistance
{
    private readonly float originX, originZ, coneFactor, innerRadius, outerRadius, nlX, nlZ, nrX, nrZ;

    public SDDonutSector(WPos Origin, float InnerRadius, float OuterRadius, Angle CenterDir, Angle HalfAngle)
    {
        originX = Origin.X;
        originZ = Origin.Z;
        innerRadius = InnerRadius;
        outerRadius = OuterRadius;
        var halfAngle = HalfAngle;
        coneFactor = halfAngle.Rad > Angle.HalfPi ? -1f : 1f;
        var centerDir = CenterDir;
        var a90 = 90f.Degrees();
        var nl = coneFactor * (centerDir + halfAngle + a90).ToDirection().OrthoL();
        var nr = coneFactor * (centerDir - halfAngle - a90).ToDirection().OrthoR();
        nlX = nl.X;
        nlZ = nl.Z;
        nrX = nr.X;
        nrZ = nr.Z;
    }

    // for <= 180-degree cone: result = intersection of circle and two half-planes with normals pointing outside cone sides
    // for > 180-degree cone: result = intersection of circle and negated intersection of two half-planes with inverted normals
    // both normals point outside
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        var distOrigin = MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ);
        var distOuter = distOrigin - outerRadius;
        var distInner = innerRadius - distOrigin;
        var distLeft = pXoriginX * nlX + pZoriginZ * nlZ;
        var distRight = pXoriginX * nrX + pZoriginZ * nrZ;

        var maxRadial = distOuter > distInner ? distOuter : distInner;
        var maxCone = distLeft > distRight ? distLeft : distRight;
        var conef = coneFactor * maxCone;
        return maxRadial > conef ? maxRadial : conef;
    }
}

public sealed class SDInvertedDonutSector : ShapeDistance
{
    private readonly float originX, originZ, coneFactor, innerRadius, outerRadius, nlX, nlZ, nrX, nrZ;

    public SDInvertedDonutSector(WPos Origin, float InnerRadius, float OuterRadius, Angle CenterDir, Angle HalfAngle)
    {
        originX = Origin.X;
        originZ = Origin.Z;
        innerRadius = InnerRadius;
        outerRadius = OuterRadius;
        var halfAngle = HalfAngle;
        coneFactor = halfAngle.Rad > Angle.HalfPi ? -1f : 1f;
        var centerDir = CenterDir;
        var a90 = 90f.Degrees();
        var nl = coneFactor * (centerDir + halfAngle + a90).ToDirection().OrthoL();
        var nr = coneFactor * (centerDir - halfAngle - a90).ToDirection().OrthoR();
        nlX = nl.X;
        nlZ = nl.Z;
        nrX = nr.X;
        nrZ = nr.Z;
    }

    // for <= 180-degree cone: result = intersection of circle and two half-planes with normals pointing outside cone sides
    // for > 180-degree cone: result = intersection of circle and negated intersection of two half-planes with inverted normals
    // both normals point outside
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        var distOrigin = MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ);
        var distOuter = distOrigin - outerRadius;
        var distInner = innerRadius - distOrigin;
        var distLeft = pXoriginX * nlX + pZoriginZ * nlZ;
        var distRight = pXoriginX * nrX + pZoriginZ * nrZ;

        var maxRadial = distOuter > distInner ? distOuter : distInner;
        var maxCone = distLeft > distRight ? distLeft : distRight;
        var conef = coneFactor * maxCone;
        return maxRadial > conef ? -maxRadial : -conef;
    }
}

public sealed class SDTri : ShapeDistance
{
    private readonly float n1X, n1Z, n2X, n2Z, n3X, n3Z, aX, aZ, bX, bZ, cX, cZ;

    public SDTri(WPos Origin, RelTriangle Triangle)
    {
        var tri = Triangle;
        var ab = tri.B - tri.A;
        var bc = tri.C - tri.B;
        var ca = tri.A - tri.C;
        var n1 = ab.OrthoL().Normalized();
        var n2 = bc.OrthoL().Normalized();
        var n3 = ca.OrthoL().Normalized();
        if (ab.Cross(bc) < 0f)
        {
            n1 = -n1;
            n2 = -n2;
            n3 = -n3;
        }
        var origin = Origin;
        var a = origin + tri.A;
        var b = origin + tri.B;
        var c = origin + tri.C;

        n1X = n1.X;
        n1Z = n1.Z;
        n2X = n2.X;
        n2Z = n2.Z;
        n3X = n3.X;
        n3Z = n3.Z;
        aX = a.X;
        aZ = a.Z;
        bX = b.X;
        bZ = b.Z;
        cX = c.X;
        cZ = c.Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        var pX = p.X;
        var pZ = p.Z;
        var d1 = n1X * (pX - aX) + n1Z * (pZ - aZ);
        var d2 = n2X * (pX - bX) + n2Z * (pZ - bZ);
        var d3 = n3X * (pX - cX) + n3Z * (pZ - cZ);
        var max1 = d1 > d2 ? d1 : d2;
        return max1 > d3 ? max1 : d3;
    }
}

public sealed class SDInvertedTri : ShapeDistance
{
    private readonly float n1X, n1Z, n2X, n2Z, n3X, n3Z, aX, aZ, bX, bZ, cX, cZ;

    public SDInvertedTri(WPos Origin, RelTriangle Triangle)
    {
        var tri = Triangle;
        var ab = tri.B - tri.A;
        var bc = tri.C - tri.B;
        var ca = tri.A - tri.C;
        var n1 = ab.OrthoL().Normalized();
        var n2 = bc.OrthoL().Normalized();
        var n3 = ca.OrthoL().Normalized();
        if (ab.Cross(bc) < 0f)
        {
            n1 = -n1;
            n2 = -n2;
            n3 = -n3;
        }
        var origin = Origin;
        var a = origin + tri.A;
        var b = origin + tri.B;
        var c = origin + tri.C;

        n1X = n1.X;
        n1Z = n1.Z;
        n2X = n2.X;
        n2Z = n2.Z;
        n3X = n3.X;
        n3Z = n3.Z;
        aX = a.X;
        aZ = a.Z;
        bX = b.X;
        bZ = b.Z;
        cX = c.X;
        cZ = c.Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        var pX = p.X;
        var pZ = p.Z;
        var d1 = n1X * (pX - aX) + n1Z * (pZ - aZ);
        var d2 = n2X * (pX - bX) + n2Z * (pZ - bZ);
        var d3 = n3X * (pX - cX) + n3Z * (pZ - cZ);
        var max1 = d1 > d2 ? d1 : d2;
        return max1 > d3 ? -max1 : -d3;
    }
}

public sealed class SDRect : ShapeDistance
{
    private readonly float originX, originZ, dirX, dirZ, normalX, normalZ, lenFront, lenBack, halfWidth;

    public SDRect(WPos Origin, Angle Direction, float LenFront, float LenBack, float HalfWidth) : this(Origin, Direction.ToDirection(), LenFront, LenBack, HalfWidth) { }

    public SDRect(WPos From, WPos To, float HalfWidth)
    {
        var from = From;
        var dir = To - from;
        var l = dir.Length();
        var normalizedDir = dir / l;
        var normal = normalizedDir.OrthoL();

        originX = from.X;
        originZ = from.Z;
        dirX = normalizedDir.X;
        dirZ = normalizedDir.Z;
        normalX = normal.X;
        normalZ = normal.Z;
        lenFront = l;
        lenBack = 0;
        halfWidth = HalfWidth;
    }

    public SDRect(WPos Origin, WDir Dir, float LenFront, float LenBack, float HalfWidth)
    {
        // dir points outside far side
        var dir = Dir;
        var normal = dir.OrthoL(); // points outside left side
        var origin = Origin;
        originX = origin.X;
        originZ = origin.Z;
        dirX = dir.X;
        dirZ = dir.Z;
        normalX = normal.X;
        normalZ = normal.Z;
        lenFront = LenFront;
        lenBack = LenBack;
        halfWidth = HalfWidth;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        var distParr = pXoriginX * dirX + pZoriginZ * dirZ;
        var distOrtho = pXoriginX * normalX + pZoriginZ * normalZ;
        var distFront = distParr - lenFront;
        var distBack = -distParr - lenBack;
        var distLeft = distOrtho - halfWidth;
        var distRight = -distOrtho - halfWidth;

        var maxParr = distFront > distBack ? distFront : distBack;
        var maxOrtho = distLeft > distRight ? distLeft : distRight;

        return maxParr > maxOrtho ? maxParr : maxOrtho;
    }
}

public sealed class SDInvertedRect : ShapeDistance
{
    private readonly float originX, originZ, dirX, dirZ, normalX, normalZ, lenFront, lenBack, halfWidth;

    public SDInvertedRect(WPos Origin, Angle Direction, float LenFront, float LenBack, float HalfWidth) : this(Origin, Direction.ToDirection(), LenFront, LenBack, HalfWidth) { }

    public SDInvertedRect(WPos From, WPos To, float HalfWidth)
    {
        var from = From;
        var dir = To - from;
        var l = dir.Length();
        var normalizedDir = dir / l;
        var normal = normalizedDir.OrthoL();

        originX = from.X;
        originZ = from.Z;
        dirX = normalizedDir.X;
        dirZ = normalizedDir.Z;
        normalX = normal.X;
        normalZ = normal.Z;
        lenFront = l;
        lenBack = default;
        halfWidth = HalfWidth;
    }

    public SDInvertedRect(WPos Origin, WDir Dir, float LenFront, float LenBack, float HalfWidth)
    {
        // dir points outside far side
        var dir = Dir;
        var normal = dir.OrthoL(); // points outside left side
        var origin = Origin;
        originX = origin.X;
        originZ = origin.Z;
        dirX = dir.X;
        dirZ = dir.Z;
        normalX = normal.X;
        normalZ = normal.Z;
        lenFront = LenFront;
        lenBack = LenBack;
        halfWidth = HalfWidth;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        var distParr = pXoriginX * dirX + pZoriginZ * dirZ;
        var distOrtho = pXoriginX * normalX + pZoriginZ * normalZ;
        var distFront = distParr - lenFront;
        var distBack = -distParr - lenBack;
        var distLeft = distOrtho - halfWidth;
        var distRight = -distOrtho - halfWidth;

        var maxParr = distFront > distBack ? distFront : distBack;
        var maxOrtho = distLeft > distRight ? distLeft : distRight;

        return maxParr > maxOrtho ? -maxParr : -maxOrtho;
    }
}

public sealed class SDCapsule : ShapeDistance
{
    private readonly float originX, originZ, dirX, dirZ, length, radius;
    private readonly WDir direction;
    private readonly WPos origin;

    public SDCapsule(WPos Origin, Angle Direction, float Length, float Radius) : this(Origin, Direction.ToDirection(), Length, Radius) { }

    public SDCapsule(WPos Origin, WDir Direction, float Length, float Radius)
    {
        origin = Origin;
        direction = Direction;
        originX = origin.X;
        originZ = origin.Z;
        dirX = direction.X;
        dirZ = direction.Z;
        length = Length;
        radius = Radius;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        var pX = p.X;
        var pZ = p.Z;
        var pXoriginX = pX - originX;
        var pZoriginZ = pZ - originZ;
        var t = pXoriginX * dirX + pZoriginZ * dirZ;
        t = (t < 0f) ? default : (t > length ? length : t);
        var proj = origin + t * direction;
        var pXprojX = pX - proj.X;
        var pZprojZ = pZ - proj.Z;
        return MathF.Sqrt(pXprojX * pXprojX + pZprojZ * pZprojZ) - radius;
    }
}

public sealed class SDInvertedCapsule : ShapeDistance
{
    private readonly float originX, originZ, dirX, dirZ, length, radius;
    private readonly WDir direction;
    private readonly WPos origin;

    public SDInvertedCapsule(WPos Origin, Angle Direction, float Length, float Radius) : this(Origin, Direction.ToDirection(), Length, Radius) { }

    public SDInvertedCapsule(WPos Origin, WDir Direction, float Length, float Radius)
    {
        origin = Origin;
        direction = Direction;
        originX = origin.X;
        originZ = origin.Z;
        dirX = direction.X;
        dirZ = direction.Z;
        length = Length;
        radius = Radius;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        var pX = p.X;
        var pZ = p.Z;
        var pXoriginX = pX - originX;
        var pZoriginZ = pZ - originZ;
        var t = pXoriginX * dirX + pZoriginZ * dirZ;
        t = (t < 0f) ? default : (t > length ? length : t);
        var proj = origin + t * direction;
        var pXprojX = pX - proj.X;
        var pZprojZ = pZ - proj.Z;
        return radius - MathF.Sqrt(pXprojX * pXprojX + pZprojZ * pZprojZ);
    }
}

public sealed class SDCross : ShapeDistance
{
    private readonly float length, halfWidth;
    private readonly WDir direction, normal;
    private readonly WPos origin;

    public SDCross(WPos Origin, Angle Direction, float Length, float HalfWidth)
    {
        origin = Origin;
        direction = Direction.ToDirection();
        length = Length;
        halfWidth = HalfWidth;
        normal = direction.OrthoL();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        var offset = p - origin;
        var distParr = offset.Dot(direction);
        var distOrtho = offset.Dot(normal);
        var distPFront = distParr - length;
        var distPBack = -distParr - length;
        var distPLeft = distOrtho - halfWidth;
        var distPRight = -distOrtho - halfWidth;
        var distOFront = distOrtho - length;
        var distOBack = -distOrtho - length;
        var distOLeft = distParr - halfWidth;
        var distORight = -distParr - halfWidth;

        var distPMax1 = distPFront > distPBack ? distPFront : distPBack;
        var distPMax2 = distPLeft > distPRight ? distPLeft : distPRight;
        var distP = distPMax1 > distPMax2 ? distPMax1 : distPMax2;

        var distOMax1 = distOFront > distOBack ? distOFront : distOBack;
        var distOMax2 = distOLeft > distORight ? distOLeft : distORight;
        var distO = distOMax1 > distOMax2 ? distOMax1 : distOMax2;

        return distP < distO ? distP : distO;
    }
}

public sealed class SDInvertedCross : ShapeDistance
{
    private readonly float length, halfWidth;
    private readonly WDir direction, normal;
    private readonly WPos origin;

    public SDInvertedCross(WPos Origin, Angle Direction, float Length, float HalfWidth)
    {
        origin = Origin;
        direction = Direction.ToDirection();
        length = Length;
        halfWidth = HalfWidth;
        normal = direction.OrthoL();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        var offset = p - origin;
        var distParr = offset.Dot(direction);
        var distOrtho = offset.Dot(normal);
        var distPFront = distParr - length;
        var distPBack = -distParr - length;
        var distPLeft = distOrtho - halfWidth;
        var distPRight = -distOrtho - halfWidth;
        var distOFront = distOrtho - length;
        var distOBack = -distOrtho - length;
        var distOLeft = distParr - halfWidth;
        var distORight = -distParr - halfWidth;

        var distPMax1 = distPFront > distPBack ? distPFront : distPBack;
        var distPMax2 = distPLeft > distPRight ? distPLeft : distPRight;
        var distP = distPMax1 > distPMax2 ? distPMax1 : distPMax2;

        var distOMax1 = distOFront > distOBack ? distOFront : distOBack;
        var distOMax2 = distOLeft > distORight ? distOLeft : distORight;
        var distO = distOMax1 > distOMax2 ? distOMax1 : distOMax2;

        return distP < distO ? -distP : -distO;
    }
}

public sealed class SDConvexPolygon : ShapeDistance
{
    private readonly bool cw;
    private readonly (WPos, WPos)[] edges;

    public SDConvexPolygon((WPos, WPos)[] Edges, bool Cw)
    {
        edges = Edges;
        cw = Cw;
    }

    public SDConvexPolygon(ReadOnlySpan<WPos> Vertices, bool Cw)
    {
        var vertices = Vertices;
        var len = vertices.Length;
        var edgesA = new (WPos, WPos)[len];
        for (var i = 0; i < len; ++i)
        {
            var a = vertices[i];
            var b = vertices[(i + 1) % len];
            edgesA[i] = (a, b);
        }
        edges = edgesA;
        cw = Cw;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        var minDistance = float.MaxValue;
        var inside = true;
        var len = edges.Length;
        var point = p;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var edge = ref edges[i];
            var a = edge.Item1;
            var b = edge.Item2;
            var ab = b - a;
            var ap = point - a;
            var distance = (ab.X * ap.Z - ab.Z * ap.X) / ab.Length();

            if (cw && distance > 0f || !cw && distance < 0f)
            {
                inside = false;
            }
            minDistance = Math.Min(minDistance, Math.Abs(distance));
        }
        return inside ? -minDistance : minDistance;
    }
}

public sealed class SDIntersection : ShapeDistance // max distance func
{
    private readonly ShapeDistance[] zones;
    private readonly int length;

    public SDIntersection(ShapeDistance[] Zones)
    {
        zones = Zones;
        length = zones.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

public sealed class SDInvertedUnion : ShapeDistance // min distance func
{
    private readonly ShapeDistance[] zones;
    private readonly int length;

    public SDInvertedUnion(ShapeDistance[] Zones)
    {
        zones = Zones;
        length = zones.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

// special distance function for precise positioning, finer than map resolution
// it's an inverted rect of a size equal to one grid cell, with a special adjustment if starting position is in the same cell, but farther than tolerance
public sealed class SDPrecisePosition : ShapeDistance
{
    private readonly float originX, originZ, dirX, dirZ, normalX, normalZ, cellSize;

    public SDPrecisePosition(WPos Origin, WDir Direction, float CellSize, WPos Starting, float Tolerance = default)
    {
        var starting = Starting;
        var origin = Origin;
        cellSize = CellSize;
        var dir = Direction;
        var tolerance = Tolerance;
        var delta = starting - origin;
        var dparr = delta.Dot(dir);
        if (dparr > tolerance && dparr <= cellSize)
        {
            origin -= cellSize * dir;
        }
        else if (dparr < -tolerance && dparr >= -cellSize)
        {
            origin += cellSize * dir;
        }

        var normal = dir.OrthoL();
        var dortho = delta.Dot(normal);
        if (dortho > tolerance && dortho <= cellSize)
        {
            origin -= cellSize * normal;
        }
        else if (dortho < -tolerance && dortho >= -cellSize)
        {
            origin += cellSize * normal;
        }
        originX = origin.X;
        originZ = origin.Z;
        dirX = dir.X;
        dirZ = dir.Z;
        normalX = normal.X;
        normalZ = normal.Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        var distParr = pXoriginX * dirX + pZoriginZ * dirZ;
        var distOrtho = pXoriginX * normalX + pZoriginZ * normalZ;
        var distFront = distParr - cellSize;
        var distBack = -distParr - cellSize;
        var distLeft = distOrtho - cellSize;
        var distRight = -distOrtho - cellSize;

        var maxParr = distFront > distBack ? distFront : distBack;
        var maxOrtho = distLeft > distRight ? distLeft : distRight;

        return maxParr > maxOrtho ? maxParr : maxOrtho;
    }
}

public abstract class SDPolygonWithHolesAbs : ShapeDistance
{
    protected readonly RelSimplifiedComplexPolygon _polygon;
    protected readonly float _originX, _originZ;
    protected readonly Edge[] _edges;
    protected readonly SpatialIndex _spatialIndex;

    public SDPolygonWithHolesAbs(WPos origin, RelSimplifiedComplexPolygon polygon)
    {
        _originX = origin.X;
        _originZ = origin.Z;
        _polygon = polygon;
        var edgeCount = 0;
        var countPolygonParts = polygon.Parts.Count;
        for (var i = 0; i < countPolygonParts; ++i)
        {
            var part = polygon.Parts[i];
            edgeCount += part.ExteriorEdges.Length;
            var lenPolygonHoles = part.Holes.Length;
            for (var j = 0; j < lenPolygonHoles; ++j)
                edgeCount += part.InteriorEdges(j).Length;
        }
        _edges = new Edge[edgeCount];
        var edgeIndex = 0;
        for (var i = 0; i < countPolygonParts; ++i)
        {
            var part = polygon.Parts[i];
            var exteriorEdges = GetEdges(part.Exterior, origin);
            var exteriorCount = exteriorEdges.Length;
            Array.Copy(exteriorEdges, 0, _edges, edgeIndex, exteriorCount);
            edgeIndex += exteriorCount;
            var lenPolygonHoles = part.Holes.Length;
            for (var j = 0; j < lenPolygonHoles; ++j)
            {
                var holeEdges = GetEdges(part.Interior(j), origin);
                var holeEdgesCount = holeEdges.Length;
                Array.Copy(holeEdges, 0, _edges, edgeIndex, holeEdgesCount);
                edgeIndex += holeEdgesCount;
            }
        }
        _spatialIndex = new(_edges);

        static Edge[] GetEdges(ReadOnlySpan<WDir> vertices, WPos origin)
        {
            var count = vertices.Length;

            if (count == 0)
            {
                return [];
            }

            var edges = new Edge[count];

            var prev = vertices[count - 1];
            var originX = origin.X;
            var originZ = origin.Z;

            for (var i = 0; i < count; ++i)
            {
                var curr = vertices[i];
                var prevX = prev.X;
                var prevZ = prev.Z;
                edges[i] = new(originX + prevX, originZ + prevZ, curr.X - prevX, curr.Z - prevZ);
                prev = curr;
            }

            return edges;
        }
    }
}

public sealed class SDPolygonWithHoles(WPos origin, RelSimplifiedComplexPolygon polygon) : SDPolygonWithHolesAbs(origin, polygon)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        var pX = p.X;
        var pZ = p.Z;
        if (_polygon.Contains(new(pX - _originX, pZ - _originZ))) // NOTE: our usecase doesn't care about distance inside of the polygon, so we can short circuit here
        {
            return default;
        }
        var minDistanceSq = float.MaxValue;

        var indices = _spatialIndex.Query(pX, pZ);
        var len = indices.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var edge = ref _edges[indices[i]];
            var edgeAx = edge.Ax;
            var edgeAy = edge.Ay;
            var edgeDx = edge.Dx;
            var edgeDy = edge.Dy;
            var t = Math.Clamp(((pX - edgeAx) * edgeDx + (pZ - edgeAy) * edgeDy) * edge.InvLengthSq, default, 1f);
            var distX = pX - (edgeAx + t * edgeDx);
            var distY = pZ - (edgeAy + t * edgeDy);

            minDistanceSq = Math.Min(minDistanceSq, distX * distX + distY * distY);
        }
        return MathF.Sqrt(minDistanceSq);
    }
}

public sealed class SDInvertedPolygonWithHoles(WPos origin, RelSimplifiedComplexPolygon polygon) : SDPolygonWithHolesAbs(origin, polygon)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(WPos p)
    {
        var pX = p.X;
        var pZ = p.Z;
        if (!_polygon.Contains(new(pX - _originX, pZ - _originZ))) // NOTE: our usecase doesn't care about distance outside of the polygon, so we can short circuit here
        {
            return default;
        }
        var minDistanceSq = float.MaxValue;

        var indices = _spatialIndex.Query(pX, pZ);
        var len = indices.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var edge = ref _edges[indices[i]];
            var edgeAx = edge.Ax;
            var edgeAy = edge.Ay;
            var edgeDx = edge.Dx;
            var edgeDy = edge.Dy;
            var t = Math.Clamp(((pX - edgeAx) * edgeDx + (pZ - edgeAy) * edgeDy) * edge.InvLengthSq, default, 1f);
            var distX = pX - (edgeAx + t * edgeDx);
            var distY = pZ - (edgeAy + t * edgeDy);

            minDistanceSq = Math.Min(minDistanceSq, distX * distX + distY * distY);
        }
        return MathF.Sqrt(minDistanceSq);
    }
}
