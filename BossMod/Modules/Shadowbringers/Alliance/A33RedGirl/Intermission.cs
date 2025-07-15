namespace BossMod.Shadowbringers.Alliance.A33RedGirl;

sealed class IntermissionArena(BossModule module) : BossComponent(module)
{
    private readonly List<Rectangle> walls = new(8);
    private readonly A33RedGirl bossmod = (A33RedGirl)module;
    private PolygonCustomO[] baseArena = [];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.WhiteWall or (uint)OID.BlackWall)
        {
            walls.Add(new Rectangle(actor.Position, 2.125f, 1.5f));
            if (walls.Count == 8)
            {
                baseArena = bossmod.RedSphere!.Position.Z switch
                {
                    900f => A33RedGirl.VirusArena1,
                    400f => A33RedGirl.VirusArena2,
                    _ => A33RedGirl.VirusArena3
                };
                ArenaBoundsComplex arena = new(baseArena, [.. walls]);
                Arena.Bounds = arena;
                Arena.Center = arena.Center;
            }
        }
    }

    public override void OnActorDeath(Actor actor)
    {
        if (actor.OID is (uint)OID.WhiteWall or (uint)OID.BlackWall)
        {
            var count = walls.Count;
            var pos = actor.Position;
            for (var i = 0; i < count; ++i)
            {
                if (walls[i].Center == pos)
                {
                    walls.RemoveAt(i);
                    break;
                }
            }
            Arena.Bounds = new ArenaBoundsComplex(baseArena, [.. walls]);
        }
    }
}

sealed class WaveWhite(BossModule module) : Components.CastHint(module, (uint)AID.WaveWhite, "Be white to avoid damage!");
sealed class WaveBlack(BossModule module) : Components.CastHint(module, (uint)AID.WaveBlack, "Be black to avoid damage!");
sealed class BigExplosion(BossModule module) : Components.CastHint(module, (uint)AID.BigExplosion, "Pylons explode!", true);
