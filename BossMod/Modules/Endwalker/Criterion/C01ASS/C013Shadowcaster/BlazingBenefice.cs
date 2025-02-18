namespace BossMod.Endwalker.VariantCriterion.C01ASS.C013Shadowcaster;

abstract class BlazingBenifice(BossModule module, AID aid, OID oid) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(100f, 5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        List<AOEInstance> aoes = new(count);
        var time = WorldState.CurrentTime;
        var act0 = _aoes[0].Activation;
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if ((aoe.Activation - time).TotalSeconds <= 5d && (aoe.Activation - act0).TotalSeconds <= 1d)
                aoes.Add(aoe);
        }
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)oid)
            _aoes.Add(new(rect, actor.Position, actor.Rotation, WorldState.FutureTime(21.7d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)aid)
            _aoes.RemoveAt(0);
    }
}

class NBlazingBenifice(BossModule module) : BlazingBenifice(module, AID.NBlazingBenifice, OID.NArcaneFont);
class SBlazingBenifice(BossModule module) : BlazingBenifice(module, AID.SBlazingBenifice, OID.SArcaneFont);