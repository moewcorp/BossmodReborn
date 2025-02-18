﻿namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN2Dahu;

class Shockwave(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone _shape = new(15, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.LeftSidedShockwaveFirst or AID.RightSidedShockwaveFirst)
        {
            _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell, 2.6f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && (AID)spell.Action.ID is AID.LeftSidedShockwaveFirst or AID.RightSidedShockwaveFirst or AID.LeftSidedShockwaveSecond or AID.RightSidedShockwaveSecond)
            _aoes.RemoveAt(0);
    }
}
