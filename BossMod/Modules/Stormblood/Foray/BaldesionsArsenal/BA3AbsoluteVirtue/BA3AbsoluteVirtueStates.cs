namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA3AbsoluteVirtue;

class BA3AbsoluteVirtueStates : StateMachineBuilder
{
    public BA3AbsoluteVirtueStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<Meteor>()
            .ActivateOnEnter<MedusaJavelin>()
            .ActivateOnEnter<AuroralWind>()
            .ActivateOnEnter<ExplosiveImpulse1>()
            .ActivateOnEnter<ExplosiveImpulse2>()
            .ActivateOnEnter<AernsWynavExplosion>()
            .ActivateOnEnter<BrightDarkAurora>()
            .ActivateOnEnter<AstralUmbralRays>()
            .ActivateOnEnter<BrightDarkAuroraExplosion>()
            .ActivateOnEnter<DarkAuroraTether>()
            .ActivateOnEnter<BrightAuroraTether>();
    }
    // some timings have noticeable variations of over 1s
    private void SinglePhase(uint id)
    {
        Meteor(id, 11.1f);
        Eidos(id + 0x10000, 4.2f);
        HostileAspect1(id + 0x20000, 4.1f);
        MedusaJavelin(id + 0x30000, 2.7f);
        Eidos(id + 0x40000, 5.2f);
        ImpactStreamBoss(id + 0x50000, 4);
        AuroralWind(id + 0x60000, 6.4f);
        Eidos(id + 0x70000, 12);
        HostileAspect1(id + 0x80000, 3.8f);
        Meteor(id + 0x90000, 14);
        DarkBrightAuroraTowers(id + 0xA0000, 10);
        MedusaJavelin(id + 0xB0000, 5.8f);
        AuroralWind(id + 0xC0000, 3.5f);
        Meteor(id + 0xD0000, 4.1f);
        Meteor(id + 0xE0000, 5.1f);
        RelativeVirtues(id + 0xF0000, 10);
        MedusaJavelin(id + 0x100000, 3.2f);
        AuroralWind(id + 0x110000, 7);
        Meteor(id + 0x120000, 4.2f);
        CallWyvern(id + 0x130000, 6.5f);
        DarkBrightAuroraTowers(id + 0x140000, 6.4f);
        MedusaJavelin(id + 0x150000, 5);
        AuroralWind(id + 0x160000, 3.1f);
        Meteor(id + 0x170000, 13);
        Eidos(id + 0x180000, 4.2f);
        HostileAspect2(id + 0x190000, 5);
        MedusaJavelin(id + 0x1A0000, 8.2f);
        Meteor(id + 0x1B0000, 3.2f);
        CallWyvern(id + 0x1C0000, 12.3f);
        DarkBrightAuroraTowers(id + 0x1D0000, 6.2f);
        MedusaJavelin(id + 0x1E0000, 5);
        AuroralWind(id + 0x1F0000, 3.2f);
        Meteor(id + 0x200000, 13.2f);
        Eidos(id + 0x210000, 4.2f);
        HostileAspect2(id + 0x220000, 3.5f);
        MedusaJavelin(id + 0x230000, 8);
        Meteor(id + 0x240000, 3);
        MeteorEnrage(id + 0x250000);
    }

    private void Meteor(uint id, float delay)
    {
        Cast(id, AID.Meteor, delay, 4, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void MeteorEnrage(uint id)
    {
        Cast(id, AID.MeteorEnrage, 3.1f, 10, "Enrage")
            .ActivateOnEnter<MeteorEnrageCounter>();
        ComponentCondition<MeteorEnrageCounter>(id + 0x10, 3, comp => comp.NumCasts == 1, "Enrage repeat 1");
        ComponentCondition<MeteorEnrageCounter>(id + 0x20, 5, comp => comp.NumCasts == 2, "Enrage repeat 2");
    }

    private void Eidos(uint id, float delay)
    {
        CastMulti(id, [AID.EidosAstral, AID.EidosUmbral], delay, 2, "Change element");
    }

    private void HostileAspect1(uint id, float delay)
    {
        Cast(id, AID.HostileAspect, delay, 8, "Circle AOEs");
    }

    private void HostileAspect2(uint id, float delay)
    {
        CastStart(id, AID.HostileAspect, delay, "Circle AOEs appear");
        ComponentCondition<ExplosiveImpulse1>(id + 0x10, 7.4f, comp => comp.Casters.Count != 0, "Proximity AOE appears")
            .ActivateOnEnter<BrightDarkAuroraCounter>();
        CastEnd(id + 0x20, 2, "Circle AOEs resolve");
        ComponentCondition<ExplosiveImpulse1>(id + 0x30, 4.4f, comp => comp.Casters.Count == 0, "Proximity AOE resolves");
        CastMulti(id + 0x40, [AID.EidosAstral, AID.EidosUmbral], 6.3f, 2, "Change element");
        ComponentCondition<BrightDarkAuroraCounter>(id + 0x50, 1.9f, comp => comp.NumCasts == 2, "Half room cleaves 1");
        ComponentCondition<BrightDarkAuroraCounter>(id + 0x60, 4.8f, comp => comp.NumCasts == 4, "Half room cleaves 2")
            .DeactivateOnExit<BrightDarkAuroraCounter>();
    }

    private void MedusaJavelin(uint id, float delay)
    {
        Cast(id, AID.MedusaJavelin, delay, 3, "Cone AOE");
    }

    private void AuroralWind(uint id, float delay)
    {
        Cast(id, AID.AuroralWind, delay, 5, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void ImpactStreamBoss(uint id, float delay)
    {
        Cast(id, AID.ImpactStream1, delay, 3, "Half room cleaves");
    }

    private void DarkBrightAuroraTowers(uint id, float delay)
    {
        ComponentCondition<DarkAuroraTether>(id, delay, comp => comp.Towers.Count != 0, "Towers + tethers appear");
    }

    private void RelativeVirtues(uint id, float delay)
    {
        ComponentCondition<ExplosiveImpulse1>(id, delay, comp => comp.Casters.Count != 0, "Proximity AOEs appear")
            .ActivateOnEnter<BrightDarkAuroraCounter>();
        ComponentCondition<ExplosiveImpulse1>(id + 0x10, 5, comp => comp.Casters.Count == 0, "Proximity AOEs resolve");
        ComponentCondition<BrightDarkAuroraCounter>(id + 0x20, 8, comp => comp.NumCasts == 2, "Half room cleaves 1");
        ComponentCondition<BrightDarkAuroraCounter>(id + 0x30, 5.6f, comp => comp.NumCasts == 4, "Half room cleaves 2");
        ComponentCondition<BrightDarkAuroraCounter>(id + 0x40, 4.4f, comp => comp.NumCasts == 6, "Half room cleaves 3")
            .DeactivateOnExit<BrightDarkAuroraCounter>();
        CastStart(id + 0x50, AID.ExplosiveImpulse2, 1.3f, "Proximity AOE appears");
        CastEnd(id + 0x60, 5, "Proximity AOE resolves");
    }

    private void CallWyvern(uint id, float delay)
    {
        Cast(id, AID.CallWyvern, delay, 3, "Adds spawn");
    }
}
