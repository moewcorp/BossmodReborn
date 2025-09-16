﻿namespace BossMod;
// radius is the largest horizontal/vertical dimension: radius for circle, max of width/height for rect
// note: this class to represent *relative* arena bounds (relative to arena center) - the reason being that in some cases effective center moves every frame, and bounds caches a lot (clip poly & base map for pathfinding)
// note: if arena bounds are changed, new instance is recreated; max approx error can change without recreating the instance
public abstract class ArenaBounds(float radius, float mapResolution, float scaleFactor = 1f, bool allowObstacleMap = false)
{
    public readonly float Radius = radius;
    public readonly float MapResolution = mapResolution;
    public readonly float ScaleFactor = scaleFactor;
    public readonly bool AllowObstacleMap = allowObstacleMap;

    // fields below are used for clipping & drawing borders
    public readonly PolygonClipper Clipper = new();
    public float MaxApproxError;
    public RelSimplifiedComplexPolygon ShapeSimplified = new();
    public List<RelTriangle> ShapeTriangulation = [];
    private readonly PolygonClipper.Operand _clipOperand = new();
    public readonly Dictionary<object, object> Cache = [];

    public float ScreenHalfSize
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                MaxApproxError = CurveApprox.ScreenError / value * Radius;
                ShapeSimplified = Clipper.Simplify(BuildClipPoly());
                ShapeTriangulation = ShapeSimplified.Triangulate();
                _clipOperand.Clear();
                _clipOperand.AddPolygon(ShapeSimplified); // note: I assume using simplified shape as an operand is better than raw one
            }
        }
    }

    protected abstract PolygonClipper.Operand BuildClipPoly();
    public abstract void PathfindMap(Pathfinding.Map map, WPos center);
    public abstract bool Contains(WDir offset);
    public abstract float IntersectRay(WDir originOffset, WDir dir);
    public abstract WDir ClampToBounds(WDir offset);

    // functions for clipping various shapes to bounds; all shapes are expected to be defined relative to bounds center
    public List<RelTriangle> ClipAndTriangulate(ReadOnlySpan<WDir> poly) => Clipper.Intersect(new PolygonClipper.Operand(poly), _clipOperand).Triangulate();
    public List<RelTriangle> ClipAndTriangulate(RelSimplifiedComplexPolygon poly) => Clipper.Intersect(new(poly), _clipOperand).Triangulate();

    public List<RelTriangle> ClipAndTriangulateCone(WDir centerOffset, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle)
    {
        // TODO: think of a better way to do that (analytical clipping?)
        if (innerRadius >= outerRadius || innerRadius < 0f || halfAngle.Rad <= 0f)
            return [];

        var fullCircle = halfAngle.Rad >= MathF.PI;
        var donut = innerRadius != 0;
        var points = (donut, fullCircle) switch
        {
            (false, false) => CurveApprox.CircleSector(outerRadius, centerDirection - halfAngle, centerDirection + halfAngle, MaxApproxError),
            (false, true) => CurveApprox.Circle(outerRadius, MaxApproxError),
            (true, false) => CurveApprox.DonutSector(innerRadius, outerRadius, centerDirection - halfAngle, centerDirection + halfAngle, MaxApproxError),
            (true, true) => CurveApprox.Donut(innerRadius, outerRadius, MaxApproxError),
        };
        var len = points.Length;
        var offset = centerOffset;
        for (var i = 0; i < len; ++i)
        {
            points[i] += offset;
        }
        return ClipAndTriangulate(points);
    }

    public List<RelTriangle> ClipAndTriangulateCircle(WDir centerOffset, float radius)
    {
        var points = CurveApprox.Circle(radius, MaxApproxError);
        var len = points.Length;
        var offset = centerOffset;
        for (var i = 0; i < len; ++i)
        {
            points[i] += offset;
        }
        return ClipAndTriangulate(points);
    }

    public List<RelTriangle> ClipAndTriangulateCapsule(WDir centerOffset, WDir direction, float radius, float length)
    {
        var points = CurveApprox.Capsule(direction, length, radius, MaxApproxError);
        var len = points.Length;
        var offset = centerOffset;
        for (var i = 0; i < len; ++i)
        {
            points[i] += offset;
        }
        return ClipAndTriangulate(points);
    }

    public List<RelTriangle> ClipAndTriangulateDonut(WDir centerOffset, float innerRadius, float outerRadius)
    {
        if (innerRadius < outerRadius && innerRadius >= 0f)
        {
            var points = CurveApprox.Donut(innerRadius, outerRadius, MaxApproxError);
            var len = points.Length;
            var offset = centerOffset;
            for (var i = 0; i < len; ++i)
            {
                points[i] += offset;
            }
            return ClipAndTriangulate(points);
        }
        return [];
    }

    public List<RelTriangle> ClipAndTriangulateTri(WDir oa, WDir ob, WDir oc)
        => ClipAndTriangulate([oa, ob, oc]);

    public List<RelTriangle> ClipAndTriangulateIsoscelesTri(WDir apexOffset, WDir height, WDir halfBase)
        => ClipAndTriangulateTri(apexOffset, apexOffset + height + halfBase, apexOffset + height - halfBase);

    public List<RelTriangle> ClipAndTriangulateIsoscelesTri(WDir apexOffset, Angle direction, Angle halfAngle, float height)
    {
        var dir = direction.ToDirection();
        var normal = dir.OrthoL();
        return ClipAndTriangulateIsoscelesTri(apexOffset, height * dir, height * halfAngle.Tan() * normal);
    }

    public List<RelTriangle> ClipAndTriangulateRect(WDir originOffset, WDir direction, float lenFront, float lenBack, float halfWidth)
    {
        var side = halfWidth * direction.OrthoR();
        var front = originOffset + lenFront * direction;
        var back = originOffset - lenBack * direction;
        return ClipAndTriangulate([front + side, front - side, back - side, back + side]);
    }

    public List<RelTriangle> ClipAndTriangulateRect(WDir originOffset, Angle direction, float lenFront, float lenBack, float halfWidth)
        => ClipAndTriangulateRect(originOffset, direction.ToDirection(), lenFront, lenBack, halfWidth);

    public List<RelTriangle> ClipAndTriangulateRect(WDir startOffset, WDir endOffset, float halfWidth)
    {
        var dir = (endOffset - startOffset).Normalized();
        var side = halfWidth * dir.OrthoR();
        return ClipAndTriangulate([startOffset + side, startOffset - side, endOffset - side, endOffset + side]);
    }

    public void AddToInstanceCache(object key, object value)
    {
        if (Cache.Count > 500)
            Cache.Clear();
        Cache[key] = value;
    }
}

