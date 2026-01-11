namespace BossMod.DawnTrail.Raid.M10NDaringDevils;

[SkipLocalsInit]
sealed class M10NDaringDevilsStates : StateMachineBuilder
{
    private readonly M10NDaringDevils _module;

    public M10NDaringDevilsStates(M10NDaringDevils module) : base(module)
    {
        _module = module;

        TrivialPhase()
            .ActivateOnEnter<HotImpact>()
            .ActivateOnEnter<DeepImpact>()
            .ActivateOnEnter<DiversDare>()
            .ActivateOnEnter<DiversDareBlue>()

            .ActivateOnEnter<CutbackBlaze>()
            .ActivateOnEnter<CutbackBlazePersistent>()

            .ActivateOnEnter<AlleyOopInfernoSpread>()
            .ActivateOnEnter<AlleyOopInfernoPuddles>()
            .ActivateOnEnter<AlleyOopMaelstrom30>()
            .ActivateOnEnter<AlleyOopMaelstrom15>()


            .ActivateOnEnter<DeepVarialCone>()
            .ActivateOnEnter<SickestTakeOffLine>()
            .ActivateOnEnter<SickSwellKB>()

            .ActivateOnEnter<PyrotationStack>()
            .ActivateOnEnter<XtremeSpectacularHits>()

            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && (_module.DeepBlue?.IsDeadOrDestroyed ?? true);
    }
}
