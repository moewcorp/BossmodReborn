namespace BossMod.Endwalker.VariantCriterion.V01SS.V013Gladiator;

class V013GladiatorStates : StateMachineBuilder
{
    public V013GladiatorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SunderedRemains>()
            .ActivateOnEnter<BitingWindBad>()
            .ActivateOnEnter<RingOfMight1>()
            .ActivateOnEnter<RingOfMight2>()
            .ActivateOnEnter<RingOfMight3>()
            .ActivateOnEnter<RushOfMight>()
            .ActivateOnEnter<ShatteringSteelMeteor>()
            .ActivateOnEnter<RackAndRuin>()
            .ActivateOnEnter<FlashOfSteel1>()
            .ActivateOnEnter<FlashOfSteel2>()
            .ActivateOnEnter<SculptorsPassion>()
            .ActivateOnEnter<GoldenFlame>()
            .ActivateOnEnter<SilverFlame>()
            .ActivateOnEnter<Landing>()
            .ActivateOnEnter<MightySmite>();
    }
}
