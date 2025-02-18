namespace BossMod.Dawntrail.Raid.M04NWickedThunder;

class WitchHunt(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(25);
    private static readonly AOEShapeCircle circle = new(6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        List<AOEInstance> aoes = new(count);
        var countH = count / 2;
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i < countH)
                aoes.Add(aoe with { Color = count > 2 ? Colors.Danger : 0 });
            else if (i >= countH)
                aoes.Add(aoe with { Risky = count < 12 });
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WitchHuntTelegraph)
            _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 6.3f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && (AID)spell.Action.ID == AID.WitchHunt)
            _aoes.RemoveAt(0);
    }
}
