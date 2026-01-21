namespace BossMod.Dawntrail.Savage.M11STheTyrant;

sealed class CometTethers(BossModule module) : Components.TankbusterTether(module, (uint)AID.ForegoneFatality, (uint)TetherID.CometTether, 0f);
sealed class CosmicKiss(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.CosmicKissIcon, (uint)AID.CosmicKiss, 4f, 5d);
sealed class MeteorainComets(BossModule module) : BossComponent(module)
{
    // Reborn default arena center / inactive helper location
    private static readonly WPos InvalidPos = new(100f, 100f);
    // Squared tolerance (~0.1y). Avoids fragile float equality.
    private const float InvalidPosEpsilonSq = 0.01f;
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // Query authoritative world state every frame
        var comets = Module.Enemies((uint)OID.Comet);
        var count = comets.Count;

        for (int i = 0; i < count; ++i)
        {
            var c = comets[i];
            if (c.IsDead)
                continue;

            // Skip comets that are still at the engine default location
            if ((c.Position - InvalidPos).LengthSq() < InvalidPosEpsilonSq)
                continue;

            // Draw the comet body (terrain object)
            Arena.AddCircle(c.Position, c.HitboxRadius, Colors.Object);
        }
    }
}

sealed class TripleTyrannhilation(BossModule module)
    : Components.CastLineOfSightAOE(module, (uint)AID.TripleTyrannhilation1, 60f)
{
    public override ReadOnlySpan<Actor> BlockerActors()
    {
        var comets = Module.Enemies((uint)OID.Comet);
        var count = comets.Count;
        if (count == 0)
            return default;

        var blockers = new List<Actor>();
        for (var i = 0; i < count; ++i)
        {
            var c = comets[i];
            if (!c.IsDead)
                blockers.Add(c);
        }

        return CollectionsMarshal.AsSpan(blockers);
    }
}
