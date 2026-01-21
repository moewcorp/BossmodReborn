namespace BossMod.Dawntrail.Savage.M11STheTyrant;

static class ArenaChanges
{
    private const float CenterX = 100f;
    private const float CenterZ = 100f;

    private const float ArenaHalfSize = 25f;
    private const float GapHalfWidth = 5f;

    // Full arena: 40x40 square centered at (100, 100)
    private static readonly Shape[] BaseArenaShapes =
    [
        new PolygonCustom([
            new(CenterX - ArenaHalfSize, CenterZ - ArenaHalfSize),
            new(CenterX - ArenaHalfSize, CenterZ + ArenaHalfSize),
            new(CenterX + ArenaHalfSize, CenterZ + ArenaHalfSize),
            new(CenterX + ArenaHalfSize, CenterZ - ArenaHalfSize),
        ])
    ];

    // Removed middle strip (north–south), splitting east/west
    private static readonly Shape[] RemovedMiddle =
    [
        new PolygonCustom([
            new(CenterX - GapHalfWidth, CenterZ - ArenaHalfSize),
            new(CenterX - GapHalfWidth, CenterZ + ArenaHalfSize),
            new(CenterX + GapHalfWidth, CenterZ + ArenaHalfSize),
            new(CenterX + GapHalfWidth, CenterZ - ArenaHalfSize),
        ])
    ];

    // Initial arena (no split)
    public static readonly ArenaBoundsCustom InitialArena =
        new(BaseArenaShapes);

    // Arena after Flatliner
    public static readonly ArenaBoundsCustom SplitArena =
        new(BaseArenaShapes, RemovedMiddle);
}