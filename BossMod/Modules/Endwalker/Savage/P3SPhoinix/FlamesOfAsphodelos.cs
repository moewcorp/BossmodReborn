namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to flames of asphodelos mechanic
class FlamesOfAsphodelos(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(60f, 30f.Degrees());
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 4 ? 4 : count;
        var aoes = new AOEInstance[max];

        for (var i = 0; i < max; ++i)
        {
            var aoe = _aoes[i];
            if (i < 2)
                aoes[i] = count > 2 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe with { Risky = false };
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FlamesOfAsphodelosAOE1 or (uint)AID.FlamesOfAsphodelosAOE2 or (uint)AID.FlamesOfAsphodelosAOE3)
        {
            _aoes.Add(new(cone, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            if (_aoes.Count == 6)
                _aoes.SortBy(x => x.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.FlamesOfAsphodelosAOE1 or (uint)AID.FlamesOfAsphodelosAOE2 or (uint)AID.FlamesOfAsphodelosAOE3)
            _aoes.RemoveAt(0);
    }
}
