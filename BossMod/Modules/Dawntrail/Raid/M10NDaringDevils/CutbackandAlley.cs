namespace BossMod.DawnTrail.Raid.M10NDaringDevils;

// Cutback Blaze Mechanic needs fixing.

sealed class CutbackBlaze(BossModule module) : Components.BaitAwayCast(
    module,
    (uint)AID.CutbackBlaze,
    Cone,
    centerAtTarget: false, // important: origin at boss
    endsOnCastEvent: true)
{
    public static readonly AOEShapeCone Cone = new(60f, 22.5f.Degrees());
}

// Working persistent AOEs for Cutback Blaze cones.
sealed class CutbackBlazePersistent(BossModule module) : Components.GenericAOEs(module, (uint)AID.CutbackBlaze1)
{
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CutbackBlaze1:
                // NOTE: constructor signature in BossModReborn is (shape, pos, rot, activation, color, risky, actorID)
                _aoes.Add(new(CutbackBlaze.Cone, caster.Position, caster.Rotation, WorldState.CurrentTime, Colors.AOE, true, caster.InstanceID));
                break;

            // Clear when Divers' Dare resolves (event cast is effectively "cast end" for NPCs)
            case (uint)AID.DiversDare:
            case (uint)AID.DiversDare1:
                _aoes.Clear();
                break;
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        var id = actor.InstanceID;
        _aoes.RemoveAll(a => a.ActorID == id);
    }
}
// Spread markers (helper -> player, 5s cast, range 5 circle)
sealed class AlleyOopInfernoSpread(BossModule module)
    : Components.SpreadFromCastTargets(module, (uint)AID.AlleyOopInferno1, 5f);

// Persistent puddles from Alley-oop Inferno (spread resolves -> leaves puddle; burns if you linger).
// Finally Working!
sealed class AlleyOopInfernoPuddles(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _puddles = [];
    private static readonly AOEShapeCircle Shape = new(6f); // tweak if needed

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => CollectionsMarshal.AsSpan(_puddles);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AlleyOopInferno1: // 46471 helper -> player
            {
                // Create a puddle under each affected target at their CURRENT position (reliable)
                foreach (var t in spell.Targets)
                {
                    var target = WorldState.Actors.Find(t.ID);
                    if (target != null)
                        _puddles.Add(new(Shape, target.Position, default, WorldState.CurrentTime, Colors.AOE, true));
                }
                break;
            }

            case (uint)AID.DiversDare:
            case (uint)AID.DiversDare1:
                _puddles.Clear();
                break;
        }
    }
}

// Alley-oop Maelstrom cones. There are 2 cone widths in enums (30 and 15 degrees).
// Helper cones (actual damaging AOEs) - To be refined, maybe using maxCasts?
sealed class AlleyOopMaelstrom30(BossModule module) : Components.SimpleAOEs(
    module,
    (uint)AID.AlleyOopMaelstrom,
    new AOEShapeCone(60f, 30f.Degrees()));

sealed class AlleyOopMaelstrom15(BossModule module) : Components.SimpleAOEs(
    module,
    (uint)AID.AlleyOopMaelstrom2,
    new AOEShapeCone(60f, 15f.Degrees()));
