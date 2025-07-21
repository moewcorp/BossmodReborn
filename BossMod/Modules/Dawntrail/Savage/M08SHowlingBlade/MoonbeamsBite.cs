namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

sealed class MoonbeamsBite(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(40f, 10f);
    private readonly List<AOEInstance> _aoes = new(4);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var max = count > 2 ? 2 : count;
        if (count > 1)
        {
            ref var aoe0 = ref aoes[0];
            aoe0.Color = Colors.Danger;
        }
        return aoes[..max];
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.MoonlitShadow && modelState is 6 or 7)
        {
            var rot = actor.Rotation;
            _aoes.Add(new(rect, (actor.Position + (modelState == 7u ? 1f : -1f) * 10f * (rot + 90f.Degrees()).ToDirection()).Quantized(), rot, WorldState.FutureTime(11.1d + 1d * _aoes.Count)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // ensure pixel perfectness (even though the prediction should have less than 0.001y error)
        if (spell.Action.ID is (uint)AID.MoonbeamsBite1 or (uint)AID.MoonbeamsBite2)
        {
            var count = _aoes.Count;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var pos = spell.LocXZ;
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Origin.AlmostEqual(pos, 1f))
                {
                    aoe.Origin = pos;
                    aoe.Rotation = spell.Rotation;
                    return;
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.MoonbeamsBite1 or (uint)AID.MoonbeamsBite2)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }
}
