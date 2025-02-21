namespace BossMod.Endwalker.VariantCriterion.V02MR.V023Gorai;

class WorldlyPursuit(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCross cross = new(60f, 10f);
    public override uint ImminentColor => Colors.AOE;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddSequence(Angle increment) => Sequences.Add(new(cross, spell.LocXZ, spell.Rotation, increment, Module.CastFinishAt(spell), 3.7f, 5, 1));
        switch (spell.Action.ID)
        {
            case (uint)AID.WorldlyPursuitFirstCCW:
                AddSequence(22.5f.Degrees());
                break;
            case (uint)AID.WorldlyPursuitFirstCW:
                AddSequence(-22.5f.Degrees());
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WorldlyPursuitFirstCCW or (uint)AID.WorldlyPursuitFirstCW or (uint)AID.WorldlyPursuitRest)
            AdvanceSequence(0, WorldState.CurrentTime);
    }
}
