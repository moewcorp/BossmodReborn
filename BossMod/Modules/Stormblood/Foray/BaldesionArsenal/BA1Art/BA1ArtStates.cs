namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA1Art;

class BA1ArtStates : StateMachineBuilder
{
    public BA1ArtStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<Thricecull>()
            .ActivateOnEnter<AcallamNaSenorach>()
            .ActivateOnEnter<LegendMythSpinnerCarver>()
            .ActivateOnEnter<DefilersDeserts>()
            .ActivateOnEnter<DefilersDesertsPredict>()
            .ActivateOnEnter<Pitfall>()
            .ActivateOnEnter<LegendaryGeasAOE>()
            .ActivateOnEnter<LegendaryGeasStay>()
            .ActivateOnEnter<GloryUnearthed>()
            .ActivateOnEnter<PiercingDark>();
    }
    // all timings seem to have upto 1s variation
    private void SinglePhase(uint id)
    {
        Thricecull(id, 10);
        LegendCarverSpinner(id + 0x10000);
        AcallamNaSenorach(id + 0x20000, 4.1f);
        Mythcall(id + 0x30000, 7);
        AcallamNaSenorach(id + 0x40000, 3.2f);
        Thricecull(id + 0x50000, 3.6f);
        Mythcall(id + 0x60000, 5);
        // from now on repeats until wipe or victory, this extends timeline until up around 20min since its theoretically possible to solo it as long as Owain is pulled
        for (var i = 0; i < 10; ++i)
        {
            var pid = (uint)(i * 0x10000);
            LegendaryGeas(id += 0x70000 + pid, 3.1f);
            AcallamNaSenorach(id += 0x80000 + pid, 4.5f);
            GloryUnearthedPitfall(id += 0x90000 + pid, 4.5f);
            Thricecull(id += 0xA0000 + pid, 5);
            AcallamNaSenorach(id += 0xB0000 + pid, 3.2f);
            Mythcall(id += 0xC0000 + pid, 7.8f);
            Thricecull(id += 0xD0000 + pid, 3);
            AcallamNaSenorach(id += 0xE0000 + pid, 3.2f);
            Mythcall2(id += 0xF0000 + pid, 6);
        }
        SimpleState(id + 0xFF0000, 10, "???");
    }

    private void Thricecull(uint id, float delay)
    {
        Cast(id, AID.Thricecull, delay, 4, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void LegendCarverSpinner(uint id)
    {
        CastMulti(id, [AID.Legendcarver, AID.Legendspinner], 3.5f, 4.5f, "In/Out AOE");
        CastMulti(id + 0x10, [AID.Legendcarver, AID.Legendspinner], 4, 4.5f, "Inverse of previous AOE");
    }

    private void AcallamNaSenorach(uint id, float delay)
    {
        Cast(id, AID.AcallamNaSenorach, delay, 4, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Mythcall(uint id, float delay)
    {
        Cast(id, AID.Mythcall, delay, 2, "Spawn spears");
        CastMulti(id + 0x10, [AID.Legendcarver, AID.Legendspinner], 6, 4.5f, "In/Out AOE");
        ComponentCondition<LegendMythSpinnerCarver>(id + 0x20, 3, comp => comp.AOEs.Count == 0, "Spears repeat AOE");
    }

    private void Mythcall2(uint id, float delay)
    {
        Cast(id, AID.Mythcall, delay, 2, "Spawn spears");
        Cast(id + 0x10, AID.PiercingDarkVisual, 6.1f, 2.5f, "Spreads");
        CastMulti(id + 0x20, [AID.Legendcarver, AID.Legendspinner], 1.6f, 4.5f, "In/Out AOE");
        ComponentCondition<PiercingDark>(id + 0x30, 0.4f, comp => comp.ActiveSpreads.Count == 0, "Spreads resolve");
        ComponentCondition<LegendMythSpinnerCarver>(id + 0x40, 3, comp => comp.AOEs.Count == 0, "Spears repeat AOE");
    }

    private void LegendaryGeas(uint id, float delay)
    {
        Cast(id, AID.LegendaryGeas, delay, 4, "Circle AOE + stop moving");
        ComponentCondition<LegendaryGeasStay>(id + 0x10, 3, comp => comp.PlayerStates[0] == default, "Can move again + cross");
        ComponentCondition<DefilersDeserts>(id + 0x20, 0.5f, comp => comp.Casters.Count != 0, "Crosses");
    }

    private void GloryUnearthedPitfall(uint id, float delay)
    {
        ComponentCondition<GloryUnearthed>(id, delay, comp => comp.Chasers.Count != 0, "Chasing AOE start");
        CastStart(id + 0x10, AID.Pitfall, 2.5f, "Proximity AOE start");
        CastEnd(id + 0x20, 5, "Proximity AOE resolve");
        ComponentCondition<GloryUnearthed>(id + 0x30, 3.7f, comp => comp.Chasers.Count == 0, "Chasing AOE ends");
    }
}
