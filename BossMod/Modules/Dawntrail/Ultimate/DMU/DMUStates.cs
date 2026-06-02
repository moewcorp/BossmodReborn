namespace BossMod.Dawntrail.Ultimate.DMU;

[SkipLocalsInit]
sealed class KefkaStates : StateMachineBuilder
{
    public KefkaStates(BossModule module) : base(module) {
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        SimpleState(id + 0xFF0000u, 10000f, "???")
            //.ActivateOnEnter<RevoltingRuin>();
            .ActivateOnEnter<GravenImage>();
        //    .ActivateOnEnter<BlizzardSafeSpot>()
        //    .ActivateOnEnter<StackSpreadOrbs>();
        //.ActivateOnEnter<BlizzardIIIBlowoutCone>()
        //.ActivateOnEnter<BlizzardIIIBlowoutCone2nd>();
    }

    //private void XXX(uint id, float delay)
}
