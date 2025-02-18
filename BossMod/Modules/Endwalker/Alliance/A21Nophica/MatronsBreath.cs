﻿namespace BossMod.Endwalker.Alliance.A21Nophica;

class MatronsBreath(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _blueSafe = module.Enemies(OID.BlueSafeZone);
    private readonly List<Actor> _goldSafe = module.Enemies(OID.GoldSafeZone);
    private readonly List<AOEInstance> _flowers = [];

    private static readonly AOEShapeDonut _shape = new(8, 50);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_flowers.Count != 0)
            yield return _flowers[0];
    }

    public override void OnActorCreated(Actor actor)
    {
        var safezone = (OID)actor.OID switch
        {
            OID.BlueFlowers => _blueSafe.FirstOrDefault(),
            OID.GoldFlowers => _goldSafe.FirstOrDefault(),
            _ => null
        };
        if (safezone != null)
            _flowers.Add(new(_shape, safezone.Position, default, WorldState.FutureTime(11.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Blueblossoms or AID.Giltblossoms)
        {
            ++NumCasts;
            if (_flowers.Count != 0)
                _flowers.RemoveAt(0);
        }
    }
}
