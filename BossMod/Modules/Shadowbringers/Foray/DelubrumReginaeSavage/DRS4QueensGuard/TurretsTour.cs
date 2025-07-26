﻿namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

sealed class TurretsTour(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(6);
    private static readonly AOEShapeRect rect = new(55f, 3f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TurretsTourNormal)
        {
            var toTarget = spell.LocXZ - caster.Position;
            var act = Module.CastFinishAt(spell);
            _aoes.Add(new(new AOEShapeRect(toTarget.Length(), 3f), caster.Position.Quantized(), Angle.FromDirection(toTarget), act, actorID: caster.InstanceID));

            var turrets = Module.Enemies((uint)OID.AutomaticTurret);
            var count = turrets.Count;
            if (_aoes.Count != count / 2)
                return;
            for (var i = 0; i < count; ++i)
            {
                var t = turrets[i];
                var minDistance = float.MaxValue;
                Actor? closestTarget = null;

                for (var j = 0; j < count; ++j)
                {
                    if (i == j)
                        continue; // Exclude the current turret itself
                    var potentialTarget = turrets[j];

                    if (potentialTarget.Position.InRect(t.Position, t.Rotation, 55f, default, 0.1f)) // full half width of 3 gives false positives
                    {
                        var distance = (potentialTarget.Position - t.Position).LengthSq();
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestTarget = potentialTarget;
                        }
                    }
                }

                var shape = closestTarget != null ? new AOEShapeRect(MathF.Sqrt(minDistance), 3f) : rect;
                _aoes.Add(new(shape, t.Position, t.Rotation, act, actorID: t.InstanceID));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TurretsTourNormalFirst or (uint)AID.TurretsTourNormalRest1 or (uint)AID.TurretsTourNormalRest2)
        {
            var count = _aoes.Count;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].ActorID == caster.InstanceID)
                {
                    ++NumCasts;
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
