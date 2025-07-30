namespace BossMod.Stormblood.Extreme.Ex8Seiryu;

sealed class Ex8SeiryuStates : StateMachineBuilder
{
    public Ex8SeiryuStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<OnmyoSerpentEyeSigil>()
            .ActivateOnEnter<SerpentAscending>()
        ;
    }

    private void SinglePhase(uint id)
    {
        SimpleState(id + 0xFF0000u, 10000, "???");
    }

    //private void XXX(uint id, float delay)
}
