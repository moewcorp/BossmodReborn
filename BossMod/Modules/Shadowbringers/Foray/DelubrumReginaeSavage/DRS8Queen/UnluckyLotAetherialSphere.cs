﻿namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

class UnluckyLotAetherialSphere(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.UnluckyLotAetherialSphere))
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCircle circle = new(20);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OptimalOffensiveMoveSphere)
            _aoe = new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 2.6f));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.UnluckyLotAetherialSphere)
            _aoe = null;
    }
}
