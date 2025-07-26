namespace BossMod.Endwalker.VariantCriterion.V01SildihnSubterrane.V014ZelessGah;

class V014ZelessGahStates : StateMachineBuilder
{
    public V014ZelessGahStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Burn>()
            .ActivateOnEnter<BlazingBenifice>()
            .ActivateOnEnter<PureFire2>()
            .ActivateOnEnter<CastShadow>()
            .ActivateOnEnter<FiresteelFracture>()
            .ActivateOnEnter<InfernGaleKnockback>()
            .ActivateOnEnter<ShowOfStrength>();
    }
}
