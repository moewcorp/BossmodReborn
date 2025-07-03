namespace BossMod.Endwalker.VariantCriterion.V02MR.V023Gorai;

sealed class V023GoraiStates : StateMachineBuilder
{
    public V023GoraiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            // Route 5
            .ActivateOnEnter<PureShock>()
            // Route 6
            .ActivateOnEnter<HumbleHammer>()
            .ActivateOnEnter<Thundercall>()
            // Route 7
            .ActivateOnEnter<WorldlyPursuit>()
            .ActivateOnEnter<FightingSpirits>()
            .ActivateOnEnter<BiwaBreaker>()
            // Standard
            .ActivateOnEnter<ImpurePurgation>()
            .ActivateOnEnter<StringSnap>()
            .ActivateOnEnter<SpikeOfFlameAOE>()
            .ActivateOnEnter<FlameAndSulphur>()
            .ActivateOnEnter<TorchingTorment>()
            .ActivateOnEnter<MalformedPrayer>()
            .ActivateOnEnter<Unenlightenment>();
    }
}
