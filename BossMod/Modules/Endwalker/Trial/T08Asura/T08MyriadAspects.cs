namespace BossMod.Endwalker.Trial.T08Asura;

class MyriadAspects(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(40, 15.Degrees());
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(6);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MyriadAspects1 or AID.MyriadAspects2)
        {
            _aoes.Add(new(cone, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            if (_aoes.Count == 12)
                _aoes.SortBy(x => x.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && (AID)spell.Action.ID is AID.MyriadAspects1 or AID.MyriadAspects2)
            _aoes.RemoveAt(0);
    }
}
