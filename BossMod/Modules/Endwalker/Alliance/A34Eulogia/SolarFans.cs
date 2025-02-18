namespace BossMod.Endwalker.Alliance.A34Eulogia;

class SolarFans(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.SolarFansAOE), 5f);

class RadiantRhythm(BossModule module) : Components.GenericAOEs(module)
{
    private Angle _nextAngle;
    private DateTime _activation;
    private static readonly AOEShapeDonutSector _shape = new(20f, 30f, 45f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation == default)
            return [];

        var aoes = new AOEInstance[2];
        var center = Arena.Center;
        // assumption: we always have 4 moves
        if (NumCasts < 8)
        {
            aoes[0] = new(_shape, center, _nextAngle, _activation, Colors.Danger);
            aoes[1] = new(_shape, center, _nextAngle + 180f.Degrees(), _activation, Colors.Danger);
        }
        if (NumCasts < 6)
        {
            var future = _activation.AddSeconds(2.1d);
            aoes[0] = new(_shape, center, _nextAngle + 90f.Degrees(), future);
            aoes[1] = new(_shape, center, _nextAngle - 90f.Degrees(), future);
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SolarFansAOE)
        {
            // assumption: flames always move CCW
            var startingAngle = Angle.FromDirection(spell.LocXZ - Arena.Center);
            NumCasts = 0;
            _nextAngle = startingAngle + 45f.Degrees();
            _activation = Module.CastFinishAt(spell, 2.8f);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RadiantFlight)
        {
            ++NumCasts;
            if (NumCasts == 8)
            {
                _nextAngle = default;
                _activation = default;
            }
            else if ((NumCasts & 1) == 0)
            {
                _nextAngle += 90f.Degrees();
                _activation = WorldState.FutureTime(2.1d);
            }
        }
    }
}

class RadiantFlourish(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeCircle _shape = new(25);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // assumption: we always have 4 moves, so flames finish where they start
        switch ((AID)spell.Action.ID)
        {
            case AID.SolarFansAOE:
                _aoes.Add(new(_shape, spell.LocXZ, default, Module.CastFinishAt(spell, 13.8f)));
                break;
            case AID.RadiantFlourish:
                // verify the assumption
                if (!_aoes.Any(aoe => aoe.Origin.AlmostEqual(caster.Position, 1)))
                    ReportError($"Unexpected AOE position: {caster.Position}");
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RadiantFlourish)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }
}
