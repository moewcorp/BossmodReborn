namespace BossMod.Endwalker.VariantCriterion.C02AMR.C021Shishio;

class NoblePursuit(BossModule module) : Components.GenericAOEs(module)
{
    private WPos _posAfterLastCharge;
    private readonly List<AOEInstance> _charges = [];
    private readonly List<AOEInstance> _rings = [];

    private const float _chargeHalfWidth = 6f;
    private static readonly AOEShapeRect _shapeRing = new(5f, 50f, 5f);

    public bool Active => _charges.Count + _rings.Count > 0;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var countCharges = _charges.Count;
        var countRings = _rings.Count;
        var total = countCharges + countRings;
        if (total == 0)
            return [];
        var firstActivation = _charges.Count != 0 ? _charges[0].Activation : _rings.Count > 0 ? _rings[0].Activation : default;
        var deadline = firstActivation.AddSeconds(2.5d);
        List<AOEInstance> aoes = new(total);
        for (var i = 0; i < countCharges; ++i)
        {
            var aoe = _charges[i];
            if (aoe.Activation <= deadline)
                aoes.Add(aoe);
        }

        for (var i = 0; i < countRings; ++i)
        {
            var aoe = _rings[i];
            if (aoe.Activation <= deadline)
                aoes.Add(aoe);
        }
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.NRairin or (uint)OID.SRairin)
        {
            if (_charges.Count == 0)
            {
                ReportError("Ring appeared while no charges are in progress");
                return;
            }

            // see whether this ring shows next charge
            if (!_charges[^1].Check(actor.Position))
            {
                var nextDir = actor.Position - _posAfterLastCharge;
                if (Math.Abs(nextDir.X) < 0.1f)
                    nextDir.X = 0;
                if (Math.Abs(nextDir.Z) < 0.1f)
                    nextDir.Z = 0;
                nextDir = nextDir.Normalized();
                var ts = Module.Center + nextDir.Sign() * Module.Bounds.Radius - _posAfterLastCharge;
                var t = Math.Min(nextDir.X != 0f ? ts.X / nextDir.X : float.MaxValue, nextDir.Z != 0f ? ts.Z / nextDir.Z : float.MaxValue);
                _charges.Add(new(new AOEShapeRect(t, _chargeHalfWidth), _posAfterLastCharge, Angle.FromDirection(nextDir), _charges[^1].Activation.AddSeconds(1.4d)));
                _posAfterLastCharge += nextDir * t;
            }

            // ensure ring rotations are expected
            if (!_charges[^1].Rotation.AlmostEqual(actor.Rotation, 0.1f))
            {
                ReportError("Unexpected rotation for ring inside last pending charge");
            }

            _rings.Add(new(_shapeRing, actor.Position, actor.Rotation, _charges[^1].Activation.AddSeconds(0.8d)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NNoblePursuitFirst or (uint)AID.SNoblePursuitFirst)
        {
            var dir = spell.LocXZ - caster.Position;
            _charges.Add(new(new AOEShapeRect(dir.Length(), _chargeHalfWidth), caster.Position, Angle.FromDirection(dir), Module.CastFinishAt(spell)));
            _posAfterLastCharge = spell.LocXZ;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NNoblePursuitFirst:
            case (uint)AID.NNoblePursuitRest:
            case (uint)AID.SNoblePursuitFirst:
            case (uint)AID.SNoblePursuitRest:
                if (_charges.Count != 0)
                    _charges.RemoveAt(0);
                ++NumCasts;
                break;
            case (uint)AID.NLevinburst:
            case (uint)AID.SLevinburst:
                _rings.RemoveAll(r => r.Origin.AlmostEqual(caster.Position, 1f));
                break;
        }
    }
}
