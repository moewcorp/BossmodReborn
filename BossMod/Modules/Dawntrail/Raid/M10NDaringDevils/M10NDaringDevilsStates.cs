namespace BossMod.DawnTrail.Raid.M10NDaringDevils;

[SkipLocalsInit]
sealed class M10NDaringDevilsStates : StateMachineBuilder
{
    private readonly M10NDaringDevils _module;

    public M10NDaringDevilsStates(M10NDaringDevils module) : base(module)
    {
        _module = module;
        TrivialPhase()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && (_module.DeepBlue?.IsDeadOrDestroyed ?? true);
    }
}
