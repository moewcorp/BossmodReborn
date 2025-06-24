﻿namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

sealed class FreezingDust(BossModule module) : Components.StayMove(module)
{
    public int NumActiveFreezes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FreezingDust)
            Array.Fill(PlayerStates, new(Requirement.Move, Module.CastFinishAt(spell, 1)));
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.FreezingUp)
            ++NumActiveFreezes;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.FreezingUp)
            --NumActiveFreezes;
    }
}
