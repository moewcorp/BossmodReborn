namespace BossMod;

[SkipLocalsInit]
public sealed class SDKnockbackTowardsOriginPlusAOECirclesPlusAABBSquareIntersection(WPos Center, float Distance, WPos[] AOEs, float Radius, WPos CenterTile, float TileHalfWidth, int Length) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly float distance = Distance;
    private readonly WPos[] aoes = AOEs;
    private readonly float radius = Radius;
    private readonly WPos centerTile = CenterTile;
    private readonly float tileHalfWidth = TileHalfWidth;
    private readonly int len = Length;

    public override bool Contains(in WPos p)
    {
        var offsetCenter = p - center;
        var length = offsetCenter.Length();
        var lengthAdj = length > distance ? distance : length;
        var offsetSource = length > 0f ? -offsetCenter / length : default;
        var projected = lengthAdj * offsetSource;
        for (var i = 0; i < len; ++i)
        {
            if ((p + projected).InCircle(aoes[i], radius))
            {
                return true;
            }
        }

        return Intersect.RayAABB(centerTile - center, offsetSource, tileHalfWidth, tileHalfWidth) <= lengthAdj;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}
