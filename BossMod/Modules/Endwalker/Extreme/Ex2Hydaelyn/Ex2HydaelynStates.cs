﻿namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

class Ex2HydaelynStates : StateMachineBuilder
{
    public Ex2HydaelynStates(BossModule module) : base(module)
    {
        SimplePhase(0, Phase1, "P1")
            .ActivateOnEnter<WeaponTracker>()
            .Raw.Update = () => Module.Enemies(OID.CrystalOfLight).Count != 0;
        DeathPhase(1, Phase2)
            .ActivateOnEnter<WeaponTracker>();
    }

    private void Phase1(uint id)
    {
        HerosRadiance(id, 10.2f);
        ShiningSaber(id + 0x10000, 5.2f);
        CrystallizeSwitchWeapon(id + 0x20000, 5.5f, false);
        ForkByWeapon(id + 0x30000, 8, ForkFirstStaff, ForkFirstChakram);
    }

    private void ForkByWeapon(uint id, uint secondOffset, Action<uint> forkStaff, Action<uint> forkChakram)
    {
        Dictionary<WeaponTracker.Stance, (uint seqID, Action<uint> buildState)> dispatch = new()
        {
            [WeaponTracker.Stance.Staff] = ((id >> 24) + 1, forkStaff),
            [WeaponTracker.Stance.Chakram] = ((id >> 24) + secondOffset, forkChakram)
        };
        ComponentConditionFork<WeaponTracker, WeaponTracker.Stance>(id, 0, _ => true, comp => comp.CurStance, dispatch);
    }

    private void ForkFirstStaff(uint id)
    {
        MagosRadiance(id, 2.7f);
        Aureole(id + 0x10000, 5.2f);
        CrystallizeSwitchWeapon(id + 0x20000, 4.6f, false);
        MousaScorn(id + 0x30000, 3.2f);
        Aureole(id + 0x40000, 5.2f);
        CrystallizeSwitchWeapon(id + 0x50000, 4.6f, true);
    }

    private void ForkFirstChakram(uint id)
    {
        MousaScorn(id, 3.2f);
        Aureole(id + 0x10000, 5.2f);
        CrystallizeSwitchWeapon(id + 0x20000, 4.6f, false);
        MagosRadiance(id + 0x30000, 2.7f);
        Aureole(id + 0x40000, 5.2f);
        CrystallizeSwitchWeapon(id + 0x50000, 4.6f, true);
    }

    private void Phase2(uint id)
    {
        Intermission(id + 0x100000, 2.2f);
        Halo(id + 0x200000, 10.2f);
        Lightwave1(id + 0x210000, 4.1f);
        Lightwave2(id + 0x220000, 2.1f);
        Halo(id + 0x230000, 3.8f);
        HerosSundering(id + 0x240000, 6.1f);
        ShiningSaber(id + 0x250000, 5.3f);
        SwitchWeapon(id + 0x260000, 3.4f, false);
        ForkByWeapon(id + 0x270000, 4, ForkSecondStaff, ForkSecondChakram);
    }

    private void ForkSecondStaff(uint id)
    {
        MagosRadiance(id, 5.2f);
        CrystallizeParhelicCircleAureole(id + 0x10000, 5.2f);
        SwitchWeapon(id + 0x20000, 2.5f, false);
        MousaScorn(id + 0x30000, 5.2f);
        ParhelionCrystallizeAureole(id + 0x40000, 6.8f);
        SwitchWeapon(id + 0x50000, 2.5f, true);
        ForkSecondMerge(id, 8);
    }

    private void ForkSecondChakram(uint id)
    {
        MousaScorn(id, 5.2f);
        ParhelionCrystallizeAureole(id + 0x10000, 6.8f);
        SwitchWeapon(id + 0x20000, 2.5f, false);
        MagosRadiance(id + 0x30000, 5.2f);
        CrystallizeParhelicCircleAureole(id + 0x40000, 5.2f);
        SwitchWeapon(id + 0x50000, 2.5f, true);
        ForkSecondMerge(id, 8);
    }

    private void ForkSecondMerge(uint id, float delay)
    {
        RadiantHalo(id + 0x100000, delay);
        Lightwave3(id + 0x110000, 5.2f);
        CrystallizeShiningSaber(id + 0x120000, 9.1f); // TODO: can there be aureole instead of saber here?..
        SwitchWeapon(id + 0x130000, 1.3f, false, true); // note: we don't create a fork here, since it's kind of irrelevant...
        Lightwave3(id + 0x140000, 7.3f);
        CrystallizeAureole(id + 0x150000, 9.1f, true);
        SwitchWeapon(id + 0x160000, 1.3f, false, true);
        CrystallizeAureole(id + 0x170000, 7.3f, false);
        SwitchWeapon(id + 0x180000, 1.3f, true, true);
        Cast(id + 0x190000, (uint)AID.HerosRadianceEnrage, 9.5f, 10, "Enrage");
    }

