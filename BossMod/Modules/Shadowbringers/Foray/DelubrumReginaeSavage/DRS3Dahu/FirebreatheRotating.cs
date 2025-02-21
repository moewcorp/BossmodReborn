namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS3Dahu;

class FirebreatheRotating(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;

    private static readonly AOEShapeCone _shape = new(60f, 45f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FirebreatheRotating)
        {
            Sequences.Add(new(_shape, caster.Position, spell.Rotation, _increment, Module.CastFinishAt(spell, 0.7f), 2f, 5));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.FirebreatheRotatingAOE)
        {
            AdvanceSequence(0, WorldState.CurrentTime);
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var angle = iconID switch
        {
            (uint)IconID.FirebreatheCW => -90f.Degrees(),
            (uint)IconID.FirebreatheCCW => 90f.Degrees(),
            _ => default
        };
        if (angle != default)
            _increment = angle;
    }
}
