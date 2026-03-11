namespace BossMod.Modules.Dawntrail.Advanced.Ad01TheMerchantsTale.Ad011PariofPlenty;

sealed class FireFlight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private WPos _nextLanding = default;
    private DateTime _firstActivation = default;
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);
    private bool left;
    private Actor _who;
    private bool _active = false;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is ((uint)AID.LeftFireflight))
        {
            left = true;
            _who = caster;
            _active = true;
        }
        else if (spell.Action.ID is (uint)AID.RightFireflight)
        {
            left = false;
            _who = caster;
            _active = true;
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (_active)
        {
            if (tether.ID == (uint)TetherID.Fireflight)
            {
                var target = WorldState.Actors.Find(tether.Target);
                if (target == null) { return; }
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                var len = aoes.Length;
                var initial = len == 0 ? _who.Position : _nextLanding;
                _nextLanding = target.Position;
                if (_firstActivation == default)
                {
                    _firstActivation = WorldState.FutureTime(11.25d);
                }
                var dist = (_nextLanding - initial).Length();
                var rot = (_nextLanding - initial).ToAngle();
                var activation = _firstActivation.AddSeconds(2d * (len - 1));
                var conerot = left ? rot + 90.Degrees() : rot + 270.Degrees();
                //var conepos = initial + conerot.ToDirection() * 5f;
                _aoes.Add(new(new AOEShapeCone(60f, 90f.Degrees()), initial, conerot, activation));
                if (_aoes.Count == 3)
                {
                    // inner slighly larger but untethered helper location not exactly where boss lands
                    _aoes.Add(new(new AOEShapeDonut(7.7f, 60f), _nextLanding, activation: activation.AddSeconds(2.75d)));
                }
            }
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_active)
        {
            if (spell.Action.ID is (uint)AID.CarpetRide3 or (uint)AID.CarpetRide)
            {
                if (_aoes.Count > 0)
                {
                    _aoes.RemoveAt(0);
                    //if (_mech is Mechanic.fireFlight)
                    //{
                    //    _aoes.RemoveAt(0);
                    //}
                }
            }
            else if (spell.Action.ID == (uint)AID.SunCirclet)
            {
                _aoes.Clear();
                _nextLanding = default;
                _firstActivation = default;
                _active = false;
            }
        }
    }
}

sealed class FireFlightFactOrFiction(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private WPos _nextLanding = default;
    private DateTime _firstActivation = default;
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);
    private bool left;
    private Actor real;
    private bool _active = false;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.LeftFireflightFactAndFiction)
        {
            left = true;
            _active = true;
        }
        else if (spell.Action.ID is (uint)AID.RightFireflightFactAndFiction)
        {
            left = false;
            _active = true;
        }
    }
    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (_active)
        {
            if (_aoes.Count == 0)
            {
                if (source.Position.AlmostEqual(Module.PrimaryActor.Position, 0.5f))
                {
                    real = source;
                }
            }
            else
            {
                if (source.Position.AlmostEqual(_nextLanding, 0.5f))
                {
                    real = source;
                }
            }
            if (source == real)
            {
                var target = WorldState.Actors.Find(tether.Target);
                if (target == null) { return; }
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                var len = aoes.Length;
                var initial = len == 0 ? Module.PrimaryActor.Position : _nextLanding;
                _nextLanding = target.Position;
                if (_firstActivation == default)
                {
                    _firstActivation = WorldState.FutureTime(11.25d);
                }
                var dist = (_nextLanding - initial).Length();
                var rot = (_nextLanding - initial).ToAngle();
                var activation = _firstActivation.AddSeconds(2d * (len - 1));
                var conerot = left ? rot + 90.Degrees() : rot + 270.Degrees();
                //var conepos = initial + conerot.ToDirection() * 5f;
                _aoes.Add(new(new AOEShapeCone(60f, 90f.Degrees()), initial, conerot, activation));
                if (_aoes.Count == 3)
                {
                    // inner slighly larger but untethered helper location not exactly where boss lands
                    _aoes.Add(new(new AOEShapeDonut(7.7f, 60f), _nextLanding, activation: activation.AddSeconds(2.75d)));
                }
            }
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_active)
        {
            if (spell.Action.ID == (uint)AID.CarpetRide3)
            {
                if (_aoes.Count > 0)
                {
                    _aoes.RemoveAt(0);
                    //if (_mech is Mechanic.fireFlight)
                    //{
                    //    _aoes.RemoveAt(0);
                    //}
                }
            }
            else if (spell.Action.ID == (uint)AID.SunCirclet)
            {
                _aoes.Clear();
                _nextLanding = default;
                _firstActivation = default;
                _active = false;
            }
        }
    }
}

sealed class DoubleFableFlight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);
    private WPos _nextLanding = default;
    private DateTime _firstActivation = default;
    private bool left;
    private Actor _who;
    private bool _active = false;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is ((uint)AID.LeftFableflight1))
        {
            left = true;
            _who = caster;
            _active = true;
        }
        else if (spell.Action.ID is (uint)AID.RightFableflight1)
        {
            left = false;
            _who = caster;
            _active = true;
        }

    }
    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (_active)
        {
            if (tether.ID == (uint)TetherID.Fireflight)
            {
                var target = WorldState.Actors.Find(tether.Target);
                if (target == null) { return; }
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                var len = aoes.Length;
                var initial = len == 0 ? source.Position : _nextLanding;
                _nextLanding = target.Position;
                if (_firstActivation == default)
                {
                    _firstActivation = WorldState.FutureTime(11.25d);
                }
                var dist = (_nextLanding - initial).Length();
                var rot = (_nextLanding - initial).ToAngle();
                var activation = _firstActivation.AddSeconds(2d * (len - 1));
                var conerot = left ? rot + 90.Degrees() : rot + 270.Degrees();
                //var conepos = initial + conerot.ToDirection() * 5f;
                _aoes.Add(new(new AOEShapeCone(60f, 90f.Degrees()), initial, conerot, activation));
            }
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_active)
        {
            if (spell.Action.ID is (uint)AID.CarpetRide5 or (uint)AID.CarpetRide6 or (uint)AID.CarpetRide7)
            {
                if (_aoes.Count > 0)
                {
                    _aoes.RemoveAt(0);
                    //if (_mech is Mechanic.fireFlight)
                    //{
                    //    _aoes.RemoveAt(0);
                    //}
                }
            }
        }
    }
}
