﻿namespace BossMod.Endwalker.Alliance.A23Halone;

class Lochos(BossModule module, float activationDelay) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly float _activationDelay = activationDelay;

    private static readonly AOEShapeRect _shape = new(60f, 15f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.LochosFirst or (uint)AID.LochosRest)
        {
            ++NumCasts;
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00200010)
        {
            (var offset, var dir) = index switch
            {
                8 => (new(+15f, -30f), default),
                9 => (new(-30f, -15f), 90f.Degrees()),
                10 => (new(+30f, +15), -90f.Degrees()),
                11 => (new(-15f, +30f), 180f.Degrees()),
                _ => (new WDir(), new Angle())
            };
            if (offset != default)
            {
                _aoes.Add(new(_shape, WPos.ClampToGrid(Arena.Center + offset), dir, WorldState.FutureTime(_activationDelay)));
            }
        }
    }
}
class Lochos1(BossModule module) : Lochos(module, 10.9f);
class Lochos2(BossModule module) : Lochos(module, 14.8f);
