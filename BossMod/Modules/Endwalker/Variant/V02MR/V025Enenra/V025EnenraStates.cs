namespace BossMod.Endwalker.VariantCriterion.V02MR.V025Enenra;

class V025EnenraStates : StateMachineBuilder
{
    public V025EnenraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<PipeCleaner>()
            .ActivateOnEnter<Uplift>()
            .ActivateOnEnter<Snuff>()
            .ActivateOnEnter<Smoldering>()
            .ActivateOnEnter<IntoTheFire>()
            .ActivateOnEnter<FlagrantCombustion>()
            .ActivateOnEnter<SmokeRings>()
            .ActivateOnEnter<ClearingSmoke>()
            .ActivateOnEnter<StringRock>();
    }
}
