namespace BossMod.Endwalker.VariantCriterion.V02MR.V024Shishio;

class V024ShishioStates : StateMachineBuilder
{
    public V024ShishioStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            // Route 8
            .ActivateOnEnter<ThunderVortex>()
            .ActivateOnEnter<UnsagelySpin>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<Vasoconstrictor>()
            // Route 9
            .ActivateOnEnter<Yoki>()
            .ActivateOnEnter<FocusedTremorYokiUzu>()
            // Route 10
            .ActivateOnEnter<LeftSwipe>()
            .ActivateOnEnter<RightSwipe>()
            // Route 11
            .ActivateOnEnter<Reisho1>()
            .ActivateOnEnter<Reisho2>()
            // Standard
            .ActivateOnEnter<ThunderOnefold>()
            .ActivateOnEnter<ThunderTwofold>()
            .ActivateOnEnter<ThunderThreefold>()
            .ActivateOnEnter<NoblePursuit>()
            .ActivateOnEnter<Levinburst>()
            .ActivateOnEnter<Enkyo>()
            .ActivateOnEnter<OnceOnRokujo>()
            .ActivateOnEnter<ThriceOnRokujo>()
            .ActivateOnEnter<TwiceOnRokujo>()
            .ActivateOnEnter<SplittingCry>()
            .ActivateOnEnter<CloudToCloud1>()
            .ActivateOnEnter<CloudToCloud2>()
            .ActivateOnEnter<CloudToCloud3>()
            .ActivateOnEnter<Rokujo>()
        ;
    }
}