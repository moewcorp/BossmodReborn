namespace BossMod.Endwalker.VariantCriterion.V02MR.V025Enenra;

class IntoTheFire(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(50f, 25f);
    private readonly List<AOEInstance> _aoes = new(2);
    private const float offset = 15.556349f;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00010002)
            HandleSmokeVisuals(actor, WorldState.FutureTime(16.6d));
    }

    private void HandleSmokeVisuals(Actor actor, DateTime activation)
    {
        var positionOffset = GetPositionOffsetForActor(actor);
        var rotation = GetRotationForActor(actor);

        if (positionOffset != null && rotation != null)
        {
            var correctedPosition = WPos.ClampToGrid(RoundPosition(actor.Position + positionOffset.Value));
            _aoes.Add(new(rect, correctedPosition, rotation.Value, activation));
        }
    }

    private static WDir? GetPositionOffsetForActor(Actor actor)
    {
        return actor.OID switch
        {
            (uint)OID.SmokeVisual1 => offset * (actor.Rotation - 45f.Degrees()).ToDirection(),
            (uint)OID.SmokeVisual2 => offset * (actor.Rotation + 45f.Degrees()).ToDirection(),
            (uint)OID.SmokeVisual3 => 22f * actor.Rotation.ToDirection(),
            _ => null
        };
    }

    private static Angle? GetRotationForActor(Actor actor)
    {
        return actor.OID switch
        {
            (uint)OID.SmokeVisual1 => actor.Rotation + 90f.Degrees(),
            (uint)OID.SmokeVisual2 => actor.Rotation - 90f.Degrees(),
            (uint)OID.SmokeVisual3 => actor.Rotation + 180f.Degrees(),
            _ => null
        };
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.IntoTheFire)
            _aoes.RemoveAt(0);
    }

    private static WPos RoundPosition(WPos position) => new(MathF.Round(position.X), MathF.Round(position.Z));
}