public sealed class ArenaBoundsCircle(float Radius, float MapResolution = 0.5f, bool AllowObstacleMap = false) : ArenaBounds(Radius, MapResolution, allowObstacleMap: AllowObstacleMap)
{
    private Pathfinding.Map? _cachedMap;

    protected override PolygonClipper.Operand BuildClipPoly() => new(CurveApprox.Circle(Radius, MaxApproxError));
    public override void PathfindMap(Pathfinding.Map map, WPos center) => map.Init(_cachedMap ??= BuildMap(), center);
    public override bool Contains(WDir offset)
    {
        var radius = Radius;
        return offset.LengthSq() <= radius * radius;
    }
    public override float IntersectRay(WDir originOffset, WDir dir) => Intersect.RayCircle(originOffset, dir, Radius);

    public override WDir ClampToBounds(WDir offset)
    {
        var radius = Radius;
        if (offset.LengthSq() > radius * radius)
        {
            offset *= radius / offset.Length();
        }
        return offset;
    }

    private Pathfinding.Map BuildMap()
    {
        var radius = Radius;
        var map = new Pathfinding.Map(MapResolution, default, radius, radius);
        var iCell = 0;

        var width = map.Width;
        var height = map.Height;
        var resolution = map.Resolution;

        var pixelMaxG = map.PixelMaxG;
        var pixelPriority = map.PixelPriority;

        var threshold = radius * radius / (resolution * resolution); // square of bounds radius, in grid coordinates
        var dy = -height / 2 + 0.5f;
        var dx = -width / 2 + 0.5f;

        for (var y = 0; y < height; ++y, ++dy)
        {
            var cy = Math.Abs(dy) + 0.5f; // farthest corner
            var cySq = cy * cy;
            var dx2 = dx;
            for (var x = 0; x < width; ++x, ++dx2)
            {
                var cx = Math.Abs(dx2) + 0.5f;
                if (cx * cx + cySq > threshold)
                {
                    pixelMaxG[iCell] = -1000f;
                    pixelPriority[iCell] = float.MinValue;
                }
                ++iCell;
            }
        }
        return map;
    }

