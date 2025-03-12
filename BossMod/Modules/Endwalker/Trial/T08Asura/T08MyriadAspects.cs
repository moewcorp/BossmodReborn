namespace BossMod.Endwalker.Trial.T08Asura;

class MyriadAspects(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(40f, 15f.Degrees());
    private readonly List<AOEInstance> _aoes = new(12);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 6 ? 6 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.MyriadAspects1 or (uint)AID.MyriadAspects2)
        {
            _aoes.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            if (_aoes.Count == 12)
                _aoes.SortBy(x => x.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.MyriadAspects1 or (uint)AID.MyriadAspects2)
            _aoes.RemoveAt(0);
    }
}
