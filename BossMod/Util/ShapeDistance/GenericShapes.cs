namespace BossMod;

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

    public override float Distance(WPos p) => normalX * (p.X - pointX) + normalZ * (p.Z - pointZ);

    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        var rowstart = rowStart;
        var rowstartX = rowstart.X;
        var rowstartZ = rowstart.Z;
        var aX = rowstartX - pointX;
        var aZ = rowstartZ - pointZ;
        var bX = (rowstart + width * dx).X - pointX;
        var bZ = (rowstart + width * dx).Z - pointZ;
        var s0 = normalX * aX + normalZ * aZ;
        var s1 = normalX * bX + normalZ * bZ;
        return (s0 < s1 ? s0 : s1) <= cushion + Epsilon; // include tangency
    }
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

    public override float Distance(WPos p)
    {
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        return MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ) - radius;
    }

    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        // Segment vs disk test using closest-point distance
        var rowstart = rowStart;
        var rowstartX = rowstart.X;
        var rowstartZ = rowstart.Z;
        var aX = rowstartX - originX;
        var aZ = rowstartZ - originZ;
        var b = rowstart + width * dx;
        var dX = b.X - rowstartX;
        var dZ = b.Z - rowstartZ;
        var A = dX * dX + dZ * dZ;
        var R = radius + cushion;
        var R2 = R * R;
        if (A <= Epsilon)
        {
            var d2 = aX * aX + aZ * aZ;
            return d2 <= R2 + Epsilon;
        }
        var t = -(aX * dX + aZ * dZ) / A; // projection of center onto segment
        if (t < 0f)
        {
            t = 0f;
        }
        else if (t > 1f)
        {
            t = 1f;
        }
        var cX = aX + t * dX;
        var cZ = aZ + t * dZ;
        var minD2 = cX * cX + cZ * cZ;
        return minD2 <= R2 + Epsilon;
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

    public override float Distance(WPos p)
    {
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        return radius - MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ);
    }

    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        // Outside of disk: segment intersects unless fully inside the (inflated) disk.
        var rowstart = rowStart;
        var aX = rowstart.X - originX;
        var aZ = rowstart.Z - originZ;
        var b = rowstart + width * dx;
        var bX = b.X - originX;
        var bZ = b.Z - originZ;
        var R = radius + cushion;
        var R2 = R * R;
        var aIn = (aX * aX + aZ * aZ) <= R2 + Epsilon;
        var bIn = (bX * bX + bZ * bZ) <= R2 + Epsilon;
        return !(aIn && bIn);
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

    public override bool RowIntersectsShape(WPos rowStart, WDir dxVec, float width, float cushion = default)
    {
        // Segment vs annulus [Rin, Rout] with cushion ⇒ [max(0,Rin-c), Rout+c]
        var rowstart = rowStart;
        var rowstartX = rowstart.X;
        var rowstartZ = rowstart.Z;
        var aX = rowstartX - originX;
        var aZ = rowstartZ - originZ;
        var b = rowstart + width * dxVec;
        var dX = b.X - rowstartX;
        var dZ = b.Z - rowstartZ;
        var A = dX * dX + dZ * dZ;

        var Rout = outerRadius + cushion;
        var Rout2 = Rout * Rout;
        var Rin = MathF.Max(0f, innerRadius - cushion);
        var Rin2 = Rin * Rin;

        // closest-point distance^2 to center
        float minD2;
        if (A <= Epsilon)
        {
            minD2 = aX * aX + aZ * aZ;
        }
        else
        {
            var t = -(aX * dX + aZ * dZ) / A;
            if (t < 0f)
            {
                t = 0f;
            }
            else if (t > 1f)
            {
                t = 1f;
            }
            var cX = aX + t * dX;
            var cZ = aZ + t * dZ;
            minD2 = cX * cX + cZ * cZ;
        }
        var hitsOuter = minD2 <= Rout2 + Epsilon;
        if (!hitsOuter)
        {
            return false;
        }

        // fully inside inner disk? (convex ⇒ endpoints suffice)
        var aInInner = (aX * aX + aZ * aZ) <= Rin2 + Epsilon;
        var bX = b.X - originX;
        var bZ = b.Z - originZ;
        var bInInner = (bX * bX + bZ * bZ) <= Rin2 + Epsilon;
        return !aInInner || !bInInner;
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

    public override bool RowIntersectsShape(WPos rowStart, WDir dxVec, float width, float cushion = default)
    {
        // Inverted annulus = outside outer disk ∪ inside inner disk.
        // Segment hits inverted region unless it is fully inside the inflated annulus.
        var rowstart = rowStart;
        var rowstartX = rowstart.X;
        var rowstartZ = rowstart.Z;
        var aX = rowstartX - originX;
        var aZ = rowstartZ - originZ;
        var b = rowstart + width * dxVec;
        var bX_ = b.X;
        var bZ_ = b.Z;
        var bX = bX_ - originX;
        var bZ = bZ_ - originZ;
        var dX = bX_ - rowstartX;
        var dZ = bZ_ - rowstartZ;
        var A = dX * dX + dZ * dZ;

        var Rout = outerRadius + cushion;
        var Rout2 = Rout * Rout;
        var Rin = MathF.Max(0f, innerRadius - cushion);
        var Rin2 = Rin * Rin;

        // Fully inside outer disk? (convex ⇒ endpoints-inside suffice)
        var aInOuter = (aX * aX + aZ * aZ) <= Rout2 + Epsilon;
        var bInOuter = (bX * bX + bZ * bZ) <= Rout2 + Epsilon;
        if (!(aInOuter && bInOuter))
        {
            return true; // touches inverted outside
        }

        // Intersects inner disk? If yes, not fully inside annulus ⇒ intersects inverted
        float minD2;
        if (A <= Epsilon)
        {
            minD2 = aX * aX + aZ * aZ;
        }
        else
        {
            var t = -(aX * dX + aZ * dZ) / A;
            if (t < 0f)
            {
                t = 0f;
            }
            else if (t > 1f)
            {
                t = 1f;
            }
            var cX = aX + t * dX;
            var cZ = aZ + t * dZ;
            minD2 = cX * cX + cZ * cZ;
        }
        var hitsInner = minD2 <= Rin2 + Epsilon; // include tangency
        if (hitsInner)
        {
            return true;
        }

        // fully inside annulus
        return false;
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

    public bool SegmentIntersectsCone(WPos a, WPos b, float cushion = default)
    {
        // Shift to cone origin
        var a_ = a;
        var aX_ = a_.X;
        var aZ_ = a_.Z;
        var b_ = b;
        var ax = aX_ - originX;
        var az = aZ_ - originZ;
        var dx = b_.X - aX_;
        var dz = b_.Z - aZ_;

        // 1) Intersect with disk of radius (r+c)
        var r = radius + cushion;
        var A = dx * dx + dz * dz;
        if (A <= Epsilon)
        {
            // Point test (degenerate segment)
            return Distance(a) <= cushion + Epsilon;
        }
        var B = 2f * (ax * dx + az * dz);
        var C = ax * ax + az * az - r * r;
        var disc = B * B - 4f * A * C;
        if (disc < -Epsilon)
        {
            return false;
        }
        var sqrtD = MathF.Sqrt(disc < 0f ? 0f : disc);
        var inv2A = 0.5f / A;
        var t1 = (-B - sqrtD) * inv2A;
        var t2 = (-B + sqrtD) * inv2A;
        if (t1 > t2)
        {
            (t2, t1) = (t1, t2);
        }
        var tmin = MathF.Max(0f, t1);
        var tmax = MathF.Min(1f, t2);
        if (tmax <= tmin + Epsilon)
        {
            return false; // misses disk entirely
        }

        // 2) Angular constraint(s)
        if (coneFactor > 0f)
        {
            // <= 180° : AND of two half-planes n·((a-o) + t d) <= cushion
            return ClipHalfplaneLE(nlX, nlZ, ax, az, dx, dz, cushion, ref tmin, ref tmax)
            && ClipHalfplaneLE(nrX, nrZ, ax, az, dx, dz, cushion, ref tmin, ref tmax)
            && tmax > tmin + Epsilon;
        }
        else
        {
            // > 180° : inside if (HL >= -c) OR (HR >= -c); compute each interval on [0,1] and OR them
            float l0 = 0f, l1 = 1f;
            var lOk = ClipHalfplaneGE(nlX, nlZ, ax, az, dx, dz, -cushion, ref l0, ref l1);
            float r0 = 0f, r1 = 1f;
            var rOk = ClipHalfplaneGE(nrX, nrZ, ax, az, dx, dz, -cushion, ref r0, ref r1);
            if (!lOk && !rOk)
            {
                return false;
            }
            // Intersect each with disk window and test overlap
            return lOk && MathF.Min(l1, tmax) > MathF.Max(l0, tmin) + Epsilon || rOk && MathF.Min(r1, tmax) > MathF.Max(r0, tmin) + Epsilon;
        }
    }

    // rotated grid row: p(s) = rowStart + s*dx, s∈[s0,s1]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        return SegmentIntersectsCone(rowStart, rowStart + width * dx, cushion);
    }

    // Solve n·((a-o) + t d) <= c for t in [tmin,tmax]
    private static bool ClipHalfplaneLE(float nX, float nZ, float ax, float az, float dx, float dz, float c, ref float tmin, ref float tmax)
    {
        var s0 = nX * ax + nZ * az; // value at t=0
        var s1 = nX * dx + nZ * dz; // derivative along segment
        if (NearlyZero(s1))
        {
            return s0 <= c + Epsilon; // all or nothing
        }
        var bound = (c - s0) / s1; // t at which equality holds
        if (s1 > 0f)
        {
            tmax = MathF.Min(tmax, bound);
        }
        else
        {
            tmin = MathF.Max(tmin, bound);
        }
        return tmax > tmin + Epsilon;
    }

    // Solve n·((a-o) + t d) >= v  for t
    private static bool ClipHalfplaneGE(float nX, float nZ, float ax, float az, float dx, float dz, float v, ref float tmin, ref float tmax)
    {
        var s0 = nX * ax + nZ * az;
        var s1 = nX * dx + nZ * dz;
        if (NearlyZero(s1))
        {
            if (s0 >= v - Epsilon)
            {
                return true; // whole [0,1]
            }
            tmin = 1f;
            tmax = 0f;
            return false; // empty
        }
        var bound = (v - s0) / s1;
        if (s1 > 0f)
        {
            tmin = MathF.Max(tmin, bound);
        }
        else
        {
            tmax = MathF.Min(tmax, bound);
        }
        return tmax > tmin + Epsilon;
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

    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        return !SegmentFullyInsideOriginal(rowStart, rowStart + width * dx, cushion);
    }

    private bool SegmentFullyInsideOriginal(WPos a, WPos b, float cushion)
    {
        var r = radius + cushion;

        // Shift to origin frame
        var a_ = a;
        var b_ = b;
        var ax = a_.X - originX;
        var az = a_.Z - originZ;
        var bx = b_.X - originX;
        var bz = b_.Z - originZ;

        // Disk: convex ⇒ endpoints-inside implies whole segment-inside
        var r2 = r * r;
        var aInDisk = (ax * ax + az * az) <= r2 + Epsilon;
        var bInDisk = (bx * bx + bz * bz) <= r2 + Epsilon;
        if (!(aInDisk && bInDisk))
        {
            return false;
        }
        if (coneFactor > 0f)
        {
            // ≤180°: wedge = intersection of two half-planes (convex). Endpoints test is sufficient.
            bool EndInside(float px, float pz)
            {
                var left = px * nlX + pz * nlZ; // <= cushion
                var right = px * nrX + pz * nrZ; // <= cushion
                return left <= cushion + Epsilon && right <= cushion + Epsilon;
            }
            return EndInside(ax, az) && EndInside(bx, bz);
        }
        else
        {
            // >180°: wedge = union of two half-planes. Need coverage of [0,1] by union of GE intervals.
            // Parametric segment in local frame: p(t) = a + t (b - a), t∈[0,1]
            var dx = bx - ax;
            var dz = bz - az;
            float t0 = 0f, t1 = 1f;

            // Intervals where each half-plane holds: n·(a + t d) >= -cushion
            var LOk = ClipHalfplaneGE(nlX, nlZ, ax, az, dx, dz, -cushion, ref t0, ref t1, out var l0, out var l1);
            var ROk = ClipHalfplaneGE(nrX, nrZ, ax, az, dx, dz, -cushion, ref t0, ref t1, out var r0, out var r1);

            if (!LOk && !ROk)
            {
                return false; // no t satisfies wedge → cannot be fully inside
            }

            // Check if union covers [0,1]
            if (LOk && !ROk)
            {
                return l0 <= Epsilon && l1 >= 1f - Epsilon;
            }
            if (ROk && !LOk)
            {
                return r0 <= Epsilon && r1 >= 1f - Epsilon;
            }

            // Both present: merge two intervals
            float s0, s1, t2, t3;
            if (l0 <= r0)
            {
                s0 = l0;
                s1 = l1;
                t2 = r0;
                t3 = r1;
            }
            else
            {
                s0 = r0;
                s1 = r1;
                t2 = l0;
                t3 = l1;
            }

            if (s0 > 0f + Epsilon)
            {
                return false; // gap at start
            }
            var coverEnd = s1;
            if (t2 > coverEnd + Epsilon)
            {
                return false; // gap between intervals
            }
            if (t3 > coverEnd)
            {
                coverEnd = t3;
            }
            return coverEnd >= 1f - Epsilon;
        }
    }

    // Compute interval on [0,1] where n·(a + t d) >= value. Returns false if empty.
    private static bool ClipHalfplaneGE(float nX, float nZ, float ax, float az, float dx, float dz, float value, ref float tmin, ref float tmax, out float outMin, out float outMax)
    {
        outMin = 1f;
        outMax = 0f; // empty by default
        var s0 = nX * ax + nZ * az;
        var s1 = nX * dx + nZ * dz;
        if (NearlyZero(s1))
        {
            if (s0 >= value - Epsilon)
            {
                outMin = 0f;
                outMax = 1f;
                return true;
            }
            return false;
        }
        var bound = (value - s0) / s1;
        float a0 = 0f, a1 = 1f;
        if (s1 > 0f)
        {
            a0 = MathF.Max(a0, bound);
        }
        else
        {
            a1 = MathF.Min(a1, bound);
        }
        // also clip with caller's [tmin,tmax] if desired
        a0 = MathF.Max(a0, tmin);
        a1 = MathF.Min(a1, tmax);
        if (a1 > a0 + Epsilon)
        {
            outMin = a0;
            outMax = a1;
            return true;
        }
        return false;
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
        var nl = coneFactor * (centerDir + halfAngle + a90).ToDirection();
        var nr = coneFactor * (centerDir - halfAngle - a90).ToDirection();
        nlX = nl.X;
        nlZ = nl.Z;
        nrX = nr.X;
        nrZ = nr.Z;
    }

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

    // Row cull with exact annulus∩wedge clipping on the segment
    public override bool RowIntersectsShape(WPos rowStart, WDir dxVec, float width, float cushion = default)
    {
        var rowstart = rowStart;
        var rowstartX = rowstart.X;
        var rowstartZ = rowstart.Z;
        var ax = rowstartX - originX;
        var az = rowstartZ - originZ;
        var b = rowstart + width * dxVec;
        var dx = b.X - rowstartX;
        var dz = b.Z - rowstartZ;
        var A = dx * dx + dz * dz;
        if (A <= Epsilon)
        {
            // degenerate: point test
            return Distance(rowStart) <= cushion + Epsilon;
        }

        // radial annulus window(s)
        var Rout = outerRadius + cushion;
        var Rout2 = Rout * Rout;
        var Rin = MathF.Max(0f, innerRadius - cushion);
        var Rin2 = Rin * Rin;
        var B = 2f * (ax * dx + az * dz);
        var Couter = ax * ax + az * az - Rout2;
        var DiscOuter = B * B - 4f * A * Couter;
        if (DiscOuter < -Epsilon)
        {
            return false; // misses outer disk entirely
        }
        var sqrtOut = MathF.Sqrt(DiscOuter < 0f ? 0f : DiscOuter);
        var inv2A = 0.5f / A;
        var tOut1 = (-B - sqrtOut) * inv2A;
        var tOut2 = (-B + sqrtOut) * inv2A;
        if (tOut1 > tOut2)
        {
            (tOut2, tOut1) = (tOut1, tOut2);
        }
        var u0 = MathF.Max(0f, tOut1);
        var u1 = MathF.Min(1f, tOut2);
        if (u1 <= u0 + Epsilon)
        {
            return false;
        }

        // inner disk interval (forbidden) to subtract
        var Cinner = ax * ax + az * az - Rin2;
        var DiscInner = B * B - 4f * A * Cinner;
        var hasInner = DiscInner >= -Epsilon;
        float v0 = 0f, v1 = 0f;
        if (hasInner)
        {
            var sqrtIn = MathF.Sqrt(DiscInner < 0f ? 0f : DiscInner);
            var tIn1 = (-B - sqrtIn) * inv2A;
            var tIn2 = (-B + sqrtIn) * inv2A;
            if (tIn1 > tIn2)
            {
                (tIn2, tIn1) = (tIn1, tIn2);
            }
            v0 = MathF.Max(0f, tIn1);
            v1 = MathF.Min(1f, tIn2);
            if (v1 <= v0 + Epsilon)
            {
                hasInner = false; // tangent/empty
            }
        }

        // Subtract [v0,v1] from [u0,u1] to get up to two annulus intervals
        Span<(float a, float b)> annulus = stackalloc (float a, float b)[2];
        var annN = 0;
        void Add(float a, float b, Span<(float, float)> annulus)
        {
            if (b > a + Epsilon)
                annulus[annN++] = (a, b);
        }

        if (!hasInner)
        {
            Add(u0, u1, annulus);
        }
        else
        {
            var a0 = u0;
            var a1 = u1;
            var b0 = v0;
            var b1 = v1;
            if (b1 <= a0 || b0 >= a1)
            {
                Add(a0, a1, annulus);
            }
            else if (b0 <= a0 && b1 >= a1)
            {
                // fully removed
            }
            else if (b0 <= a0)
            {
                Add(b1, a1, annulus);
            }
            else if (b1 >= a1)
            {
                Add(a0, b0, annulus);
            }
            else
            {
                Add(a0, b0, annulus);
                Add(b1, a1, annulus);
            }
        }
        if (annN == 0)
        {
            return false;
        }

        // --- wedge constraints ---
        if (coneFactor > 0f)
        {
            // <= 180° : intersection of two half-planes n·((a-o) + t d) <= cushion
            for (var i = 0; i < annN; ++i)
            {
                float t0 = annulus[i].a, t1 = annulus[i].b;
                if (!ClipHalfplaneLE(nlX, nlZ, ax, az, dx, dz, cushion, ref t0, ref t1))
                {
                    continue;
                }
                if (!ClipHalfplaneLE(nrX, nrZ, ax, az, dx, dz, cushion, ref t0, ref t1))
                {
                    continue;
                }
                if (t1 > t0 + Epsilon)
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            // > 180° : union of two half-planes n·((a-o) + t d) >= -cushion
            float l0 = 0f, l1 = 1f;
            var lOk = ClipHalfplaneGE(nlX, nlZ, ax, az, dx, dz, -cushion, ref l0, ref l1);
            float r0 = 0f, r1 = 1f;
            var rOk = ClipHalfplaneGE(nrX, nrZ, ax, az, dx, dz, -cushion, ref r0, ref r1);
            if (!lOk && !rOk)
            {
                return false;
            }
            for (var i = 0; i < annN; ++i)
            {
                var a0 = annulus[i].a;
                var a1 = annulus[i].b;
                if (lOk && MathF.Min(l1, a1) > MathF.Max(l0, a0) + Epsilon)
                {
                    return true;
                }
                if (rOk && MathF.Min(r1, a1) > MathF.Max(r0, a0) + Epsilon)
                {
                    return true;
                }
            }
            return false;
        }
    }

    private static bool ClipHalfplaneLE(float nX, float nZ, float ax, float az, float dx, float dz, float c, ref float tmin, ref float tmax)
    {
        var s0 = nX * ax + nZ * az;
        var s1 = nX * dx + nZ * dz;
        if (MathF.Abs(s1) <= Epsilon)
        {
            return s0 <= c + Epsilon;
        }
        var bound = (c - s0) / s1;
        if (s1 > 0f)
        {
            tmax = MathF.Min(tmax, bound);
        }
        else
        {
            tmin = MathF.Max(tmin, bound);
        }
        return tmax > tmin + Epsilon;
    }

    private static bool ClipHalfplaneGE(float nX, float nZ, float ax, float az, float dx, float dz, float v, ref float tmin, ref float tmax)
    {
        var s0 = nX * ax + nZ * az;
        var s1 = nX * dx + nZ * dz;
        if (MathF.Abs(s1) <= Epsilon)
        {
            return s0 >= v - Epsilon;
        }
        var bound = (v - s0) / s1;
        if (s1 > 0f)
        {
            tmin = MathF.Max(tmin, bound);
        }
        else
        {
            tmax = MathF.Min(tmax, bound);
        }
        return tmax > tmin + Epsilon;
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
        var nl = coneFactor * (centerDir + halfAngle + a90).ToDirection();
        var nr = coneFactor * (centerDir - halfAngle - a90).ToDirection();
        nlX = nl.X;
        nlZ = nl.Z;
        nrX = nr.X;
        nrZ = nr.Z;
    }

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

    // conservative and safe: consider the row to intersect the inverted region unless we can prove
    // the whole segment lies inside the original donut-sector.
    public override bool RowIntersectsShape(WPos rowStart, WDir dxVec, float width, float cushion = default)
    {
        return !SegmentProvablyInsideOriginal(rowStart, rowStart + width * dxVec, cushion);
    }

    private bool SegmentProvablyInsideOriginal(WPos a, WPos b, float cushion)
    {
        var a_ = a;
        var aX_ = a_.X;
        var aZ_ = a_.Z;
        var b_ = b;
        var bX_ = b_.X;
        var bZ_ = b_.Z;
        var ax = aX_ - originX;
        var az = aZ_ - originZ;
        var bx = bX_ - originX;
        var bz = bZ_ - originZ;
        var dx = bX_ - aX_;
        var dz = bZ_ - aZ_;

        // 1) Entire segment inside outer disk (convex): endpoints suffice
        var Rout = outerRadius + cushion;
        var Rout2 = Rout * Rout;
        var aOut = (ax * ax + az * az) <= Rout2 + Epsilon;
        var bOut = (bx * bx + bz * bz) <= Rout2 + Epsilon;
        if (!(aOut && bOut))
        {
            return false;
        }

        // 2) Entire segment outside inner disk: min distance >= Rin
        var Rin = MathF.Max(0f, innerRadius - cushion);
        var Rin2 = Rin * Rin;
        float minD2;
        var A = dx * dx + dz * dz;
        if (A <= Epsilon)
        {
            minD2 = ax * ax + az * az;
        }
        else
        {
            var t = -(ax * dx + az * dz) / A;
            if (t < 0f)
            {
                t = 0f;
            }
            else if (t > 1f)
            {
                t = 1f;
            }
            var cx = ax + t * dx;
            var cz = az + t * dz;
            minD2 = cx * cx + cz * cz;
        }
        if (minD2 < Rin2 - Epsilon)
        {
            return false;
        }

        // 3) Wedge containment
        var lA = ax * nlX + az * nlZ;
        var lB = bx * nlX + bz * nlZ;
        var rA = ax * nrX + az * nrZ;
        var rB = bx * nrX + bz * nrZ;
        if (coneFactor > 0f)
        {
            // intersection of two convex half-planes ⇒ endpoints suffice
            return lA <= cushion + Epsilon && lB <= cushion + Epsilon && rA <= cushion + Epsilon && rB <= cushion + Epsilon;
        }
        else
        {
            // union of two half-planes: ensure segment stays entirely in at least one
            var inL = lA >= -cushion - Epsilon && lB >= -cushion - Epsilon;
            var inR = rA >= -cushion - Epsilon && rB >= -cushion - Epsilon;
            return inL || inR;
        }
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

    // Row-cull: clip segment by 3 half-planes (convex triangle inflated by cushion)
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        var rowstart = rowStart;
        var a0x = rowstart.X;
        var a0z = rowstart.Z;
        var b = rowstart + width * dx;
        var dX = b.X - a0x;
        var dZ = b.Z - a0z;
        float tmin = 0f, tmax = 1f;

        return ClipLE(n1X, n1Z, a0x - aX, a0z - aZ, dX, dZ, cushion, ref tmin, ref tmax)
        && ClipLE(n2X, n2Z, a0x - bX, a0z - bZ, dX, dZ, cushion, ref tmin, ref tmax)
        && ClipLE(n3X, n3Z, a0x - cX, a0z - cZ, dX, dZ, cushion, ref tmin, ref tmax)
        && tmax > tmin + Epsilon;
    }

    private static bool ClipLE(float nX, float nZ, float vx, float vz, float dx, float dz, float value, ref float tmin, ref float tmax)
    {
        var s0 = nX * vx + nZ * vz;
        var s1 = nX * dx + nZ * dz;
        if (NearlyZero(s1))
        {
            return s0 <= value + Epsilon; // why: constant along the row
        }
        var bound = (value - s0) / s1;
        if (s1 > 0f)
        {
            tmax = MathF.Min(tmax, bound);
        }
        else
        {
            tmin = MathF.Max(tmin, bound);
        }
        return tmax > tmin + Epsilon;
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

    // inverted: segment intersects unless fully inside (inflated) triangle
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        return !(Inside(rowStart, cushion) && Inside(rowStart + width * dx, cushion));
    }

    private bool Inside(WPos p, float cushion)
    {
        var px = p.X;
        var pz = p.Z;
        var d1 = n1X * (px - aX) + n1Z * (pz - aZ);
        var d2 = n2X * (px - bX) + n2Z * (pz - bZ);
        var d3 = n3X * (px - cX) + n3Z * (pz - cZ);
        return d1 <= cushion + Epsilon && d2 <= cushion + Epsilon && d3 <= cushion + Epsilon;
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

    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        return SegmentIntersectsRect(rowStart, rowStart + width * dx, cushion);
    }

    private bool SegmentIntersectsRect(WPos a, WPos b, float cushion)
    {
        // Clip segment [0,1] by 4 half-planes of inflated rectangle
        var a_ = a;
        var aX_ = a_.X;
        var aZ_ = a_.Z;
        var ax = aX_ - originX;
        var az = aZ_ - originZ;
        var b_ = b;
        var dxs = b_.X - aX_;
        var dzs = b_.Z - aZ_;

        float tmin = 0f, tmax = 1f;
        // dir·(p-o) <= lenFront + c
        return ClipLE(dirX, dirZ, ax, az, dxs, dzs, lenFront + cushion, ref tmin, ref tmax) // dir·(p-o) <= lenFront + c
        && ClipGE(dirX, dirZ, ax, az, dxs, dzs, -(lenBack + cushion), ref tmin, ref tmax) // dir·(p-o) >= -(lenBack + c)
        && ClipLE(normalX, normalZ, ax, az, dxs, dzs, halfWidth + cushion, ref tmin, ref tmax) // n·(p-o) <= halfWidth + c
        && ClipGE(normalX, normalZ, ax, az, dxs, dzs, -(halfWidth + cushion), ref tmin, ref tmax) // n·(p-o) >= -(halfWidth + c)
        && tmax > tmin + Epsilon;
    }

    private static bool ClipLE(float nX, float nZ, float ax, float az, float dx, float dz, float value, ref float tmin, ref float tmax)
    {
        var s0 = nX * ax + nZ * az;
        var s1 = nX * dx + nZ * dz;
        if (NearlyZero(s1))
        {
            return s0 <= value + Epsilon;
        }
        var bound = (value - s0) / s1;
        if (s1 > 0f)
        {
            tmax = MathF.Min(tmax, bound);
        }
        else
        {
            tmin = MathF.Max(tmin, bound);
        }
        return tmax > tmin + Epsilon;
    }

    private static bool ClipGE(float nX, float nZ, float ax, float az, float dx, float dz, float value, ref float tmin, ref float tmax)
    {
        var s0 = nX * ax + nZ * az;
        var s1 = nX * dx + nZ * dz;
        if (NearlyZero(s1))
        {
            return s0 >= value - Epsilon;
        }
        var bound = (value - s0) / s1;
        if (s1 > 0f)
        {
            tmin = MathF.Max(tmin, bound);
        }
        else
        {
            tmax = MathF.Min(tmax, bound);
        }
        return tmax > tmin + Epsilon;
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

    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        var a = rowStart;
        var b = rowStart + width * dx;
        return !SegmentFullyInsideOriginalRect(a, b, cushion);
    }

    private bool SegmentFullyInsideOriginalRect(WPos a, WPos b, float cushion)
    {
        bool Inside(WPos p)
        {
            var px = p.X - originX;
            var pz = p.Z - originZ;
            var parr = px * dirX + pz * dirZ;
            var ortho = px * normalX + pz * normalZ;
            return parr <= lenFront + cushion + Epsilon && parr >= -(lenBack + cushion) - Epsilon
            && ortho <= halfWidth + cushion + Epsilon && ortho >= -(halfWidth + cushion) - Epsilon;
        }
        return Inside(a) && Inside(b);
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

    // Row-cull: segment vs capsule (segment swept disk)
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        var aX = rowStart.X;
        var aZ = rowStart.Z;
        var b = rowStart + width * dx;
        var bX = b.X;
        var bZ = b.Z;

        var q0X = originX;
        var q0Z = originZ;
        var q1X = originX + length * dirX;
        var q1Z = originZ + length * dirZ;

        var r = radius + cushion;
        var r2 = r * r;
        var dist2 = SegmentSegmentDistanceSquared(aX, aZ, bX, bZ, q0X, q0Z, q1X, q1Z);
        return dist2 <= r2 + Epsilon;
    }

    // Closest distance^2 between 2D segments P=[p0,p1], Q=[q0,q1]
    private static float SegmentSegmentDistanceSquared(float p0x, float p0z, float p1x, float p1z, float q0x, float q0z, float q1x, float q1z)
    {
        // Based on Ericson RTCD 5.1.9 (clamped)
        var ux = p1x - p0x;
        var uz = p1z - p0z;
        var vx = q1x - q0x;
        var vz = q1z - q0z;
        var wx = p0x - q0x;
        var wz = p0z - q0z;
        var a = ux * ux + uz * uz; // ||u||^2
        var b = ux * vx + uz * vz; // u·v
        var c = vx * vx + vz * vz; // ||v||^2
        var d = ux * wx + uz * wz; // u·w
        var e = vx * wx + vz * wz; // v·w
        float s, t;
        var D = a * c - b * b; // denom

        if (a <= Epsilon && c <= Epsilon)
        {
            // both segments degenerate
            return (p0x - q0x) * (p0x - q0x) + (p0z - q0z) * (p0z - q0z);
        }
        if (a <= Epsilon)
        {
            // P degenerate -> point to segment Q
            s = 0f;
            t = Clamp01(e / c);
        }
        else if (c <= Epsilon)
        {
            // Q degenerate -> point to segment P
            t = 0f;
            s = Clamp01(-d / a);
        }
        else
        {
            s = Clamp01((b * e - c * d) / D);
            t = (b * s + e) / c;
            if (t < 0f)
            {
                t = 0f;
                s = Clamp01(-d / a);
            }
            else if (t > 1f)
            {
                t = 1f;
                s = Clamp01((b - d) / a);
            }
        }

        var dx = p0x + s * ux - (q0x + t * vx);
        var dz = p0z + s * uz - (q0z + t * vz);
        return dx * dx + dz * dz;

        static float Clamp01(float x) => x < 0f ? 0f : (x > 1f ? 1f : x);
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

    // Inverted: row intersects unless fully inside (inflated) capsule — convex ⇒ endpoints check
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        return !(PointInsideCapsule(rowStart, cushion) && PointInsideCapsule(rowStart + width * dx, cushion));
    }

    private bool PointInsideCapsule(WPos p, float cushion)
    {
        var q0x = originX;
        var q0z = originZ;
        var q1x = originX + length * dirX;
        var q1z = originZ + length * dirZ;
        var r = radius + cushion;
        var r2 = r * r;
        var d2 = PointSegmentDistanceSquared(p.X, p.Z, q0x, q0z, q1x, q1z);
        return d2 <= r2 + Epsilon;
    }

    private static float PointSegmentDistanceSquared(float px, float pz, float q0x, float q0z, float q1x, float q1z)
    {
        var vx = q1x - q0x;
        var vz = q1z - q0z;
        var wx = px - q0x;
        var wz = pz - q0z;
        var vv = vx * vx + vz * vz;
        if (vv <= Epsilon)
        {
            var dx = px - q0x;
            var dz = pz - q0z;
            return dx * dx + dz * dz;
        }
        var t = (wx * vx + wz * vz) / vv;
        if (t < 0f)
        {
            t = 0f;
        }
        else if (t > 1f)
        {
            t = 1f;
        }
        var cx = q0x + t * vx;
        var cz = q0z + t * vz;
        var dx2 = px - cx;
        var dz2 = pz - cz;
        return dx2 * dx2 + dz2 * dz2;
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

    // Row intersects union of two rectangles (axis-aligned in {direction,normal} frame)
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        var a = rowStart;
        var b = rowStart + width * dx;
        return SegmentIntersectsRect(a, b, direction, normal, length, halfWidth, origin, cushion)
        || SegmentIntersectsRect(a, b, normal, direction, length, halfWidth, origin, cushion);
    }

    private static bool SegmentIntersectsRect(WPos a, WPos b, WDir dir, WDir nrm, float len, float halfW, WPos origin, float cushion)
    {
        var a_ = a;
        var aX_ = a_.X;
        var aZ_ = a_.Z;
        var ax = aX_ - origin.X;
        var az = aZ_ - origin.Z;
        var dx = b.X - aX_;
        var dz = b.Z - aZ_;
        var dirX = dir.X;
        var dirZ = dir.Z;
        var nX = nrm.X;
        var nZ = nrm.Z;
        float tmin = 0f, tmax = 1f;

        return ClipLE(dirX, dirZ, ax, az, dx, dz, len + cushion, ref tmin, ref tmax)
        && ClipGE(dirX, dirZ, ax, az, dx, dz, -(len + cushion), ref tmin, ref tmax)
        && ClipLE(nX, nZ, ax, az, dx, dz, halfW + cushion, ref tmin, ref tmax)
        && ClipGE(nX, nZ, ax, az, dx, dz, -(halfW + cushion), ref tmin, ref tmax)
        && tmax > tmin + Epsilon;
    }

    private static bool ClipLE(float nX, float nZ, float ax, float az, float dx, float dz, float value, ref float tmin, ref float tmax)
    {
        var s0 = nX * ax + nZ * az;
        var s1 = nX * dx + nZ * dz;
        if (MathF.Abs(s1) <= Epsilon)
        {
            return s0 <= value + Epsilon;
        }
        var bound = (value - s0) / s1;
        if (s1 > 0f)
        {
            tmax = MathF.Min(tmax, bound);
        }
        else
        {
            tmin = MathF.Max(tmin, bound);
        }
        return tmax > tmin + Epsilon;
    }

    private static bool ClipGE(float nX, float nZ, float ax, float az, float dx, float dz, float value, ref float tmin, ref float tmax)
    {
        var s0 = nX * ax + nZ * az;
        var s1 = nX * dx + nZ * dz;
        if (MathF.Abs(s1) <= Epsilon)
        {
            return s0 >= value - Epsilon;
        }
        var bound = (value - s0) / s1;
        if (s1 > 0f)
        {
            tmin = MathF.Max(tmin, bound);
        }
        else
        {
            tmax = MathF.Min(tmax, bound);
        }
        return tmax > tmin + Epsilon;
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

    // inverted: row intersects unless fully inside the union (i.e., fully in arm P or fully in arm O)
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        // conservative: may over-include, never miss
        var a = rowStart;
        var b = rowStart + width * dx;
        return !SegmentFullyInsideRect(a, b, direction, normal, length, halfWidth, origin, cushion)
        && !SegmentFullyInsideRect(a, b, normal, direction, length, halfWidth, origin, cushion);
    }

    private static bool SegmentFullyInsideRect(WPos a, WPos b, WDir dir, WDir nrm, float len, float halfW, WPos origin, float cushion)
    {
        bool Inside(WPos p)
        {
            var rx = p.X - origin.X;
            var rz = p.Z - origin.Z;
            var dirX = dir.X;
            var dirZ = dir.Z;
            var nX = nrm.X;
            var nZ = nrm.Z;
            var parr = dirX * rx + dirZ * rz;
            var ortho = nX * rx + nZ * rz;
            return parr <= len + cushion + Epsilon && parr >= -(len + cushion) - Epsilon
            && ortho <= halfW + cushion + Epsilon && ortho >= -(halfW + cushion) - Epsilon;
        }
        return Inside(a) && Inside(b); // convex ⇒ endpoints suffice
    }
}

public sealed class SDConvexPolygon : ShapeDistance
{
    private readonly bool cw;
    private readonly (WPos a, WPos b)[] edges;

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

    public override float Distance(WPos p)
    {
        var minDistance = float.MaxValue;
        var inside = true;
        var len = edges.Length;
        var point = p;
        for (var i = 0; i < len; ++i)
        {
            ref var edge = ref edges[i];
            var a = edge.a;
            var b = edge.b;
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

    // Segment vs convex polygon (inflated by cushion). Rotation-agnostic.
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default)
    {
        var a = rowStart;
        var b = rowStart + width * dx;
        var tmin = 0f;
        var tmax = 1f;

        var dirx = b.X - a.X;
        var dirz = b.Z - a.Z;
        var lenEdges = edges.Length;

        // Degenerate segment → point-in-inflated-polygon test by half-planes
        if (MathF.Abs(dirx) + MathF.Abs(dirz) <= Epsilon)
        {
            for (var i = 0; i < lenEdges; ++i)
            {
                ref var edge = ref edges[i];
                var eA = edge.a;
                var eB = edge.b;
                var ex = eB.X - eA.X;
                var ez = eB.Z - eA.Z;
                var len = MathF.Sqrt(ex * ex + ez * ez);
                if (len <= Epsilon)
                {
                    continue;
                }
                var off = cushion * len;
                var s0 = ex * (a.Z - eA.Z) - ez * (a.X - eA.X);
                if (cw)
                {
                    if (s0 > off + Epsilon)
                    {
                        return false;
                    }
                }
                else
                {
                    if (s0 < -off - Epsilon)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        for (var i = 0; i < lenEdges; ++i)
        {
            ref var edge = ref edges[i];
            var eA = edge.a;
            var eB = edge.b;
            var ex = eB.X - eA.X;
            var ez = eB.Z - eA.Z;
            var len = MathF.Sqrt(ex * ex + ez * ez);
            if (len <= Epsilon)
            {
                continue; // skip degenerate
            }
            var off = cushion * len; // scale cushion by edge length to avoid normalization

            // s(t) = cross(ex,ez, (a - eA) + t*(b - a)) = s0 + t*s1
            var s0 = ex * (a.Z - eA.Z) - ez * (a.X - eA.X);
            var s1 = ex * dirz - ez * dirx;

            if (cw)
            {
                // s(t) <= off
                if (!ClipLE_Unnormalized(s0, s1, off, ref tmin, ref tmax))
                {
                    return false;
                }
            }
            else
            {
                // s(t) >= -off
                if (!ClipGE_Unnormalized(s0, s1, -off, ref tmin, ref tmax))
                {
                    return false;
                }
            }
        }
        return tmax > tmin + Epsilon;
    }

    private static bool ClipLE_Unnormalized(float s0, float s1, float value, ref float tmin, ref float tmax)
    {
        if (MathF.Abs(s1) <= Epsilon)
        {
            return s0 <= value + Epsilon;
        }
        var bound = (value - s0) / s1;
        if (s1 > 0f)
        {
            tmax = MathF.Min(tmax, bound);
        }
        else
        {
            tmin = MathF.Max(tmin, bound);
        }
        return tmax > tmin + Epsilon;
    }

    private static bool ClipGE_Unnormalized(float s0, float s1, float value, ref float tmin, ref float tmax)
    {
        if (MathF.Abs(s1) <= Epsilon)
        {
            return s0 >= value - Epsilon;
        }
        var bound = (value - s0) / s1;
        if (s1 > 0f)
        {
            tmin = MathF.Max(tmin, bound);
        }
        else
        {
            tmax = MathF.Min(tmax, bound);
        }
        return tmax > tmin + Epsilon;
    }
}
