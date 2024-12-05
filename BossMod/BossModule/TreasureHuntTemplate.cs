namespace BossMod;

// for treasure hunt roulettes
public abstract class THTemplate(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(100, 100), 19, 48)]);
}
