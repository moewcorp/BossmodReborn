namespace BossMod.Shadowbringers.Alliance.A33RedGirl;

sealed class DiffuseEnergy(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCone _shape = new(12f, 60f.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var increment = iconID switch
        {
            (uint)IconID.RotateCW => -120f.Degrees(),
            (uint)IconID.RotateCCW => 120f.Degrees(),
            _ => default
        };
        if (increment != default)
        {
            Sequences.Add(new(_shape, WPos.ClampToGrid(actor.Position), actor.Rotation, increment, WorldState.FutureTime(5d), 2.7d, 6));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.DiffuseEnergyFirst or (uint)AID.DiffuseEnergyRest)
        {
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
        }
    }
}
