namespace BossMod.Dawntrail.Ultimate.DMU;

sealed class Gaze(BossModule module) : Components.GenericGaze(module)
{
    private Eye[] _eye = [];

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == (uint)Animations.EyeStart)
        {
            var pos = actor.Position.Quantized();
            _eye = [new Eye(pos, WorldState.FutureTime(9.9d), inverted: actor.OID == (uint)OID.StatueYellowEye, eyeCenter: IndicatorWorldPos(pos))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.IndolentWill) or ((uint)AID.AveMaria))
        {
            ++NumCasts;
            _eye = [];
        }
    }

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor) => _eye;
}
