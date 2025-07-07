namespace BossMod.Endwalker.VariantCriterion.V02MR.V024Shishio;

sealed class V024ShishioStates : StateMachineBuilder
{
    public V024ShishioStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            // Route 8
            .ActivateOnEnter<ThunderVortex>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<Vasoconstrictor>()
            // Route 9
            .ActivateOnEnter<FocusedTremorYokiUzu>()
            // Route 10
            .ActivateOnEnter<RightLeftSwipe>()
            // Route 11
            .ActivateOnEnter<Reisho1>()
            .ActivateOnEnter<Reisho2>()
            // Standard
            .ActivateOnEnter<UnsagelySpinYokiThunderOneTwoThreefold>()
            .ActivateOnEnter<NoblePursuit>()
            .ActivateOnEnter<Levinburst>()
            .ActivateOnEnter<Enkyo>()
            .ActivateOnEnter<OnceTwiceThriceRokujo>()
            .ActivateOnEnter<SplittingCry>()
            .ActivateOnEnter<CloudToCloud1>()
            .ActivateOnEnter<CloudToCloud2>()
            .ActivateOnEnter<CloudToCloud3>()
            .ActivateOnEnter<Rokujo>()
        ;
    }
}