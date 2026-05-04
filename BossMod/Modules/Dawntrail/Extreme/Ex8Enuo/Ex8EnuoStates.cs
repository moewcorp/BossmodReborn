namespace BossMod.Modules.Dawntrail.Extreme.Ex8Enuo;

[SkipLocalsInit]

sealed class Ex8EnuoStates : StateMachineBuilder
{
    public Ex8EnuoStates(Ex8Enuo module) : base(module)
    {
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Cast(id, (uint)AID.Meteorain, 9.16f, 5f, "Meteorain").ActivateOnEnter<Meteorain>();
        TempState(id + 0x010000, 2f);
    }

    private void TempState(uint id, float delay)
    {
        Cast(id, (uint)AID.NaughtGrows, 7.44f, 7f, "Naught Grows 1")
            .ActivateOnEnter<NaughtGrowsWildCharge>();
    }

    //private void XXX(uint id, float delay)
}
