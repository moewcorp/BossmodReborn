﻿namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

class ParhelicCircle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(10);
    private static readonly AOEShapeCircle _circle = new(6);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        var activation = WorldState.FutureTime(7.6d);
        var c = Arena.Center;
        if (actor.OID == (uint)OID.RefulgenceHexagon)
        {
            var dir = c + 17f * actor.Rotation.ToDirection();
            _aoes.Add(new(_circle, WPos.ClampToGrid(c), default, activation));
            for (var i = 1; i < 7; ++i)
                _aoes.Add(new(_circle, WPos.ClampToGrid(WPos.RotateAroundOrigin(i * 60f, c, dir)), default, activation));
        }
        else if (actor.OID == (uint)OID.RefulgenceTriangle)
        {
            var dir = c + 8f * actor.Rotation.ToDirection();
            for (var i = 1; i < 4; ++i)
                _aoes.Add(new(_circle, WPos.ClampToGrid(WPos.RotateAroundOrigin(-60f + i * 120f, c, dir)), default, activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Incandescence)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }
}
