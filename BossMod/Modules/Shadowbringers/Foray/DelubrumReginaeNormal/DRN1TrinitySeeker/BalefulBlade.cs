﻿namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN1TrinitySeeker;

sealed class BalefulBlade(BossModule module) : Components.GenericAOEs(module)
{
    private bool _phantomEdge;
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe is not AOEInstance aoe)
            return;
        hints.Add(_phantomEdge ? "Stay in front of barricade!" : "Hide behind barricade!", !aoe.Check(actor.Position));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.BalefulBladeVisual1 or (uint)AID.BalefulBladeVisual2)
        {
            var shapes = new Shape[4];
            var centerPos = Arena.Center;
            var halfAngle = 22.5f.Degrees();
            if (spell.Action.ID == (uint)AID.BalefulBladeVisual1)
            {
                for (var i = 0; i < 4; ++i)
                {
                    shapes[i] = new DonutSegmentV(centerPos, 20f, 30f, (45f + i * 90f).Degrees(), halfAngle, 6);
                }
                _phantomEdge = false;
            }
            else
            {
                for (var i = 0; i < 4; ++i)
                {
                    shapes[i] = new ConeV(centerPos, 20f, (45f + i * 90f).Degrees(), halfAngle, 6);
                }
                _phantomEdge = true;
            }
            _aoe = new(new AOEShapeCustom(shapes, InvertForbiddenZone: true), centerPos, default, Module.CastFinishAt(spell, 0.1f), Colors.SafeFromAOE);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BalefulBlade1 or (uint)AID.BalefulBlade2)
        {
            _aoe = null;
        }
    }
}
