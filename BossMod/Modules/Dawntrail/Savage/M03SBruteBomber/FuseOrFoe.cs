namespace BossMod.Dawntrail.Savage.M03SBruteBomber;

class InfernalSpin(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCone _shape = new(40, 30.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var increment = (AID)spell.Action.ID switch
        {
            AID.InfernalSpinFirstCW => -45.Degrees(),
            AID.InfernalSpinFirstCCW => 45.Degrees(),
            _ => default
        };
        if (increment != default)
        {
            Sequences.Add(new(_shape, caster.Position, spell.Rotation, increment, Module.CastFinishAt(spell, 0.5f), 1.1f, 8));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.InfernalSpinFirstAOE or AID.InfernalSpinRestAOE)
            AdvanceSequence(0, WorldState.CurrentTime);
    }
}

class ExplosiveRain(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(8), new AOEShapeDonut(8, 16), new AOEShapeDonut(16, 24)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ExplosiveRain11 or AID.ExplosiveRain21)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.ExplosiveRain11 or AID.ExplosiveRain21 => 0,
            AID.ExplosiveRain12 or AID.ExplosiveRain22 => 1,
            AID.ExplosiveRain13 or AID.ExplosiveRain23 => 2,
            _ => -1
        };
        if (!AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(4)))
            ReportError($"Unexpected ring {order}");
    }
}
