﻿namespace BossMod.Endwalker.Alliance.A11Byregot;

class A11ByregotStates : StateMachineBuilder
{
    public A11ByregotStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<ByregotWard>()
            .ActivateOnEnter<ByregotStrikeJump>()
            .ActivateOnEnter<ByregotStrikeKnockback>()
            .ActivateOnEnter<ByregotStrikeCone>()
            .ActivateOnEnter<DestroySideTiles>()
            .ActivateOnEnter<HammersCells>()
            .ActivateOnEnter<HammersLevinforge>()
            .ActivateOnEnter<HammersSpire>()
            .ActivateOnEnter<Reproduce>();
    }

    private void SinglePhase(uint id)
    {
        OrdealOfThunder(id, 6.2f);
        ByregotStrikeSimple(id + 0x10000u, 5.8f);
        ByregotWard(id + 0x20000u, 8.2f);
        ByregotStrikeCones(id + 0x30000u, 5.1f);

        Hammers1(id + 0x100000u, 6.2f);
        ByregotStrikeAny(id + 0x110000u, 14.8f);
        OrdealOfThunder(id + 0x120000u, 2.1f);
        Reproduce(id + 0x130000u, 6.1f);
        ByregotWard(id + 0x140000u, 2.6f);

        Hammers2(id + 0x200000u, 2.1f);
        ByregotStrikeAny(id + 0x210000u, 16.1f);
        OrdealOfThunder(id + 0x220000u, 2.1f);
        ByregotWard(id + 0x230000u, 7.2f);
        Reproduce(id + 0x240000u, 5.1f);

        SimpleState(id + 0xFF0000u, 10, "???");
    }

    private void OrdealOfThunder(uint id, float delay)
    {
        Cast(id, (uint)AID.OrdealOfThunder, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ByregotWard(uint id, float delay)
    {
        Cast(id, (uint)AID.ByregotWard, delay, 5f);
        ComponentCondition<ByregotWard>(id + 2u, 0.1f, comp => comp.NumCasts != 0, "Tankbuster")
            .ResetComp<ByregotWard>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void ByregotStrikeSimple(uint id, float delay)
    {
        Cast(id, (uint)AID.ByregotStrikeJump, delay, 6f);
        ComponentCondition<ByregotStrikeKnockback>(id + 2u, 1f, comp => comp.NumCasts != 0, "Knockback")
            .ResetComp<ByregotStrikeKnockback>();
    }

    private void ByregotStrikeCones(uint id, float delay)
    {
        Cast(id, (uint)AID.BuilderBuild, delay, 3f);
        Cast(id + 0x10, (uint)AID.ByregotStrikeJumpCone, 2.6f, 6f);
        ComponentCondition<ByregotStrikeCone>(id + 0x20u, 0.9f, comp => comp.NumCasts != 0, "Cones");
        ComponentCondition<ByregotStrikeKnockback>(id + 0x21u, 0.1f, comp => comp.NumCasts != 0, "Knockback")
            .ResetComp<ByregotStrikeCone>()
            .ResetComp<ByregotStrikeKnockback>();
    }

    private void ByregotStrikeAny(uint id, float delay)
    {
        // with 'cone' variant, we'll first get "builder's build" visual, and that will delay actual cast by ~5.6s - ignore that for simplicity
        SimpleState(id, delay, "")
            .SetHint(StateMachine.StateHint.BossCastStart)
            .Raw.Update = _ => Module.PrimaryActor.CastInfo != null && Module.PrimaryActor.CastInfo.Action.ID is (uint)AID.ByregotStrikeJump or (uint)AID.ByregotStrikeJumpCone ? 0 : -1;
        CastEnd(id + 1u, 6f);
        ComponentCondition<ByregotStrikeKnockback>(id + 2u, 1f, comp => comp.NumCasts != 0, "Knockback + maybe cones")
            .ResetComp<ByregotStrikeKnockback>()
            .ResetComp<ByregotStrikeCone>();
    }

    private void Reproduce(uint id, float delay)
    {
        Cast(id, (uint)AID.Reproduce, delay, 3f);
        ComponentCondition<Reproduce>(id + 0x10u, 5f, comp => comp.Active);
        ComponentCondition<Reproduce>(id + 0x20u, 7f, comp => comp.NumCasts > 0, "Exaflares start");
        ComponentCondition<Reproduce>(id + 0x30u, 6.6f, comp => !comp.Active, "Exaflares resolve")
            .ResetComp<Reproduce>();
    }

    private void HammersStart(uint id, float delay)
    {
        ComponentCondition<HammersCells>(id, delay, comp => comp.Active);
        ComponentCondition<HammersCells>(id + 1u, 9f, comp => comp.NumCasts != 0, "Destroy side tiles");
    }

    private void HammersMove(uint id, float delay)
    {
        ComponentCondition<HammersCells>(id, delay, comp => comp.MovementPending);
        ComponentCondition<HammersCells>(id + 0x10u, 16f, comp => !comp.MovementPending, "Move tiles");
    }

    private void HammersLevinforge(uint id, float delay)
    {
        ComponentCondition<HammersLevinforge>(id, delay, comp => comp.Casters.Count != 0)
            .ResetComp<HammersLevinforge>();
        ComponentCondition<HammersCells>(id + 0x10u, 2.2f, comp => comp.MovementPending);
        ComponentCondition<HammersCells>(id + 0x20u, 16f, comp => !comp.MovementPending, "Move tiles");
        ComponentCondition<HammersLevinforge>(id + 0x21u, 0.3f, comp => comp.NumCasts != 0, "Narrow line")
            .ResetComp<HammersLevinforge>();
    }

    private void HammersSpire(uint id, float delay)
    {
        ComponentCondition<HammersCells>(id, delay, comp => comp.MovementPending);
        ComponentCondition<HammersSpire>(id + 0x10u, 7.1f, comp => comp.Casters.Count != 0);
        ComponentCondition<HammersCells>(id + 0x20u, 8.9f, comp => !comp.MovementPending, "Move tiles");
        ComponentCondition<HammersSpire>(id + 0x21u, 2.1f, comp => comp.NumCasts != 0, "Wide line");
    }

    private void HammersResolve(uint id, float delay)
    {
        ComponentCondition<HammersCells>(id, delay, comp => !comp.Active, "Hammers resolve")
            .ResetComp<HammersCells>()
            .ResetComp<HammersSpire>();
    }

    private void Hammers1(uint id, float delay)
    {
        HammersStart(id, delay);
        HammersMove(id + 0x100, 14.6f); // large variance
        HammersLevinforge(id + 0x200, 8.8f);
        HammersSpire(id + 0x300, 2.4f);
        HammersResolve(id + 0x400, 10.8f);
    }

    private void Hammers2(uint id, float delay)
    {
        HammersStart(id, delay);
        HammersLevinforge(id + 0x100, 14.6f); // large variance
        HammersLevinforge(id + 0x200, 8.6f);
        HammersSpire(id + 0x300, 2.4f);
        HammersResolve(id + 0x400, 10.8f);
    }
}
