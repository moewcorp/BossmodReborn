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

        // TODO Everything beyond this point is not fully completed, so may have issues
            .ActivateOnExit<WaveCannon>();
        ComponentCondition<WaveCannon>(id + 0x50, 4.0f, o => o.NumCasts > 0, "Wave Cannon Spreads")
            .DeactivateOnExit<WaveCannon>()
            .ActivateOnEnter<WaveCannonTowers>()
            .ActivateOnEnter<BlizzardSafeSpots>()
            .ActivateOnEnter<LightningSafeSpots>();
    }
}
