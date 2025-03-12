namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS5Phantom;

// TODO: improve hints, currently they are not good... we probably don't need a fully generic implementation, since there are few possible patterns
class Miasma(BossModule module) : Components.GenericAOEs(module)
{
    public enum Order { Unknown, LowHigh, HighLow }

    public struct LaneState
    {
        public AOEShape? Shape;
        public int NumCasts;
        public DateTime Activation;
        public WPos NextOrigin;
    }

    public int NumLanesFinished;
    private readonly LaneState[,] _laneStates = new LaneState[4, 2];
    private Order _order;

    private static readonly AOEShapeRect _shapeRect = new(50, 6);
    private static readonly AOEShapeCircle _shapeCircle = new(8);
    private static readonly AOEShapeDonut _shapeDonut = new(5, 19);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_order == Order.Unknown)
            return [];

        var (order1, order2) = _order == Order.HighLow ? (1, 0) : (0, 1);

        var count = 0;
        for (var i = 0; i < 4; ++i)
        {
            if (_laneStates[i, order1].Shape != null)
                ++count;
            if (_laneStates[i, order2].Shape != null && (_laneStates[i, order1].Shape == null || _laneStates[i, order1].Shape == _shapeDonut && _laneStates[i, order2].Shape == _shapeRect))
                ++count;
        }

        if (count == 0)
            return [];

        var aoes = new AOEInstance[count];
        var index = 0;

        for (var i = 0; i < 4; ++i)
        {
            var l1 = _laneStates[i, order1];
            var l2 = _laneStates[i, order2];

            if (l1.Shape != null)
                aoes[index++] = new(l1.Shape, l1.NextOrigin, new(), l1.Activation);

            if (l2.Shape != null && (l1.Shape == null || l1.Shape == _shapeDonut && l2.Shape == _shapeRect))
                aoes[index++] = new(l2.Shape, l2.NextOrigin, new(), l2.Activation);
        }

        return aoes;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_order != Order.Unknown)
            hints.Add($"Order: {(_order == Order.HighLow ? "high > low" : "low > high")}");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var order = spell.Action.ID switch
        {
            (uint)AID.ManipulateMiasma => Order.LowHigh,
            (uint)AID.InvertMiasma => Order.HighLow,
            _ => Order.Unknown
        };
        if (order != Order.Unknown)
            _order = order;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.CreepingMiasmaFirst or (uint)AID.CreepingMiasmaRest => _shapeRect,
            (uint)AID.LingeringMiasmaFirst or (uint)AID.LingeringMiasmaRest => _shapeCircle,
            (uint)AID.SwirlingMiasmaFirst or (uint)AID.SwirlingMiasmaRest => _shapeDonut,
            _ => null
        };
        if (shape == null)
            return;

        var laneIndex = LaneIndex(shape == _shapeRect ? caster.Position : spell.TargetXZ);
        if (spell.Action.ID is (uint)AID.CreepingMiasmaFirst or (uint)AID.LingeringMiasmaFirst or (uint)AID.SwirlingMiasmaFirst)
        {
            var heightIndex = (_laneStates[laneIndex, 0].NumCasts, _laneStates[laneIndex, 1].NumCasts) switch
            {
                (_, > 0) => 0,
                ( > 0, _) => 1,
                _ => _order == Order.HighLow ? 1 : 0,
            };

            ref var l = ref _laneStates[laneIndex, heightIndex];
            if (l.Shape != shape || l.NumCasts != 0)
            {
                ReportError($"Unexpected state at first-cast end");
            }
            else
            {
                AdvanceLane(ref l);
            }
        }
        else
        {
            // note: for non-rects, we get single 'rest' cast belonging to first set right after 'first' cast of second set
            var heightIndex =
                _laneStates[laneIndex, 0].Shape != shape ? 1 :
                _laneStates[laneIndex, 1].Shape != shape ? 0 :
                _laneStates[laneIndex, 0].NumCasts > _laneStates[laneIndex, 1].NumCasts ? 0 : 1;

            ref var l = ref _laneStates[laneIndex, heightIndex];
            if (l.Shape != shape)
            {
                ReportError($"Unexpected state at rest-cast end");
            }
            else
            {
                AdvanceLane(ref l);
            }
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state != 0x00010002)
            return; // other states: 00080010 - start glowing, 00040020 - disappear
        AOEShape? shape = actor.OID switch
        {
            (uint)OID.MiasmaLowRect or (uint)OID.MiasmaHighRect => _shapeRect,
            (uint)OID.MiasmaLowCircle or (uint)OID.MiasmaHighCircle => _shapeCircle,
            (uint)OID.MiasmaLowDonut or (uint)OID.MiasmaHighDonut => _shapeDonut,
            _ => null
        };
        if (shape == null)
            return;
        var heightIndex = actor.OID is (uint)OID.MiasmaLowRect or (uint)OID.MiasmaLowCircle or (uint)OID.MiasmaLowDonut ? 0 : 1;
        var laneIndex = LaneIndex(actor.Position);
        _laneStates[laneIndex, heightIndex] = new() { Shape = shape, Activation = WorldState.FutureTime(16.1d), NextOrigin = new(actor.Position.X, Arena.Center.Z - Arena.Bounds.Radius + (shape == _shapeRect ? 0 : 5)) };
    }

    private int LaneIndex(WPos pos) => (pos.X - Arena.Center.X) switch
    {
        < -10 => 0,
        < 0 => 1,
        < 10 => 2,
        _ => 3,
    };

    private void AdvanceLane(ref LaneState lane)
    {
        lane.Activation = WorldState.FutureTime(1.6d);
        ++lane.NumCasts;
        if (lane.Shape == _shapeRect)
        {
            if (lane.NumCasts >= 3)
            {
                lane.Shape = null;
                ++NumLanesFinished;
            }
        }
        else
        {
            lane.NextOrigin.Z += 6;
            if (lane.NumCasts >= 8)
            {
                lane.Shape = null;
                ++NumLanesFinished;
            }
        }
    }
}
