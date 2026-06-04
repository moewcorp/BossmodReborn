namespace BossMod.Dawntrail.Ultimate.DMU;

[SkipLocalsInit]
sealed class KefkaStates : StateMachineBuilder {
    public KefkaStates(BossModule module) : base(module) {
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id) {
        phase1(id, 10.1f);
        SimpleState(id + 0xFF0000u, 10000f, "???");
    }

    private void phase1(uint id, float delay) {
        Cast(id, (uint)AID.RevoltingRuinIII, delay, 5, "1st Tankbuster")
            .ActivateOnEnter<RevoltingRuinIII>();
        ComponentCondition<RevoltingRuinIII>(id + 0x05, 3.25f, o => o.NumCasts > 1, "2nd Tankbuster")
            .DeactivateOnExit<RevoltingRuinIII>();
        Cast(id + 0x10, (uint)AID.GravenImage, 7.7f, 3, "Graven Image")
            .ActivateOnEnter<GravenImage>()
            .ActivateOnEnter<BlizzardSafeSpots>()
            .ActivateOnEnter<StackSpreadOrbs>();
        ComponentCondition<GravenImage>(id + 0x20, 5.8f, o => o.NumCasts > 0, "Knockbacks")
            .DeactivateOnExit<GravenImage>();
        ComponentCondition<BlizzardSafeSpots>(id + 0x30, 2.3f, o => o.NumCasts > 0, "Blizzard safe spots")
            .DeactivateOnExit<BlizzardSafeSpots>();
        ComponentCondition<StackSpreadOrbs>(id + 0x40, 0.8f, o => !o.Active, "Stack / Spread")
            .DeactivateOnExit<StackSpreadOrbs>()
            .ActivateOnExit<WaveCannon>()
            .ActivateOnExit<DoubleTroubleTrapKnockback>()
            .ActivateOnExit<DoubleTroubleTrapStacks>();
        ComponentCondition<WaveCannon>(id + 0x50, 4.2f, o => o.NumCasts > 0, "Wave Cannon Spreads")
            .DeactivateOnExit<WaveCannon>()
            .ActivateOnEnter<WaveCannonTowers>();
        ComponentCondition<WaveCannonTowers>(id + 0x60, 3.6f, o => o.NumCasts > 0, "Towers resolve")
            .DeactivateOnExit<WaveCannonTowers>();
        ComponentCondition<DoubleTroubleTrapKnockback>(id + 0x70, 3.8f, o => o.NumCasts > 0, "Stacks + Knockbacks")
            .DeactivateOnExit<DoubleTroubleTrapStacks>()
            .DeactivateOnExit<DoubleTroubleTrapKnockback>()
            .ActivateOnEnter<LightningSafeSpots>()
            .ActivateOnEnter<BlizzardSafeSpots>();
        ComponentCondition<BlizzardSafeSpots>(id + 0x80, 3.8f, o => o.NumCasts > 0, "Blizzard + Lightning safe spots")
            .DeactivateOnExit<LightningSafeSpots>()
            .DeactivateOnExit<BlizzardSafeSpots>();

        Cast(id + 0x90, (uint)AID.LightOfJudgment, 4.0f, 5.0f, "Raidwide")
            .ActivateOnEnter<LightOfJudgment>()
            .DeactivateOnExit<LightOfJudgment>()
            .ActivateOnEnter<HyperDrive>();
        ComponentCondition<HyperDrive>(id + 0x100, 3.2f, o => o.NumCasts > 0, "1st Tankbuster");
        ComponentCondition<HyperDrive>(id + 0x110, 2.3f, o => o.NumCasts > 1, "2nd Tankbuster");
        ComponentCondition<HyperDrive>(id + 0x120, 2.0f, o => o.NumCasts > 2, "3rd Tankbuster")
            .DeactivateOnExit<HyperDrive>();

        Cast(id + 0x130, (uint)AID.GravenImage, 7.3f, 3, "Graven Image")
            .ActivateOnEnter<BlizzardSafeSpots>()
            .ActivateOnEnter<Gravitas>()
            .ActivateOnEnter<GravitasPuddles>();
        ComponentCondition<BlizzardSafeSpots>(id + 0x140, 7.15f, o => o.NumCasts > 0, "Blizzard safe spots + Stack")
            .DeactivateOnExit<BlizzardSafeSpots>();
        ComponentCondition<Gravitas>(id + 0x150, 4.1f, o => !o.Active, "Spreads")
            .ActivateOnEnter<RevoltingRuinIII>()
            .ActivateOnEnter<GravitationalWave>();
        Cast(id + 0x160, (uint)AID.RevoltingRuinIII, 0.7f, 5, "1st Tankbuster");
        ComponentCondition<RevoltingRuinIII>(id + 0x165, 3.25f, o => o.NumCasts > 1, "2nd Tankbuster")
            .DeactivateOnExit<RevoltingRuinIII>();
        ComponentCondition<GravitationalWave>(id + 0x170, 0.85f, o => o.NumCasts > 0, "Left/Right Cleave")
            .DeactivateOnExit<GravitationalWave>();
        ComponentCondition<Gravitas>(id + 0x180, 4.6f, o => o.totalStacksGoneOff > 4, "Stack");
        ComponentCondition<Gravitas>(id + 0x190, 4.0f, o => !o.Active, "Spreads")
            .DeactivateOnExit<Gravitas>()
            .ActivateOnEnter<GravitationalWave>();
        ComponentCondition<GravitationalWave>(id + 0x200, 4.5f, o => o.NumCasts > 0, "Left/Right Cleave")
            .DeactivateOnExit<GravitationalWave>()
            .ActivateOnEnter<DoubleTroubleTrapKnockback>()
            .ActivateOnEnter<DoubleTroubleTrapStacks>();
        ComponentCondition<DoubleTroubleTrapKnockback>(id + 0x210, 4.0f, o => o.NumCasts > 0, "Stacks + Knockbacks")
            .DeactivateOnExit<DoubleTroubleTrapStacks>()
            .DeactivateOnExit<DoubleTroubleTrapKnockback>();

        // TODO currently just left it on since  it cleans itself - not sure if it will cause problems if the orbs are never popped
        // .ActivateOnEnter<GravitasPuddles>(); - Needs to be turned off!

        Cast(id + 0x220, (uint)AID.LightOfJudgment, 4.0f, 5.0f, "Raidwide")
            .ActivateOnEnter<LightOfJudgment>()
            .DeactivateOnExit<LightOfJudgment>()
            .ActivateOnEnter<HyperDrive>();
        ComponentCondition<HyperDrive>(id + 0x100, 3.2f, o => o.NumCasts > 0, "1st Tankbuster");
        ComponentCondition<HyperDrive>(id + 0x110, 2.3f, o => o.NumCasts > 1, "2nd Tankbuster");
        ComponentCondition<HyperDrive>(id + 0x120, 2.0f, o => o.NumCasts > 2, "3rd Tankbuster")
            .DeactivateOnExit<HyperDrive>();


        // TODO everything beyond this point is just to make further mechanics so what resolved, but have not been looked at yet fully.
        /*.
        .ActivateOnEnter<LightningSafeSpots>()
        .ActivateOnEnter<StackSpreadOrbs>();*/
    }
}
