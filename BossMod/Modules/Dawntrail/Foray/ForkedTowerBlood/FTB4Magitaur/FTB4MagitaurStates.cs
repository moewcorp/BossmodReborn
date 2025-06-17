namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB4Magitaur;

sealed class FTB4MagitaurStates : StateMachineBuilder
{
    public FTB4MagitaurStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<Unseal>();
    }

    private void SinglePhase(uint id)
    {
        UnsealedAura(id, 15.2f);
        Unseal(id + 0x10000u, 23.3f);
        SimpleState(id + 0xFF0000u, 10000, "???");
    }

    private void UnsealedAura(uint id, float delay)
    {
        ComponentCondition<UnsealedAura>(id, delay, comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<UnsealedAura>()
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<UnsealedAura>();
    }

    private void Unseal(uint id, float delay)
    {
        for (var i = 1; i <= 3; ++i)
        {
            var offset = id + (uint)((i - 1) * 0x10u);
            var casts = i * 6;
            var condition = ComponentCondition<Unseal>(offset, i == 1 ? delay : 3.2f, comp => comp.NumCasts == casts, $"Baited tankbusters {i}")
                .SetHint(StateMachine.StateHint.Tankbuster);
            if (i == 3)
            {
                condition.DeactivateOnExit<Unseal>();
            }
        }
    }
}
