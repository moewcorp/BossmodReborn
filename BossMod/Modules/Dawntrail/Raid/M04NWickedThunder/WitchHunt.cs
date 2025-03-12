namespace BossMod.Dawntrail.Raid.M04NWickedThunder;

class WitchHunt(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(25);
    private static readonly AOEShapeCircle circle = new(6f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        var countH = count / 2;
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i < countH)
                aoes[i] = aoe with { Color = count > 2 ? Colors.Danger : 0 };
            else
                aoes[i] = count < 12 ? aoe : aoe with { Risky = false };
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WitchHuntTelegraph)
            _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 6.3f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.WitchHunt)
            _aoes.RemoveAt(0);
    }
}
