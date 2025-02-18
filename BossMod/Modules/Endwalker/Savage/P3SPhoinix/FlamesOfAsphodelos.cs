namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to flames of asphodelos mechanic
class FlamesOfAsphodelos(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(60, 30.Degrees());
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i < 2)
                yield return count > 2 ? aoe with { Color = Colors.Danger } : aoe;
            else if (i is > 1 and < 4)
                yield return aoe;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FlamesOfAsphodelosAOE1 or AID.FlamesOfAsphodelosAOE2 or AID.FlamesOfAsphodelosAOE3)
        {
            _aoes.Add(new(cone, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            if (_aoes.Count == 6)
                _aoes.SortBy(x => x.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && (AID)spell.Action.ID is AID.FlamesOfAsphodelosAOE1 or AID.FlamesOfAsphodelosAOE2 or AID.FlamesOfAsphodelosAOE3)
            _aoes.RemoveAt(0);
    }
}
