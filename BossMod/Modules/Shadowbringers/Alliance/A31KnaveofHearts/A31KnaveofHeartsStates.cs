namespace BossMod.Shadowbringers.Alliance.A31KnaveofHearts;

class A31KnaveofHeartsStates : StateMachineBuilder
{
    public A31KnaveofHeartsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Roar>()
            .ActivateOnEnter<ColossalImpactLeft>()
            .ActivateOnEnter<ColossalImpactRight>()
            .ActivateOnEnter<ColossalImpactCenter>()
            .ActivateOnEnter<ColossalImpact1>()
            .ActivateOnEnter<ColossalImpact2>()
            .ActivateOnEnter<ColossalImpact3>()
            .ActivateOnEnter<MagicArtilleryBeta>()
            .ActivateOnEnter<MagicArtilleryAlpha>()
            .ActivateOnEnter<Energy>()
            .ActivateOnEnter<LightLeap>()
            .ActivateOnEnter<BoxSpawn>()
            .ActivateOnEnter<MagicBarrage>()
            .ActivateOnEnter<Lunge>();
    }
}
