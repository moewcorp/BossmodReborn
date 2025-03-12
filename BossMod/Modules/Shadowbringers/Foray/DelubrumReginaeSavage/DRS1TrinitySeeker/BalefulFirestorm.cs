namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker;

// TODO: consider showing something before clones jump?
class BalefulFirestorm(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _shape = new(50f, 10f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 3 ? 3 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BalefulComet:
                var delay = 7.6d + _aoes.Count;
                _aoes.Add(new(_shape, WPos.ClampToGrid(caster.Position), caster.Rotation, WorldState.FutureTime(delay), ActorID: caster.InstanceID));
                break;
            case (uint)AID.BalefulFirestorm:
                var count = _aoes.Count;
                var id = caster.InstanceID;
                for (var i = 0; i < count; ++i)
                {
                    if (_aoes[i].ActorID == id)
                    {
                        _aoes.RemoveAt(i);
                        return;
                    }
                }
                ++NumCasts;
                break;
        }
    }
}
