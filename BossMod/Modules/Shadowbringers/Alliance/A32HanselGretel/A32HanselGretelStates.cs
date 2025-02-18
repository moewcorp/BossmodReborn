namespace BossMod.Shadowbringers.Alliance.A32HanselGretel;

class A32HanselGretelStates : StateMachineBuilder
{
    public A32HanselGretelStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Wail1>()
            .ActivateOnEnter<Wail2>()
            .ActivateOnEnter<CripplingBlow1>()
            .ActivateOnEnter<CripplingBlow2>()
            .ActivateOnEnter<BloodySweep1>()
            .ActivateOnEnter<BloodySweep2>()
            .ActivateOnEnter<BloodySweep3>()
            .ActivateOnEnter<BloodySweep4>()
            .ActivateOnEnter<SeedOfMagicAlpha>()
            .ActivateOnEnter<RiotOfMagic>()
            .ActivateOnEnter<PassingLance>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<UnevenFooting>()
            .ActivateOnEnter<HungryLance1>()
            .ActivateOnEnter<HungryLance2>()
            .ActivateOnEnter<Breakthrough>()
            .ActivateOnEnter<SeedOfMagicBeta>()
            .ActivateOnEnter<Lamentation>();
    }
}
