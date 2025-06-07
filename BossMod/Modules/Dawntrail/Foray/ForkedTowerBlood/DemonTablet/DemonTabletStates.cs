namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.DemonTablet;

sealed class DemonTabletStates : StateMachineBuilder
{
    public DemonTabletStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<DemonicDarkII>()
            .ActivateOnEnter<RayOfIgnorance>()
            .ActivateOnEnter<LandingCircle>()
            .ActivateOnEnter<LandingSmall>()
            .ActivateOnEnter<LandingMedium>()
            .ActivateOnEnter<LandingKnockback>()
            .ActivateOnEnter<OccultChisel>()
            .ActivateOnEnter<RotationBig>()
            .ActivateOnEnter<RotationSmall>()
            .ActivateOnEnter<LacunateStream>()
            .ActivateOnEnter<PortentousCometeor>()
            .ActivateOnEnter<PortentousCometeorBait>()
            .ActivateOnEnter<PortentousComet1>()
            .ActivateOnEnter<PortentousComet2>()
            .ActivateOnEnter<PortentousCometKnockback>()
            .ActivateOnEnter<Summon>()
            .ActivateOnEnter<DemonographTowers>()
            .ActivateOnEnter<GravityTowers>()
            ;
    }

    private void SinglePhase(uint id)
    {
        SimpleState(id + 0xFF0000u, 10000, "???");
    }

    //private void XXX(uint id, float delay)
}
