namespace BossMod.Dawntrail.Quantum.FinalVerse.Q40EminentGrief;

[SkipLocalsInit]
sealed class Q40EminentGriefStates : StateMachineBuilder
{
    public Q40EminentGriefStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<ScourgingBlaze>()
            .ActivateOnEnter<TerrorEyeVoidTrapBallOfFire>()
            ;
    }

    private void SinglePhase(uint id)
    {
        SimpleState(id + 0xFF0000u, 10000f, "???");
    }

    //private void XXX(uint id, float delay)
}
