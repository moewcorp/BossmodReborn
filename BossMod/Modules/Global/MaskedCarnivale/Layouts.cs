namespace BossMod.Global.MaskedCarnivale;

public static class Layouts
{
    public static readonly WPos ArenaCenter = new(100, 100);
    private static readonly Angle a45 = 45.Degrees();
    private static readonly Polygon[] _circleBig = [new Polygon(ArenaCenter, 24.5f * CosPI.Pi32th, 32)];
    private static readonly Rectangle[] walls = [new(new(90, 94.75f), 5.5f, 0.75f), new(new(94.75f, 91.75f), 0.75f, 3.25f),
    new(new(110, 94.75f), 5.5f, 0.75f), new(new(105.25f, 91.75f), 0.75f, 3.25f)];

    private const float sideLength = 2.4f;
    private static readonly Square[] squares = [new(new(110, 110), sideLength, a45), new(new(90, 110), sideLength, a45),
    new(new(110, 90), sideLength, a45), new(new(90, 90), sideLength, a45)];

    public static readonly ArenaBoundsComplex Layout4Quads = new(_circleBig, squares);
    public static readonly ArenaBoundsComplex Layout2Corners = new(_circleBig, walls);
    public static readonly ArenaBoundsComplex LayoutBigQuad = new(_circleBig, [new Square(ArenaCenter, 5.4f, a45)]);
    public static readonly ArenaBoundsComplex CircleSmall = new ArenaBoundsComplex([new Polygon(ArenaCenter, 16 * CosPI.Pi32th, 32)]) with { IsCircle = true };
    public static readonly ArenaBoundsComplex CircleBig = new ArenaBoundsComplex(_circleBig) with { IsCircle = true };
}
