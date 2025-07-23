namespace BossMod.Endwalker.VariantCriterion.V01SildihnSubterrane.V12Silkie;

sealed class V12SilkieStates : StateMachineBuilder
{
    public V12SilkieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DustBlusterKnockback>()
            .ActivateOnEnter<WashOutKnockback>()
            .ActivateOnEnter<BracingDuster>()
            .ActivateOnEnter<ChillingDuster>()
            .ActivateOnEnter<SlipperySoap>()
            .ActivateOnEnter<SpotRemover>()
            .ActivateOnEnter<SqueakyCleanConeSmall>()
            .ActivateOnEnter<SqueakyCleanConeBig>()
            .ActivateOnEnter<PuffAndTumble>()
            .ActivateOnEnter<CarpetBeater>()
            .ActivateOnEnter<EasternEwers>()
            .ActivateOnEnter<TotalWash>();
    }
}
