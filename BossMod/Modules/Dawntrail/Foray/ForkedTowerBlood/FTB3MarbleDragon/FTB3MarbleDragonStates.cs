namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB3MarbleDragon;

sealed class FTB3MarbleDragonStates : StateMachineBuilder
{
    private readonly FTB3MarbleDragon _module;

    public FTB3MarbleDragonStates(FTB3MarbleDragon module) : base(module)
    {
        _module = module;
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        ImitationStar(id, 13f);
        DraconiformMotion(id + 0x10000u, 13.5f);
        ImitationRain1(id + 0x20000u, 5f);
        DreadDeluge(id + 0x30000u, 13.1f);
        ImitationRain2(id + 0x40000u, 5.8f);
        DreadDeluge(id + 0x50000u, 8.6f);
        IceGolems(id + 0x60000u, 16.5f);
        ImitationRain3(id + 0x70000u, 0.6f);
        ImitationRain3(id + 0x80000u, 4.6f, false);
        LifelessLegacy(id + 0x90000u, 6.9f);
        SimpleState(id + 0xFF0000u, 10000, "???");
    }

    private void ImitationStar(uint id, float delay)
    {
        ComponentCondition<ImitationStar>(id, delay, comp => comp.NumCasts != 0, "Raidwide + bleed")
            .ActivateOnEnter<ImitationStar>()
            .ActivateOnEnter<ArenaChange>()
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<ImitationStar>();
    }

    private void DraconiformMotion(uint id, float delay)
    {
        ComponentCondition<DraconiformMotion>(id, delay, comp => comp.NumCasts != 0, "Front/back cone AOEs")
            .ActivateOnEnter<DraconiformMotion>()
            .DeactivateOnExit<ArenaChange>()
            .ActivateOnExit<ImitationRain>()
            .DeactivateOnExit<DraconiformMotion>();
    }

    private void ImitationRain1(uint id, float delay)
    {
        ComponentCondition<ImitationRain>(id, delay, comp => comp.NumCasts != 0, "Raidwide")
            .ExecOnEnter<ImitationRain>(comp => comp.Activation = _module.WorldState.FutureTime(4.9d))
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<ImitationRain>();
        ComponentCondition<ImitationIcicle>(id + 0x10u, 15.6f, comp => comp.NumCasts != 0, "Circle AOEs")
            .ActivateOnEnter<ImitationBlizzard>()
            .ActivateOnEnter<DraconiformMotion>()
            .ExecOnEnter<DraconiformMotion>(comp => comp.Color = Colors.Danger)
            .ActivateOnEnter<ImitationIcicle>()
            .DeactivateOnExit<ImitationIcicle>();
        ComponentCondition<DraconiformMotion>(id + 0x20u, 0.4f, comp => comp.NumCasts != 0, "Front/back cone AOEs")
            .DeactivateOnExit<DraconiformMotion>();
        for (var i = 1; i <= 4; ++i)
        {
            var offset = id + 0x30u + (uint)((i - 1) * 0x10u);
            var casts = i switch
            {
                1 => 2,
                2 => 5,
                3 => 7,
                _ => 8
            };
            var condition = ComponentCondition<ImitationBlizzard>(offset, i == 1 ? 4.3f : 1f, comp => comp.NumCasts == casts, $"Ice puddle AOEs {i}");
            if (i == 4)
            {
                condition.DeactivateOnExit<ImitationBlizzard>();
            }
        }
    }

    private void DreadDeluge(uint id, float delay)
    {
        ComponentCondition<DreadDeluge>(id, delay, comp => comp.NumCasts != 0, "Tankbusters")
            .ActivateOnEnter<DreadDeluge>()
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<DreadDeluge>();
    }

