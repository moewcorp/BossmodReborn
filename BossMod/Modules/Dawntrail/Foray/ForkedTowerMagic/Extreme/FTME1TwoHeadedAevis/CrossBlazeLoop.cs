namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME1TwoHeadedAevis;

sealed class CrossBlazeLoop(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [with(4)];
    private readonly AOEShapeCircle _circle = new(5f);
    private readonly AOEShapeDonut _donut = new(5f, 60f);
    private readonly AOEShapeCross _cross = new(35f, 5f);
    private Actor? _nextPos; // actual boss OID, instanceID of boss helper, aoe origin
    private AOEShape? _secondShape;
    private DateTime _activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
        {
            return [];
        }

        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(AOEs);
        return aoes[..max];
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Tether)
        {
            var target = WorldState.Actors.Find(tether.Target);
            if (target?.OID == (uint)OID.CrossBlazeTarget && source.OID is var oid && oid is (uint)OID.GreenHead1 or (uint)OID.BlueHead1)
            {
                _nextPos = target;
                InitIfReady();
            }
        }
    }

    private void InitIfReady()
    {
        if (_nextPos is Actor nextPos && _secondShape is AOEShape shape)
        {
            var loc = nextPos.Position.Quantized();
            var rot = Angle.AnglesCardinals[1];
            _nextPos = null;
            _secondShape = null;
            AddAOE(_circle);
            AddAOE(shape, 2d);

            void AddAOE(AOEShape shape, double delay = 0d)
            {
                var is1st = delay == 0d;
                AOEs.Add(new(shape, loc, rot, is1st ? _activation : _activation.AddSeconds(2d), is1st ? Colors.Danger : default, shapeDistance: shape.Distance(loc, rot)));
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // actual AOE and visual indicators ID by order cast, not specific spell
        // either boss can start with crossblaze / blazeloop
        // either repeats the same donut/cross or switches
        // tether comes from boss helper 1 tick before boss does the cast indicating mechanic
        // use actual target actor; actor position at time of tether not at position of AOE
        // depending on server ticks and latency tether and cast start can arrive in the same frame and random order, so we need to cache 2nd shape and activation and check
        // if we are ready
        if (spell.Action.ID is var id && id is >= (uint)AID.CrossblazeAndRepeat1 and <= (uint)AID.BlazeloopCrossblaze2)
        {
            _activation = Module.CastFinishAt(spell);

            if ((id & 1) == 0) // donut
            {
                _secondShape = _donut;
            }
            else // cross
            {
                _secondShape = _cross;
            }
            InitIfReady();
        }
        else if (id is (uint)AID.CrossblazeCast or (uint)AID.BlazeloopCast)
        {
            _activation = Module.CastFinishAt(spell);
            _secondShape = id == (uint)AID.CrossblazeCast ? _cross : _donut;
            InitIfReady();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var count = AOEs.Count;
        if (count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.BlazeFirst:
                case (uint)AID.BlazeSecond:
                case (uint)AID.Blazeloop:
                case (uint)AID.Crossblaze:
                case (uint)AID.BlazeFollowup:
                    ++NumCasts;
                    AOEs.RemoveAt(0);
                    if (count >= 2)
                    {
                        var aoes = CollectionsMarshal.AsSpan(AOEs);
                        aoes[0].Color = default;
                        if (count >= 3)
                        {
                            aoes[1].Color = Colors.Danger;
                        }
                    }
                    break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        /*
        // stay near initial circle if not during knockback
        if (_aoes.Count != 0)
        {
            var hissing = Module.FindComponent<HissingResonance>();
            ref var aoe = ref _aoes.Ref(0);
            if (hissing == null && aoe.Shape is AOEShapeCircle)
            {
                hints.GoalZones.Add(AIHints.GoalSingleTarget(aoe.Origin, 8f));
            }
            base.AddAIHints(slot, actor, assignment, hints);
        }
        */
        if (AOEs.Count != 0)
        {
            ref var aoe = ref AOEs.Ref(0);
            if (aoe.Shape is AOEShapeCircle)
            {
                hints.GoalZones.Add(AIHints.GoalSingleTarget(aoe.Origin, 10f));
            }
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}