    public override string ToString() => $"{nameof(ArenaBoundsCircle)}, Radius {Radius}, MapResolution: {MapResolution}";
}

// if rotation is 0, half-width is along X and half-height is along Z
public abstract class ABRect : ArenaBounds
{
    public ABRect(float halfWidth, float halfHeight, Angle rotation = default, float MapResolution = 0.5f, bool AllowObstacleMap = false) : base(Math.Max(halfWidth, halfHeight), MapResolution, rotation != default ? CalculateScaleFactor(rotation) : 1f, AllowObstacleMap)
    {
        HalfWidth = halfWidth;
        HalfHeight = halfHeight;
        Rotation = rotation;
        Orientation = Rotation.ToDirection();
    }
    public readonly float HalfWidth;
    public readonly float HalfHeight;
    public readonly Angle Rotation;
    private Pathfinding.Map? _cachedMap;
    public readonly WDir Orientation;

    private static float CalculateScaleFactor(Angle Rotation)
    {
        var (sin, cos) = MathF.SinCos(Rotation.Rad);
        return Math.Abs(cos) + Math.Abs(sin);
    }

    protected override PolygonClipper.Operand BuildClipPoly() => new(CurveApprox.Rect(Orientation, HalfWidth, HalfHeight));
    public override void PathfindMap(Pathfinding.Map map, WPos center) => map.Init(_cachedMap ??= BuildMap(), center);

    private Pathfinding.Map BuildMap()
    {
        var halfWidth = HalfWidth;
        var halfHeight = HalfHeight;
        var map = new Pathfinding.Map(MapResolution, default, halfWidth + 0.5f, halfHeight + 0.5f, Rotation); // +0.5 offset because otherwise the AI will not run back into the rectangle for some reason
        // pixels can be partially covered by the rectangle, so we need to rasterize it carefully
        var width = map.Width;
        var height = map.Height;
        var resolution = map.Resolution;
        var pixelMaxG = map.PixelMaxG;
        var pixelPriority = map.PixelPriority;

        var dir = Rotation.ToDirection();
        var dirX = dir.X;
        var dirZ = dir.Z;
        var normal = dir.OrthoL();
        var normalX = normal.X;
        var normalZ = normal.Z;

        var dx = normal * resolution;
        var dy = dir * resolution;
        var startPos = map.Center - ((width >> 1) - 0.5f) * dx - ((height >> 1) - 0.5f) * dy;
        var halfPixel = 0.5f * resolution;

        for (var y = 0; y < height; ++y)
        {
            var posY = startPos + y * dy;
            var rowBase = y * width;
            for (var x = 0; x < width; ++x)
            {
                var pos = posY + x * dx;
                var pX = pos.X;
                var pZ = pos.Z;

                var distParr = pX * dirX + pZ * dirZ;
                var distOrtho = pX * normalX + pZ * normalZ;

                if (!((distParr - halfPixel) >= -halfHeight && (distParr + halfPixel) <= halfHeight) || !((distOrtho - halfPixel) >= -halfWidth && (distOrtho + halfPixel) <= halfWidth))
                {
                    pixelMaxG[rowBase + x] = -1000f;
                    pixelPriority[rowBase + x] = float.MinValue;
                }
            }
        }

        return map;
    }

    public override bool Contains(WDir offset) => offset.InRect(Orientation, HalfHeight, HalfHeight, HalfWidth);
    public override float IntersectRay(WDir originOffset, WDir dir) => Intersect.RayRect(originOffset, dir, Orientation, HalfWidth, HalfHeight);

