namespace BossMod.Endwalker.VariantCriterion.C01ASS.C013Shadowcaster;

class CastShadow(BossModule module) : Components.GenericAOEs(module)
{
    public List<Actor> FirstAOECasters = [];
    public List<Actor> SecondAOECasters = [];

    private static readonly AOEShape _shape = new AOEShapeCone(65f, 15f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var countFirst = FirstAOECasters.Count;
        var countSecond = SecondAOECasters.Count;
        var total = countFirst + countSecond;
        if (total == 0)
            return [];
        var casters = countFirst != 0 ? FirstAOECasters : SecondAOECasters;
        var current = countFirst != 0 ? countFirst : countSecond;
        var aoes = new AOEInstance[current];
        for (var i = 0; i < current; ++i)
        {
            var caster = casters[i];
            aoes[i] = new AOEInstance(_shape, caster.Position, caster.CastInfo!.Rotation, Module.CastFinishAt(caster.CastInfo));
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        ListForAction(spell.Action)?.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        ListForAction(spell.Action)?.Remove(caster);
    }

    private List<Actor>? ListForAction(ActionID action) => action.ID switch
    {
        (uint)AID.NCastShadowAOE1 or (uint)AID.SCastShadowAOE1 => FirstAOECasters,
        (uint)AID.NCastShadowAOE2 or (uint)AID.SCastShadowAOE2 => SecondAOECasters,
        _ => null
    };
}
