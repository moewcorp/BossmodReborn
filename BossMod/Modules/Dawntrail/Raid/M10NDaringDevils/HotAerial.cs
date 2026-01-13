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
    private static readonly float PuddleDelay = 1.5f;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // Only show once "activation" time has passed
        var now = WorldState.CurrentTime;

        // NOTE: Returning a freshly built list avoids trying to mutate AOEInstance.
        // If perf ever matters, we can switch to a cached temp list.
        List<AOEInstance> res = [];
        foreach (var p in _puddles)
            if (now >= p.Activation)
                res.Add(p);

        return CollectionsMarshal.AsSpan(res);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.HotAerial1:
                _puddles.Add(new AOEInstance(
                    Shape,
                    spell.TargetXZ,
                    default,
                    WorldState.CurrentTime.AddSeconds(PuddleDelay),
                    Colors.AOE,
                    true));
                break;

            case (uint)AID.DiversDare:
            case (uint)AID.DiversDare1:
                _puddles.Clear();
                break;
        }
    }
}
