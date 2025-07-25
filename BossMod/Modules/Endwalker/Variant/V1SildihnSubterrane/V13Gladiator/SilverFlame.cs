namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V13Gladiator;

sealed class SilverFlame(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeRect _shape = new(60f, 4f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var increment = spell.Action.ID switch
        {
            (uint)AID.SilverFlameFirstCW => -10f.Degrees(),
            (uint)AID.SilverFlameFirstCCW => 10f.Degrees(),
            _ => default
        };
        if (increment != default)
        {
            Sequences.Add(new(_shape, spell.LocXZ, spell.Rotation, increment, Module.CastFinishAt(spell), 2d, 5));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SilverFlameFirstCCW or (uint)AID.SilverFlameFirstCW or (uint)AID.SilverFlameRest)
        {
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
        }
    }
}