    private void ImitationRain2(uint id, float delay)
    {
        ComponentCondition<ImitationRain>(id, delay, comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<ImitationRain>()
            .ExecOnEnter<ImitationRain>(comp => comp.Activation = _module.WorldState.FutureTime(5.8d))
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<ImitationRain>();
        ComponentCondition<FrigidTwister>(id + 0x10u, 7.3f, comp => comp.ActiveAOEs(0, default!).Length != 0, "Icewinds appear")
            .ActivateOnEnter<FrigidTwister>()
            .ActivateOnEnter<ImitationBlizzard>();
        ComponentCondition<DraconiformMotion>(id + 0x20u, 7.4f, comp => comp.NumCasts != 0, "Front/back cone AOEs")
            .ActivateOnEnter<DraconiformMotion>()
            .ExecOnEnter<DraconiformMotion>(comp => comp.Color = Colors.Danger)
            .DeactivateOnExit<DraconiformMotion>();
        for (var i = 1; i <= 3; ++i)
        {
            var offset = id + 0x30u + (uint)((i - 1) * 0x10u);
            var casts = i switch
            {
                1 => 2,
                2 => 3,
                _ => 7
            };
            var condition = ComponentCondition<ImitationBlizzard>(offset, i == 1 ? 3.7f : 4f, comp => comp.NumCasts >= casts, $"Ice puddle AOEs {i}");
            if (i == 3)
            {
                condition.DeactivateOnExit<ImitationBlizzard>();
            }
        }
    }

    private void IceGolems(uint id, float delay)
    {
        ComponentCondition<IceGolems>(id, delay, comp => comp.ActiveActors.Count != 0, "Adds targetable (Ice Golems)")
            .ActivateOnEnter<VulnerabilityDown>()
            .ActivateOnEnter<WitheringEternity>()
            .ActivateOnEnter<IceGolems>();
    }

    private void ImitationRain3(uint id, float delay, bool first = true)
    {
        if (!first)
        {
            ComponentCondition<ImitationRain>(id, delay, comp => comp.NumCasts != 0, "Raidwide")
                .ActivateOnEnter<ImitationRain>()
                .ExecOnEnter<ImitationRain>(comp => comp.Activation = _module.WorldState.FutureTime(delay))
                .SetHint(StateMachine.StateHint.Raidwide)
                .DeactivateOnExit<ImitationRain>();
        }
        else
        {
            ComponentCondition<WitheringEternity>(id, delay, comp => comp.NumCasts != 0, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide)
                .DeactivateOnExit<WitheringEternity>();
        }
        ComponentCondition<FrigidDive>(id + 0x10u, first ? 12.2f : 11.4f, comp => comp.NumCasts != 0, "Rect AOE")
            .ActivateOnEnter<FrigidDive>()
            .ActivateOnEnter<ImitationBlizzard>()
            .ActivateOnEnter<ImitationBlizzardTowers>()
            .DeactivateOnExit<FrigidDive>();
        ComponentCondition<ImitationBlizzardTowers>(id + 0x20u, 4.1f, comp => comp.NumCasts != 0, "Towers 1 + cross AOE");
        ComponentCondition<ImitationBlizzardTowers>(id + 0x30u, 4f, comp => comp.NumCasts == 6, "Towers 2")
            .DeactivateOnExit<ImitationBlizzard>()
            .DeactivateOnExit<ImitationBlizzardTowers>();
    }

    private void LifelessLegacy(uint id, float delay)
    {
        ComponentCondition<IceSprite>(id, delay, comp => comp.ActiveActors.Count != 0, "Adds targetable (Ice Sprites)")
            .ActivateOnEnter<LifelessLegacy>()
            .ActivateOnEnter<IceSprite>();
        ComponentCondition<LifelessLegacy>(id + 0x10u, 36.8f, comp => comp.NumCasts != 0, "Raidwide or enrage")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<LifelessLegacy>()
            .DeactivateOnExit<IceSprite>()
            .DeactivateOnExit<IceGolems>()
            .DeactivateOnExit<VulnerabilityDown>();
    }
}