    public override WDir ClampToBounds(WDir offset)
    {
        var orientation = Orientation;
        var halfWidth = HalfWidth;
        var halfHeight = HalfHeight;
        var offsetX = offset.Dot(orientation.OrthoL());
        var offsetY = offset.Dot(orientation);
        if (Math.Abs(offsetX) > halfWidth)
        {
            offsetX = Math.Sign(offsetX) * halfWidth;
        }
        if (Math.Abs(offsetY) > halfHeight)
        {
            offsetY = Math.Sign(offsetY) * halfHeight;
        }
        return orientation.OrthoL() * offsetX + orientation * offsetY;
    }
}

public sealed class ArenaBoundsRect(float halfWidth, float halfHeight, Angle rotation = default, float mapResolution = 0.5f, bool allowObstacleMap = false) : ABRect(halfWidth, halfHeight, rotation, mapResolution, allowObstacleMap)
{
    public override string ToString() => $"{nameof(ArenaBoundsRect)}, Radius {Radius}, HalfWidth: {HalfWidth}, HalfHeight: {HalfHeight}, MapResolution: {MapResolution}, ScaleFactor: {ScaleFactor}";
}
public sealed class ArenaBoundsSquare(float halfWidth, Angle rotation = default, float mapResolution = 0.5f, bool allowObstacleMap = false) : ABRect(halfWidth, halfWidth, rotation, mapResolution, allowObstacleMap)
{
    public override string ToString() => $"{nameof(ArenaBoundsSquare)}, Radius {Radius}, HalfWidth: {HalfWidth}, MapResolution: {MapResolution}, ScaleFactor: {ScaleFactor}";
}

// custom complex polygon bounds
// for creating complex bounds by using arrays of shapes
// first array contains platforms that will be united, second optional array contains shapes that will be subtracted
// for convenience third array will optionally perform additional unions at the end
// offset shrinks the pathfinding map only, for example if the edges of the arena are deadly and floating point errors cause the AI to fall of the map or problems like that
// AdjustForHitbox adjusts both the visible map and the pathfinding map
public sealed class ArenaBoundsCustom : ArenaBounds
{
    private Pathfinding.Map? _cachedMap;
    public readonly RelSimplifiedComplexPolygon Polygon;
    private readonly (WDir, WDir)[] edges;
    public readonly float HalfWidth, HalfHeight;
    private readonly float offset;
    public readonly WPos Center;
    public bool IsCircle; // can be used by gaze component for gazes outside of the arena

    public ArenaBoundsCustom(Shape[] UnionShapes, Shape[]? DifferenceShapes = null, Shape[]? AdditionalShapes = null, float MapResolution = 0.5f, float ScaleFactor = 1f, bool AllowObstacleMap = false, float Offset = default, bool AdjustForHitbox = false)
    : base(BuildBounds(UnionShapes, DifferenceShapes ?? [], AdditionalShapes ?? [], ScaleFactor, AdjustForHitbox, out var poly, out var center, out var halfWidth, out var halfHeight), MapResolution, ScaleFactor, AllowObstacleMap)
    {
        Center = center;
        HalfWidth = halfWidth + Offset;
        HalfHeight = halfHeight + Offset;
        Polygon = poly;
        offset = Offset;

        var parts = Polygon.Parts;
        var count = parts.Count;
        var vertsCount = 0;
        for (var i = 0; i < count; ++i)
        {
            vertsCount += parts[i].VerticesCount;
        }
        var edgeArray = new (WDir, WDir)[vertsCount];
        var k = 0;
        for (var i = 0; i < count; ++i)
        {
            var part = parts[i];
            var extSpan = part.ExteriorEdges;
            extSpan.CopyTo(edgeArray.AsSpan(k, extSpan.Length));
            k += extSpan.Length;
            var len = part.Holes.Length;
            for (var j = 0; j < len; ++j)
            {
                var intSpan = part.InteriorEdges(j);
                intSpan.CopyTo(edgeArray.AsSpan(k, intSpan.Length));
                k += intSpan.Length;
            }
        }
        edges = edgeArray;
    }

    private static float BuildBounds(Shape[] unionShapes, Shape[]? differenceShapes, Shape[]? additionalShapes, float scalefactor, bool adjustForHitbox, out RelSimplifiedComplexPolygon poly, out WPos center, out float halfWidth, out float halfHeight)
    {
        var properties = CalculatePolygonProperties(unionShapes, differenceShapes ?? [], additionalShapes ?? [], adjustForHitbox);
        center = properties.Center;
        halfWidth = properties.HalfWidth;
        halfHeight = properties.HalfHeight;
        poly = properties.Poly;
        return scalefactor == 1f ? properties.Radius : properties.Radius / scalefactor;
    }

