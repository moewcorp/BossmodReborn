namespace BossMod.Dawntrail.Trial.T03QueenEternal;

class RoyalBanishment(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(7);
    private static readonly AOEShapeCone cone = new(100f, 15f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var deadline = _aoes[0].Activation.AddSeconds(1d);

        var index = 0;
        while (index < count && _aoes[index].Activation < deadline)
            ++index;

        return CollectionsMarshal.AsSpan(_aoes)[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RoyalBanishment)
            _aoes.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.RoyalBanishment)
            _aoes.RemoveAt(0);
    }
}
