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
            .ActivateOnEnter<CutbackBlazePersistent>()
            .ActivateOnEnter<AlleyOopInfernoSpread>()
            .ActivateOnEnter<AlleyOopInfernoPuddles>()
            .ActivateOnEnter<AlleyOopMaelstromAOEs>()
            .ActivateOnEnter<CutbackBlaze>()
            .ActivateOnEnter<DiversDare>()
            .ActivateOnEnter<DeepImpact>()
            .ActivateOnEnter<DeepVarial1>()
            .ActivateOnEnter<SickestTakeOff1>()
            .ActivateOnEnter<SickSwell1>()
            .ActivateOnEnter<XtremeSpectacular3>()
            .ActivateOnEnter<XtremeSpectacular4>()
            .ActivateOnEnter<PyrotationStack>()
            
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && (_module.DeepBlue?.IsDeadOrDestroyed ?? true);
    }
}