    private static (WPos Center, float HalfWidth, float HalfHeight, float Radius, RelSimplifiedComplexPolygon Poly) CalculatePolygonProperties(Shape[] unionShapes, Shape[] differenceShapes, Shape[] additionalShapes, bool adjustForHitbox)
    {
        var unionPolygons = ParseShapes(unionShapes);
        var differencePolygons = ParseShapes(differenceShapes);
        var additionalPolygons = ParseShapes(additionalShapes);
        var combine = CombinePolygons(unionPolygons, differencePolygons, additionalPolygons);
        var combinedPoly = adjustForHitbox ? combine.Offset(-0.5f, Clipper2Lib.JoinType.Round) : combine;

        float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;
        var combined = combinedPoly.Parts;
        var countCombined = combined.Count;
        for (var i = 0; i < countCombined; ++i)
        {
            var parts = combined[i].Exterior;
            var len = parts.Length;
            for (var j = 0; j < len; ++j)
            {
                var vertex = parts[j];
                var vX = vertex.X;
                var vZ = vertex.Z;
                if (vX < minX)
                {
                    minX = vX;
                }
                if (vX > maxX)
                {
                    maxX = vX;
                }
                if (vZ < minZ)
                {
                    minZ = vZ;
                }
                if (vZ > maxZ)
                {
                    maxZ = vZ;
                }
            }
        }

        var center = new WPos((minX + maxX) * 0.5f, (minZ + maxZ) * 0.5f);
        var centerX = center.X;
        var centerZ = center.Z;
        var maxDistX = Math.Max(Math.Abs(maxX - centerX), Math.Abs(minX - centerX));
        var maxDistZ = Math.Max(Math.Abs(maxZ - centerZ), Math.Abs(minZ - centerZ));
        var halfWidth = (maxX - minX) * 0.5f;
        var halfHeight = (maxZ - minZ) * 0.5f;
        var dir = center.ToWDir();

        for (var i = 0; i < countCombined; ++i)
        {
            var verts = CollectionsMarshal.AsSpan(combined[i].Vertices);
            var len = verts.Length;
            for (var j = 0; j < len; ++j)
            {
                ref var vert = ref verts[j];
                vert -= dir;
            }
        }

        return (center, halfWidth, halfHeight, Math.Max(maxDistX, maxDistZ), combinedPoly);

        static RelSimplifiedComplexPolygon[] ParseShapes(Shape[] shapes)
        {
            var lenght = shapes.Length;
            var polygons = new RelSimplifiedComplexPolygon[lenght];
            for (var i = 0; i < lenght; ++i)
            {
                polygons[i] = shapes[i].ToPolygon(default);
            }
            return polygons;
        }
    }

    protected override PolygonClipper.Operand BuildClipPoly() => new(Polygon);
    public override void PathfindMap(Pathfinding.Map map, WPos center) => map.Init(_cachedMap ??= BuildMap(), center);

    public override bool Contains(WDir offset) => Polygon.Contains(offset);

    public override float IntersectRay(WDir originOffset, WDir dir)
    {
        var cacheKey = (Polygon, originOffset, dir);
        if (Cache.TryGetValue(cacheKey, out var cachedResult))
            return (float)cachedResult;
        var result = Intersect.RayPolygon(originOffset, dir, Polygon);
        AddToInstanceCache(cacheKey, result);
        return result;
    }

