namespace BossMod.Dawntrail.Quantum.FinalVerse.Q40EminentGrief;

[SkipLocalsInit]
sealed class Q40EminentGriefStates : StateMachineBuilder
{
    public Q40EminentGriefStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<TerrorEyeVoidTrapBallOfFire>()
            ;
    }

    private void SinglePhase(uint id)
    {
        BoundsOfSinScourgingBlaze(id, 12.1f);
        SimpleState(id + 0xFF0000u, 10000f, "???");
    }

    private void BoundsOfSinScourgingBlaze(uint id, float delay)
    {
        ComponentCondition<ScourgingBlaze>(id, delay, static comp => comp.Direction != 0, "Exaflare direction 1")
            .ActivateOnEnter<ScourgingBlaze>();
        ComponentCondition<ScourgingBlaze>(id + 0x10u, 8f, static comp => comp.Direction == 2, "Exaflare direction 2");
        ComponentCondition<BoundsOfSinPull>(id + 0x20u, 13.1f, static comp => comp.NumCasts != 0, "Pull into mid")
            .ActivateOnEnter<BoundsOfSinPull>()
            .ActivateOnEnter<BoundsOfSinTowers>()
            .DeactivateOnExit<BoundsOfSinPull>()
            .ActivateOnEnter<BoundsOfSinEnd>();
        ComponentCondition<BoundsOfSinSmallAOE>(id + 0x30u, 4f, static comp => comp.NumCasts != 0, "Small AOEs start")
            .ActivateOnEnter<LightDarkNeutralize>()
            .ActivateOnEnter<BoundsOfSinSmallAOE>();
        ComponentCondition<BoundsOfSinSmallAOE>(id + 0x40u, 1.6f, static comp => comp.NumCasts == 12, "Small AOEs end")
            .DeactivateOnExit<BoundsOfSinSmallAOE>();
        ComponentCondition<BoundsOfSinEnd>(id + 0x50u, 1.4f, static comp => comp.NumCasts != 0 && comp.Pillars.Count == 0, "In/Out AOE + arena restored")
            .DeactivateOnExit<BoundsOfSinEnd>();
        ComponentCondition<LightDarkNeutralize>(id + 0x60u, 1.2f, static comp => comp.NumCasts != 0, "Stacks resolve")
            .DeactivateOnExit<LightDarkNeutralize>();
        ComponentCondition<BoundsOfSinTowers>(id + 0x70u, 5.1f, static comp => comp.NumCasts != 0, "Towers resolve + raidwide") // some variation with timings here, either 4.1s or 5.1s
            .SetHint(StateMachine.StateHint.Raidwide)
            .ExecOnExit<ScourgingBlaze>(static comp => comp.ShowAOEs())
            .DeactivateOnExit<BoundsOfSinTowers>();
        ComponentCondition<ScourgingBlaze>(id + 0x80u, 6.9f, static comp => comp.NumCasts != 0, "Exaflares start");
        ComponentCondition<ScourgingBlaze>(id + 0x90u, 7.8f, static comp => comp.Lines.Count == 0, "Exaflares end")
            .DeactivateOnExit<ScourgingBlaze>();
    }
}
