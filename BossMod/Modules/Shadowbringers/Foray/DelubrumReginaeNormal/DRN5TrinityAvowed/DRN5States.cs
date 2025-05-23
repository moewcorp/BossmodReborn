namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN5TrinityAvowed;

class DRN5TrinityAvowedStates : StateMachineBuilder
{
    public DRN5TrinityAvowedStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<PlayerTemperatures>()
            .ActivateOnEnter<GloryOfBozja>()
            .ActivateOnEnter<WrathOfBozja>()
            .ActivateOnEnter<ElementalImpact>()
            .ActivateOnEnter<ElementalImpactTemperature>()
            .ActivateOnEnter<ShimmeringShot>()
            .ActivateOnEnter<AllegiantArsenal>()
            .ActivateOnEnter<BladeOfEntropy>()
            .ActivateOnEnter<FlamesOfBozja>()
            .ActivateOnEnter<GleamingArrow>();
    }
}