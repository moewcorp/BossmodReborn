namespace BossMod.Dawntrail.Trial.T03QueenEternal;

class RoyalBanishment(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(7);
    private static readonly AOEShapeCone cone = new(100, 15.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if ((aoe.Activation - _aoes[0].Activation).TotalSeconds < 1)
                aoes[index++] = aoe;
        }
        return aoes[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RoyalBanishment)
            _aoes.Add(new(cone, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && (AID)spell.Action.ID == AID.RoyalBanishment)
            _aoes.RemoveAt(0);
    }
}
