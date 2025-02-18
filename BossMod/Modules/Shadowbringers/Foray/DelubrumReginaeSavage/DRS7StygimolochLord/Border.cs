namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

class Border(BossModule module) : Components.GenericAOEs(module)
{
    private bool Active;
    private static readonly WPos ArenaCenter = new(-416, -184);
    private static readonly Polygon[] DefaultPolygon = [new(ArenaCenter, 34.5f, 48)];
    public static readonly ArenaBoundsComplex DefaultBounds = new(DefaultPolygon, [new Rectangle(new(-416, -219), 20, 1.4f), new Rectangle(new(-416, -149), 20, 1.25f)]);
    private static readonly Shape[] labyrinthDifference = [new DonutV(ArenaCenter, 30, 34.5f, 48), new DonutV(ArenaCenter, 17, 25, 48), new Polygon(ArenaCenter, 12, 48)];
    private static readonly Rectangle[] labyrinthUnion = [.. GenerateAlcoves(new(-416, -211.5f)), .. GenerateAlcoves(WPos.RotateAroundOrigin(22.5f, ArenaCenter, new(-416, -198.5f)), 22.5f.Degrees())];
    private static readonly ArenaBoundsComplex labPhase = new(DefaultPolygon, labyrinthDifference, labyrinthUnion);
    private static readonly AOEShapeCustom customShape = new(labyrinthDifference, labyrinthUnion);

    private static List<Rectangle> GenerateAlcoves(WPos basePosition, Angle start = default)
    {
        var a45 = 45.Degrees();

        List<Rectangle> rects = new(8)
        {
            new(basePosition, 2, 4, start)
        };

        for (var i = 1; i < 8; ++i)
            rects.Add(new(WPos.RotateAroundOrigin(i * 45, ArenaCenter, basePosition), 2, 4, start + a45 * i));
        return rects;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Active)
            yield return new(customShape, Arena.Center);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MemoryOfTheLabyrinth)
        {
            Active = true;
            Arena.Bounds = labPhase;
            Arena.Center = labPhase.Center;
        }
    }
}
