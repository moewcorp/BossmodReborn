﻿namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN1TrinitySeeker;

class IronSplitter(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.IronSplitter))
{
    private readonly List<AOEInstance> _aoes = new(3);
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(4f), new AOEShapeDonut(8f, 12f), new AOEShapeDonut(16f, 20f),
    new AOEShapeDonut(4f, 8f), new AOEShapeDonut(12f, 16f), new AOEShapeDonut(20f, 25f)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var distance = (caster.Position - Arena.Center).Length();
            var activation = Module.CastFinishAt(spell);
            var pos = WPos.ClampToGrid(Arena.Center);
            if (distance is < 3 or > 9 and < 11 or > 17 and < 19) // tiles
            {
                _aoes.Add(new(_shapes[0], pos, default, activation));
                _aoes.Add(new(_shapes[1], pos, default, activation));
                _aoes.Add(new(_shapes[2], pos, default, activation));
            }
            else
            {
                _aoes.Add(new(_shapes[3], pos, default, activation));
                _aoes.Add(new(_shapes[4], pos, default, activation));
                _aoes.Add(new(_shapes[5], pos, default, activation));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _aoes.Clear();
    }
}