    public override WDir ClampToBounds(WDir offset)
    {
        if (offset.AlmostEqual(default, 1f) || Math.Abs(offset.X) < 0.1f) // if actor is almost in the center of the arena, do nothing (eg donut arena or wall boss)
        {
            return offset;
        }

        var cacheKey = (Polygon, offset);
        if (Cache.TryGetValue(cacheKey, out var cachedResult))
            return (WDir)cachedResult;

        var minDistance = float.MaxValue;
        var nearestPoint = offset;
        var len = edges.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var edge = ref edges[i];
            var edge1 = edge.Item1;
            var segmentVector = edge.Item2 - edge1;
            var t = Math.Max(0, Math.Min(1f, (offset - edge1).Dot(segmentVector) / segmentVector.LengthSq()));
            var nearest = edge1 + t * segmentVector;
            var distance = (nearest - offset).LengthSq();

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPoint = nearest;
            }
        }

        AddToInstanceCache(cacheKey, nearestPoint);
        return nearestPoint;
    }

    private Pathfinding.Map BuildMap()
    {
        var polygon = offset != default ? Polygon.Offset(offset) : Polygon;
        var map = new Pathfinding.Map(MapResolution, default, HalfWidth, HalfHeight);

        var pixelMaxG = map.PixelMaxG;
        var pixelPriority = map.PixelPriority;
        var width = map.Width;
        var height = map.Height;
        var resolution = map.Resolution;

        var shape = new SDInvertedPolygonWithHoles(new(default, Polygon));
        // now check the corners
        var halfSample = resolution * 0.49999f; // tiny offset to account for floating point inaccuracies

        WDir[] sampleOffsets =
        [
            new(-halfSample, -halfSample),
            new(-halfSample,  halfSample),
            new(halfSample, -halfSample),
            new(halfSample, halfSample)
        ];

        var dx = new WDir(resolution, default);
        var dy = new WDir(default, resolution);
        var startPos = map.Center - ((width >> 1) - 0.5f) * dx - ((height >> 1) - 0.5f) * dy;
        var partitioner = Partitioner.Create(0, height);

        Parallel.ForEach(partitioner, range =>
        {
            var ys = range.Item1;
            var ye = range.Item2;
            for (var y = ys; y < ye; ++y)
            {
                var rowOffset = y * width;
                var posY = startPos + y * dy;
                for (var x = 0; x < width; ++x)
                {
                    var offset = rowOffset + x;
                    var pos = posY + x * dx;
                    if (shape.Distance(pos) <= halfSample) // inner circle of the pixel
                    {
                        pixelMaxG[offset] = -1000f; // no reason to check more points of the cell
                        pixelPriority[offset] = float.MinValue;
                        continue;
                    }
                    var relativeCenter = new WDir(pos.X, pos.Z);

                    for (var i = 0; i < 4; ++i)
                    {
                        if (!polygon.Contains(relativeCenter + sampleOffsets[i]))
                        {
                            pixelMaxG[offset] = -1000f;
                            pixelPriority[offset] = float.MinValue;
                            break;
                        }
                    }
                }
            }
        });
        return map;
    }

    private static RelSimplifiedComplexPolygon CombinePolygons(RelSimplifiedComplexPolygon[] unionPolygons, RelSimplifiedComplexPolygon[] differencePolygons, RelSimplifiedComplexPolygon[] secondUnionPolygons)
    {
        var clipper = new PolygonClipper();
        var operandUnion = new PolygonClipper.Operand();
        var operandDifference = new PolygonClipper.Operand();
        var operandSecondUnion = new PolygonClipper.Operand();

        var unionLen = unionPolygons.Length;
        for (var i = 0; i < unionLen; ++i)
        {
            operandUnion.AddPolygon(unionPolygons[i]);
        }
        var differenceLen = differencePolygons.Length;
        for (var i = 0; i < differenceLen; ++i)
        {
            operandDifference.AddPolygon(differencePolygons[i]);
        }
        var secUnionLen = secondUnionPolygons.Length;
        for (var i = 0; i < secUnionLen; ++i)
        {
            operandSecondUnion.AddPolygon(secondUnionPolygons[i]);
        }

        var combinedShape = clipper.Difference(operandUnion, operandDifference);
        if (secUnionLen != 0)
        {
            combinedShape = clipper.Union(new PolygonClipper.Operand(combinedShape), operandSecondUnion);
        }

        return combinedShape;
    }

    public override string ToString() => $"{nameof(ArenaBoundsCustom)}, Radius {Radius}, HalfWidth: {HalfWidth}, HalfHeight: {HalfHeight}, MapResolution: {MapResolution}, Pathfinding offset: {offset}, Vertices: {edges.Length}, ScaleFactor: {ScaleFactor}";
}
