namespace BossMod.Shadowbringers.Alliance.A31KnaveofHearts;

sealed class BoxSpawn(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BoxSpawn, new AOEShapeRect(8f, 4f));

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    public readonly List<Square> SquaresInflated = new(4);
    public readonly List<Square> Squares = new(4);
    private DateTime lastUpdate;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BoxSpawn)
        {
            SquaresInflated.Add(new(caster.Position, 4.5f)); // squares adjusted for player hitbox radius
            Squares.Add(new(caster.Position, 4f));
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.ArenaFeatures)
        {
            if (state == 0x00010002u)
            {
                Arena.Bounds = new ArenaBoundsComplex(A31KnaveofHearts.BaseSquare, [.. SquaresInflated]);
                SquaresInflated.Clear();
                lastUpdate = WorldState.CurrentTime;
            }
            else if (state == 0x00040008u && WorldState.CurrentTime > lastUpdate.AddSeconds(1d)) // clearing old squares can happen in the same frame as new squares got added
            {
                Arena.Bounds = A31KnaveofHearts.DefaultArena;
            }
        }
    }
}
