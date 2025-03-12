namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

class TurretsTour : Components.GenericAOEs
{
    private readonly List<AOEInstance> _aoes = [];
    private DateTime _activation;

    private static readonly AOEShapeRect _defaultShape = new(50f, 3f);

    public TurretsTour(BossModule module) : base(module)
    {
        var turrets = module.Enemies((uint)OID.AutomaticTurret);
        var count = turrets.Count;

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

                if (_defaultShape.Check(potentialTarget.Position, t.Position, t.Rotation))
                {
                    var distance = (potentialTarget.Position - t.Position).LengthSq();
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestTarget = potentialTarget;
                    }
                }
            }

            var shape = closestTarget != null ? _defaultShape with { LengthFront = minDistance } : _defaultShape;
            _aoes.Add(new(shape, t.Position, t.Rotation, _activation, ActorID: t.InstanceID));
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TurretsTourNormalAOE1)
        {
            var toTarget = spell.LocXZ - caster.Position;
            _aoes.Add(new(new AOEShapeRect(toTarget.Length(), 3f), WPos.ClampToGrid(caster.Position), Angle.FromDirection(toTarget), Module.CastFinishAt(spell), ActorID: caster.InstanceID));
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TurretsTourNormalAOE1)
            RemoveAOE(caster.InstanceID);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TurretsTourNormalAOE2 or (uint)AID.TurretsTourNormalAOE3)
        {
            ++NumCasts;
            RemoveAOE(caster.InstanceID);
        }
    }

    private void RemoveAOE(ulong instanceID)
    {
        var count = _aoes.Count;
        for (var i = 0; i < count; ++i)
        {
            if (_aoes[i].ActorID == instanceID)
            {
                _aoes.RemoveAt(i);
                return;
            }
        }
    }
}
