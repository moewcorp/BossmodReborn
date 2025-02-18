namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

class BindingSigil(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle _shape = new(9f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var firstactivation = _aoes[0].Activation;
        var aoes = new AOEInstance[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if ((aoe.Activation - firstactivation).TotalSeconds < 1d)
                aoes[index++] = aoe;
        }
        return aoes[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BindingSigilPreview)
        {
            _aoes.Add(new(_shape, spell.LocXZ, default, Module.CastFinishAt(spell, 9.6f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SoulBinding)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
                _aoes.RemoveAt(0);
        }
    }
}
