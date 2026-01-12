namespace BossMod.DawnTrail.Raid.M10NDaringDevils;

// Towers: 4 intercardinal, require at least 1 soaker.
sealed class HotAerialTowers(BossModule module) : Components.CastTowers(
    module,
    (uint)AID.HotAerial2,
    radius: 6f,
    minSoakers: 1,
    maxSoakers: 8,
    damageType: AIHints.PredictedDamageType.Raidwide);

// Persistent fire puddles left behind after Red Hot jumps to each tower location.
sealed class HotAerialFirePuddles(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _puddles = [];
    private static readonly AOEShapeCircle Shape = new(6f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => CollectionsMarshal.AsSpan(_puddles);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            // boss jump target locations (no cast) -> puddle appears
            case (uint)AID.HotAerial1:
                _puddles.Add(new(Shape, spell.TargetXZ, default, WorldState.CurrentTime, Colors.AOE, true));
                break;

            // arena cleanup
            case (uint)AID.DiversDare:
            case (uint)AID.DiversDare1:
                _puddles.Clear();
                break;
        }
    }
}
