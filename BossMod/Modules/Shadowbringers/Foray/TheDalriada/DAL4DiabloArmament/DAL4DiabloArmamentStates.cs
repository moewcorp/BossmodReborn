namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL4DiabloArmament;

sealed class DAL4DiabloArmamentStates : StateMachineBuilder
{
    public DAL4DiabloArmamentStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AdvancedDeathIV>()
            //.ActivateOnEnter<AdvancedNox>() not displaying properly
            .ActivateOnEnter<AssaultCannon>()
            .ActivateOnEnter<DeadlyDealingAOE>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<LightPseudopillarAOE>()
            .ActivateOnEnter<PillarOfShamash>()
            .ActivateOnEnter<UltimatePseudoterror>()
            .ActivateOnEnter<AdvancedDeathIV>()
            .ActivateOnEnter<PillarOfShamashBait>()
            .ActivateOnEnter<PillarOfShamashStack>()
            .ActivateOnEnter<AccelerationBomb>();
    }
}
