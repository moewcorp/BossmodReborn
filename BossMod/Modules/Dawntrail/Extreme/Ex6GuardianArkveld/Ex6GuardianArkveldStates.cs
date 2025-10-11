namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

sealed class Ex6GuardianArkveldStates : StateMachineBuilder
{
    public Ex6GuardianArkveldStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<Roar>()
            .ActivateOnEnter<ForgedFury>()
            .ActivateOnEnter<ChainbladeBlow>()
            .ActivateOnEnter<GuardianWyvernsSiegeflight>()
            .ActivateOnEnter<WhiteFlash>()
            .ActivateOnEnter<Dragonspark>()
            .ActivateOnEnter<WildEnergy>()
            .ActivateOnEnter<WyvernsRadianceGuardianResonanceCircle>()
            .ActivateOnEnter<WyvernsRadianceExaflare1>()
            .ActivateOnEnter<WyvernsRadianceExaflare2>()
            .ActivateOnEnter<WyvernsRadianceRush>()
            .ActivateOnEnter<WyvernsRadianceConcentric>()
            .ActivateOnEnter<WyvernsOuroblade>()
            .ActivateOnEnter<SteeltailThrust>()
            .ActivateOnEnter<GuardianResonanceTowers>()
            .ActivateOnEnter<WyvernsRadianceCrackedCrystal>()
            .ActivateOnEnter<WyvernsVengeance>()
            .ActivateOnEnter<WyvernsRadianceChainbladeCharge>()
            .ActivateOnEnter<WyvernsWeal>()
            .ActivateOnEnter<WyvernsWealAOE>()
            .ActivateOnEnter<ClamorousChaseBait>()
            .ActivateOnEnter<ClamorousChaseAOE>()
            .ActivateOnEnter<WyvernsRadianceCleave>();
    }

    private void SinglePhase(uint id)
    {
        SimpleState(id + 0xFF0000u, 10000, "???");
    }

    //private void XXX(uint id, float delay)
}