    private void Intermission(uint id, float delay)
    {
        var echoes = Module.Enemies(OID.Echo);

        Targetable(id, false, delay, "Intermission start");
        ComponentCondition<PureCrystal>(id + 0x10000, 12.5f, comp => comp.NumCasts > 0, "Raidwide + adds appear")
            .ActivateOnEnter<PureCrystal>()
            .DeactivateOnExit<PureCrystal>()
            .SetHint(StateMachine.StateHint.DowntimeEnd); // crystals become targetable ~0.1s before, echoes ~0.1s after
        Condition(id + 0x20000, 60, () => !echoes.Any(e => e.IsTargetable && !e.IsDead), "Adds down", 10000, 1) // note that time is arbitrary
            .ActivateOnEnter<IntermissionAdds>()
            .DeactivateOnExit<IntermissionAdds>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
        // +2.1s: boss casts 26043 'Exodus'
        ComponentCondition<Exodus>(id + 0x30000, 16.9f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<Exodus>()
            .DeactivateOnExit<Exodus>();
        Targetable(id + 0x40000, true, 5.2f, "Intermission end");
    }

    private void Lightwave1(uint id, float delay)
    {
        Cast(id, (uint)AID.LightwaveSword, delay, 4, "Lightwave1");
        ComponentCondition<Lightwave1>(id + 0x1000, 12.1f, comp => comp.NumCasts > 0, "Crystal1")
            .ActivateOnEnter<Lightwave1>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<Lightwave1>(id + 0x2000, 2.1f, comp => comp.NumCasts > 1, "Crystal2");
        CastStart(id + 0x3000, (uint)AID.InfralateralArc, 1.3f)
            .ActivateOnEnter<InfralateralArc>();
        CastEnd(id + 0x3001, 4.9f, "InfralateralArc");
        ComponentCondition<Lightwave1>(id + 0x4000, 1.3f, comp => comp.NumCasts > 2, "Crystal3")
            .DeactivateOnExit<Lightwave1>();
        ComponentCondition<InfralateralArc>(id + 0x6000, 2.2f, comp => comp.NumCasts > 2, "Resolve", 1.5f) // very large variance here...
            .DeactivateOnExit<InfralateralArc>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void Lightwave2(uint id, float delay)
    {
        Cast(id, (uint)AID.LightwaveSword, delay, 4, "Lightwave2");
        Cast(id + 0x1000, (uint)AID.HerosGlory, 4.7f, 5, "Glory1")
            .ActivateOnEnter<Lightwave2>() // note that we don't show any hints until first glory starts casting, since it's a bit misleading...
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<Lightwave2>(id + 0x2000, 4.6f, comp => comp.NumCasts > 0, "Crystal1");
        ComponentCondition<Lightwave2>(id + 0x3000, 3.0f, comp => comp.NumCasts > 1);
        ComponentCondition<Lightwave2>(id + 0x4000, 2.9f, comp => comp.NumCasts > 2);
        ComponentCondition<Lightwave2>(id + 0x5000, 3.0f, comp => comp.NumCasts > 3);
        Cast(id + 0x6000, (uint)AID.HerosGlory, 0.5f, 5, "Glory2");
        ComponentCondition<Lightwave2>(id + 0x7000, 1.3f, comp => comp.NumCasts > 4, "Resolve")
            .DeactivateOnExit<Lightwave2>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    // note: keeps Lightwave3 component active, since it is relevant for next mechanic
    private void Lightwave3(uint id, float delay)
    {
        CastMulti(id, [(uint)AID.LightwaveSword, (uint)AID.LightwaveStaff, (uint)AID.LightwaveChakram], delay, 4, "Lightwave");
        CastStartMulti(id + 0x1000, [(uint)AID.EchoesSword, (uint)AID.EchoesStaff, (uint)AID.EchoesChakram], 15.2f)
            .ActivateOnEnter<Lightwave3>()
            .ActivateOnEnter<Echoes>(); // note that icon appears slightly before cast start...
        CastEnd(id + 0x1001, 5, "Stack");
        // + ~1.0s: new lightwaves
        ComponentCondition<Echoes>(id + 0x2000, 4.5f, comp => comp.NumCasts > 4, "Echoes resolve")
            .DeactivateOnExit<Echoes>()
            .DeactivateOnExit<Lightwave3>();
        ComponentCondition<Spectrum>(id + 0x3000, 3.5f, comp => comp.NumCasts > 0, "Stack/spread")
            .ActivateOnEnter<Spectrum>()
            .ActivateOnEnter<Lightwave3>()
            .DeactivateOnExit<Spectrum>();
    }

    private void HerosRadiance(uint id, float delay)
    {
        Cast(id, (uint)AID.HerosRadiance, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void MagosRadiance(uint id, float delay)
    {
        Cast(id, (uint)AID.MagosRadiance, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void MousaScorn(uint id, float delay)
    {
        Cast(id, (uint)AID.MousaScorn, delay, 5, "Shared Tankbuster")
            .ActivateOnEnter<MousaScorn>()
            .DeactivateOnExit<MousaScorn>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Halo(uint id, float delay)
    {
        Cast(id, (uint)AID.Halo, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void RadiantHalo(uint id, float delay)
    {
        Cast(id, (uint)AID.RadiantHalo, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void HerosSundering(uint id, float delay)
    {
        Cast(id, (uint)AID.HerosSundering, delay, 5, "AOE Tankbuster")
            .ActivateOnEnter<HerosSundering>()
            .DeactivateOnExit<HerosSundering>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void ShiningSaber(uint id, float delay)
    {
        // note: resolve happens slightly after cast, but variance is too large (0.2-0.5s), so just ignore it...
        Cast(id, (uint)AID.ShiningSaber, delay, 4.9f, "Stack")
            .ActivateOnEnter<ShiningSaber>()
            .DeactivateOnExit<ShiningSaber>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State Aureole(uint id, float delay)
    {
        // note: what is the difference between aureole spells? seems to be determined by weapon?..
        CastMulti(id, [(uint)AID.Aureole1, (uint)AID.Aureole2, (uint)AID.LateralAureole1, (uint)AID.LateralAureole2], delay, 5f)
            .ActivateOnEnter<Aureole>();
        return ComponentCondition<Aureole>(id + 2, 0.5f, comp => comp.NumCasts != 0, "Aureole")
            .DeactivateOnExit<Aureole>();
    }

    private void ParhelicCircle(uint id, float delay)
    {
        Cast(id, (uint)AID.ParhelicCircle, delay, 6, "Orbs")
            .ActivateOnEnter<ParhelicCircle>();
        ComponentCondition<ParhelicCircle>(id + 0x10, 1.9f, comp => comp.NumCasts > 0, "Orbs resolve")
            .DeactivateOnExit<ParhelicCircle>();
    }

    private void SwitchWeapon(uint id, float delay, bool toSword, bool shorter = default)
    {
        ComponentCondition<WeaponTracker>(id, delay, comp => comp.AOEImminent, "Select weapon");
        ComponentCondition<WeaponTracker>(id + 0x10, toSword && !shorter ? 6.9f : !toSword && !shorter ? 6 : toSword && shorter ? 5.8f : 5, comp => !comp.AOEImminent, "Weapon AOE");
    }

    // note: activates Crystallize component and sets positioning flag
    private State CrystallizeCast(uint id, float delay, string name = "Crystallize")
    {
        // note: there are several crystallize spells, concrete is determined by element and current weapon; weapon to switch to doesn't seem to matter
        return CastMulti(id, [(uint)AID.CrystallizeSwordStaffWater, (uint)AID.CrystallizeStaffEarth, (uint)AID.CrystallizeStaffIce, (uint)AID.CrystallizeChakramIce, (uint)AID.CrystallizeChakramEarth, (uint)AID.CrystallizeChakramWater], delay, 4, name)
            .ActivateOnEnter<Crystallize>()
            .SetHint(StateMachine.StateHint.PositioningStart);
    }

    // note: deactivates Crystallize component and clears positioning flag
    private void CrystallizeResolve(uint id, float delay, string name = "Element resolve")
    {
        ComponentCondition<Crystallize>(id, delay, comp => comp.CurElement == Crystallize.Element.None, name)
            .DeactivateOnExit<Crystallize>()
            .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd);
    }

    private void CrystallizeSwitchWeapon(uint id, float delay, bool toSword)
    {
        CrystallizeCast(id, delay);
        SwitchWeapon(id + 0x200, 3.1f, toSword);
        CrystallizeResolve(id + 0x300, toSword ? 3.2f : 4);
    }

    private void CrystallizeParhelicCircleAureole(uint id, float delay)
    {
        CrystallizeCast(id, delay, "Crystallize (ice)");
        ParhelicCircle(id + 0x1000, 4.8f);
        CrystallizeResolve(id + 0x3000, 3.5f, "Ice resolve");
        Aureole(id + 0x4000, 1);
    }

    private void ParhelionCrystallizeAureole(uint id, float delay)
    {
        Cast(id, (uint)AID.Parhelion, delay, 5, "Parhelion")
            .ActivateOnEnter<Parhelion>();
        CrystallizeCast(id + 0x1000, 4.8f, "Crystallize (water)");
        Cast(id + 0x2000, (uint)AID.Subparhelion, 3.2f, 5, "Subparhelion");
        CrystallizeResolve(id + 0x2800, 2, "Water resolve");
        Aureole(id + 0x3000, 3.3f) // note that aureole cast starts slightly before last subparhelion resolves
            .DeactivateOnExit<Parhelion>(); // note that last beacon happens slightly after cast start
    }

    // note: expects Lightwave3 component
    private void CrystallizeShiningSaber(uint id, float delay)
    {
        CrystallizeCast(id, delay)
            .DeactivateOnExit<Lightwave3>();
        ShiningSaber(id + 0x1000, 3.2f);
        CrystallizeResolve(id + 0x3000, 4.3f);
    }

    private void CrystallizeAureole(uint id, float delay, bool afterLightwave)
    {
        CrystallizeCast(id, delay)
            .DeactivateOnExit<Lightwave3>(afterLightwave);
        Aureole(id + 0x1000, 3.1f);
        CrystallizeResolve(id + 0x3000, 3.7f);
    }
}
