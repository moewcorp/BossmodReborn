namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA1Owain;

class BA1OwainStates : StateMachineBuilder
{
    public BA1OwainStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<Thricecull>()
            .ActivateOnEnter<AcallamNaSenorach>()
            .ActivateOnEnter<ElementalMagicks>()
            .ActivateOnEnter<PiercingLight1>()
            .ActivateOnEnter<PiercingLight2>()
            .ActivateOnEnter<Pitfall>()
            .ActivateOnEnter<Spiritcull>()
            .ActivateOnEnter<IvoryPalm>()
            .ActivateOnEnter<IvoryPalmExplosion>()
            .ActivateOnEnter<EurekanAero>();
    }
    // all timings seem to have upto 1s variation
    private void SinglePhase(uint id)
    {
        Thricecull(id, 10);
        AcallamNaSenorach(id + 0x10000, 7.1f);
        Mythcall(id + 0x20000, 7.9f);
        Thricecull(id + 0x30000, 10);
        AcallamNaSenorach(id + 0x40000, 7);
        ElementalShift(id + 0x50000, 7.5f);
        Thricecull(id + 0x60000, 11);
        Spiritcull(id + 0x70000, 8.3f);
        // from now on repeats until wipe or victory, this extends timeline until up around 20min since its theoretically possible to solo it as long as Owain is pulled
        for (var i = 0; i < 12; ++i)
        {
            var pid = (uint)(i * 0x10000);
            Thricecull(id += 0x80000 + pid, 5.1f);
            AcallamNaSenorach(id += 0x90000 + pid, 6);
            PiercingLight2(id += 0xA0000 + pid, 6.1f);
            AcallamNaSenorach(id += 0xB0000 + pid, 9.7f);
            AcallamNaSenorach(id += 0xC0000 + pid, 5.1f);
            ElementalShiftSpiritcull(id + 0xD0000 + pid, 8.3f);
            Thricecull(id += 0xE0000 + pid, 9.3f);
            AcallamNaSenorach(id += 0xF0000 + pid, 17.2f);
            IvoryPalmElementalMagicks(id += 0x100000 + pid, 1.2f);
            Spiritcull(id += 0x110000 + pid, 5.1f);
        }
        SimpleState(id + 0xFF0000, 10, "???");
    }

    private void Thricecull(uint id, float delay)
    {
        Cast(id, AID.Thricecull, delay, 5, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void AcallamNaSenorach(uint id, float delay)
    {
        Cast(id, AID.AcallamNaSenorach, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Mythcall(uint id, float delay)
    {
        Cast(id, AID.Mythcall, delay, 2, "Spawn spears");
        Cast(id + 0x10, AID.ElementalShift1, 2.2f, 2, "Switch elements");
        CastMulti(id + 0x20, [AID.ElementalMagicksIceBoss, AID.ElementalMagicksFireBoss], 6.3f, 5, "Circle AOEs");
    }

    private void ElementalShift(uint id, float delay)
    {
        Cast(id, AID.ElementalShift1, delay, 2, "Switch elements");
        CastMulti(id + 0x10, [AID.ElementalMagicksIceBoss, AID.ElementalMagicksFireBoss], 6.4f, 5, "Circle AOEs");
    }

    private void Spiritcull(uint id, float delay)
    {
        Cast(id, AID.Spiritcull, delay, 3, "Dorito stacks appear")
            .ActivateOnEnter<LegendaryImbas>();
        ComponentCondition<LegendaryImbas>(id + 0x10, 0.1f, comp => comp.Casters.Count != 0, "Spreads appear")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<LegendaryImbas>();
        ComponentCondition<PiercingLight1>(id + 0x20, 5, comp => comp.Spreads.Count == 0, "Spreads and dorito stacks resolve");
    }

    private void PiercingLight2(uint id, float delay)
    {
        ComponentCondition<PiercingLight2>(id, delay, comp => comp.Spreads.Count != 0, "Spreads appear");
        CastStart(id + 0x10, AID.Pitfall, 1, "Proximity AOE");
        ComponentCondition<PiercingLight2>(id + 0x20, 3.8f, comp => comp.Spreads.Count == 0, "Spreads resolve");
        CastEnd(id + 0x30, 1, "Proximity AOE resolves");
    }

    private void ElementalShiftSpiritcull(uint id, float delay)
    {
        Cast(id, AID.ElementalShift1, delay, 2, "Switch elements");
        Cast(id + 0x10, AID.Spiritcull, 6.3f, 3, "Dorito stacks appear")
            .ActivateOnEnter<LegendaryImbas>();
        ComponentCondition<LegendaryImbas>(id + 0x20, 1.1f, comp => comp.Casters.Count != 0, "Spreads appear")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<LegendaryImbas>();
        CastStartMulti(id + 0x30, [AID.ElementalMagicksIceBoss, AID.ElementalMagicksFireBoss], 2, "Circle AOEs start");
        ComponentCondition<PiercingLight1>(id + 0x40, 3, comp => comp.Spreads.Count == 0, "Spreads resolve");
        ComponentCondition<ElementalMagicks>(id + 0x50, 2, comp => comp.AOEs.Count == 0, "Circles resolve");
    }

    private void IvoryPalmElementalMagicks(uint id, float delay)
    {
        ComponentCondition<IvoryPalm>(id, delay, comp => comp.Tethers.Count != 0, "Hands spawn");
        Cast(id + 0x10, AID.ElementalShift1, 6.4f, 2, "Switch elements");
        CastMulti(id + 0x20, [AID.ElementalMagicksIceBoss, AID.ElementalMagicksFireBoss], 6.3f, 5, "Circle AOEs");
        Cast(id + 0x30, AID.Thricecull, 6, 5, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<IvoryPalmExplosion>(id + 0x40, 5, comp => comp.Casters.Count == 0, "Hands soft enrage"); // quite some timing variation here since hands could be killed at any time
    }
}
