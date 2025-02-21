namespace BossMod.Endwalker.VariantCriterion.V02MR.V024Shishio;

class Levinburst(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(5f, 20f, 5f);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Rairin)
            _aoes.Add(new(rect, WPos.ClampToGrid(actor.Position), actor.Rotation, WorldState.FutureTime(6.9d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Levinburst)
            _aoes.Clear();
    }
}
