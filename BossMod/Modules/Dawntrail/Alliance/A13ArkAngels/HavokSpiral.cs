namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

class HavokSpiral(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;
    private DateTime _activation;
    private readonly List<Angle> _rotation = [];

    private static readonly AOEShapeCone _shape = new(30, 15.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        _increment = (IconID)iconID switch
        {
            IconID.RotateCW => -30.Degrees(),
            IconID.RotateCCW => 30.Degrees(),
            _ => default
        };
        _activation = WorldState.FutureTime(5.5f);
        InitIfReady();
    }

    private void InitIfReady()
    {
        if (_rotation.Count == 3 && _increment != default)
        {
            for (var i = 0; i < 3; ++i)
                Sequences.Add(new(_shape, Arena.Center, _rotation[i], _increment, _activation, 1.2f, 8));
            _rotation.Clear();
            _increment = default;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HavocSpiralFirst)
        {
            _rotation.Add(spell.Rotation);
            InitIfReady();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HavocSpiralFirst or AID.HavocSpiralRest)
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
    }
}

class SpiralFinish(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.SpiralFinishAOE), 16)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 9), Module.CastFinishAt(Casters[0].CastInfo));
    }
}
