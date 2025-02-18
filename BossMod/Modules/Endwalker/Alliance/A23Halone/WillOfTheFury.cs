namespace BossMod.Endwalker.Alliance.A23Halone;

class WillOfTheFury(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private const float _impactRadiusIncrement = 6f;
    public bool Active => _aoe != null;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WillOfTheFuryAOE1)
        {
            UpdateAOE(Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.WillOfTheFuryAOE1:
            case (uint)AID.WillOfTheFuryAOE2:
            case (uint)AID.WillOfTheFuryAOE3:
            case (uint)AID.WillOfTheFuryAOE4:
            case (uint)AID.WillOfTheFuryAOE5:
                ++NumCasts;
                UpdateAOE(WorldState.FutureTime(2d));
                break;
        }
    }

    private void UpdateAOE(DateTime activation)
    {
        var outerRadius = (5 - NumCasts) * _impactRadiusIncrement;
        AOEShape? shape = NumCasts switch
        {
            < 4 => new AOEShapeDonut(outerRadius - _impactRadiusIncrement, outerRadius),
            4 => new AOEShapeCircle(outerRadius),
            _ => null
        };
        _aoe = shape != null ? new(shape, Arena.Center, default, activation) : null;
    }
}
