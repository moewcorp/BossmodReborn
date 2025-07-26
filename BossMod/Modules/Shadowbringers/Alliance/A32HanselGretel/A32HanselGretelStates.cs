﻿namespace BossMod.Shadowbringers.Alliance.A32HanselGretel;

sealed class A32HanselGretelStates : StateMachineBuilder
{
    public A32HanselGretelStates(A32HanselGretel module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StrongerTogether>()
            .ActivateOnEnter<UpgradedShield>()
            .ActivateOnEnter<WailLamentation>()
            .ActivateOnEnter<CripplingBlow1>()
            .ActivateOnEnter<CripplingBlow2>()
            .ActivateOnEnter<BloodySweep>()
            .ActivateOnEnter<RiotOfMagicSeedOfMagicAlpha>()
            .ActivateOnEnter<PassingLance>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<UnevenFooting>()
            .ActivateOnEnter<HungryLance>()
            .ActivateOnEnter<Breakthrough>()
            .ActivateOnEnter<SeedOfMagicBeta>()
            .ActivateOnEnter<MagicalConfluence>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && (module.BossHansel?.IsDeadOrDestroyed ?? true);
    }
}
