namespace BossMod.Stormblood.Trial.T09Seiryu;

class OnmyoSerpentEyeSigil(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeDonut donut = new(7f, 30f);
    private static readonly AOEShapeCircle circle = new(12f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        void AddAOE(AOEShape shape) => _aoe = new(shape, WPos.ClampToGrid(actor.Position), default, WorldState.FutureTime(5.6d));
        if (modelState == 32)
            AddAOE(circle);
        else if (modelState == 33)
            AddAOE(donut);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.OnmyoSigil2 or (uint)AID.SerpentEyeSigil2)
            _aoe = null;
    }
}
