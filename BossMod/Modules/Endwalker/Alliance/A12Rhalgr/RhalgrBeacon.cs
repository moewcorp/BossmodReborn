﻿namespace BossMod.Endwalker.Alliance.A12Rhalgr;

class RhalgrBeaconAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RhalgrsBeaconAOE, 10f);

class RhalgrBeaconShock(BossModule module) : Components.GenericAOEs(module, (uint)AID.Shock)
{
    private readonly List<AOEInstance> _aoes = new(7);
    private static readonly AOEShapeCircle _shape = new(8);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (NumCasts != 7 && actor.OID == (uint)OID.LightningOrb)
        {
            _aoes.Add(new(_shape, WPos.ClampToGrid(actor.Position), default, WorldState.FutureTime(12.7d), ActorID: actor.InstanceID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.LightningOrb)
        {
            NumCasts = 0;
        }
    }
}

class RhalgrBeaconKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.RhalgrsBeaconKnockback, 50f, true, stopAfterWall: true, safeWalls: safewalls)
{
    private static readonly List<SafeWall> safewalls = [new(new(9.09f, 293.91f), new(3.31f, 297.2f)), new(new(-6.23f, 304.72f), new(-13.9f, 303.98f)),
    new(new(-22.35f, 306.16f), new(-31.3f, 304.94f)), new(new(-40.96f, 300.2f), new(-49.39f, 296.73f))];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;

        var source = Casters[0];
        var shock = Module.Enemies((uint)OID.LightningOrb);
        var count = shock.Count;
        var z = source.Position.Z;
        var forbidden = DetermineForbiddenZones(source.Position, shock, count, z);

        hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), DateTime.MaxValue);
    }

    private static Func<WPos, float>[] DetermineForbiddenZones(WPos sourcePos, List<Actor> shock, int count, float z)
    {
        var isZone268 = z == 268.5f;
        WPos zone1, zone2;

        if (isZone268)
        {
            zone1 = new(6.3f, 295.7f);
            zone2 = new(-10f, 304.2f);
        }
        else
        {
            zone1 = new(-27f, 305.7f);
            zone2 = new(-45f, 298.7f);
        }

        if (count == 0)
            return [ShapeDistance.InvertedRect(sourcePos, zone1, 0.5f), ShapeDistance.InvertedRect(sourcePos, zone2, 0.5f)];
        var inFirstZone = false;
        for (var i = 0; i < count; ++i)
        {
            if (shock[i].Position.InRect(sourcePos, zone1, 5f))
            {
                inFirstZone = true;
                break;
            }
        }
        return [ShapeDistance.InvertedRect(sourcePos, inFirstZone ? zone2 : zone1, 0.5f)];
    }
}
