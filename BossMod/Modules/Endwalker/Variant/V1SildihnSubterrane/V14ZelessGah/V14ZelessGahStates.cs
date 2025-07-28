namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V14ZelessGah;

sealed class V14ZelessGahStates : StateMachineBuilder
{
    public V14ZelessGahStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<ArcaneFont>()
            .ActivateOnEnter<InfernGale>()
            .ActivateOnEnter<TrespassersPyre>()
            .ActivateOnEnter<BallOfFire>()
            .ActivateOnEnter<BlazingBenifice>()
            .ActivateOnEnter<PureFire>()
            .ActivateOnEnter<CastShadow>()
            .ActivateOnEnter<FiresteelFracture>()
            .ActivateOnEnter<ShowOfStrength>();
    }
}
