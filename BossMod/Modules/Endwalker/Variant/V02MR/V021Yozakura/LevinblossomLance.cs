namespace BossMod.Endwalker.VariantCriterion.V02MR.V021Yozakura;

class LevinblossomLance(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeRect rect = new(30f, 3.5f, 30f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddSequence(Angle increment) => Sequences.Add(new(rect, spell.LocXZ, spell.Rotation, increment, Module.CastFinishAt(spell, 0.8f), 1, 5, 2));
        switch (spell.Action.ID)
        {
            case (uint)AID.LevinblossomLanceCCW:
                AddSequence(28f.Degrees());
                break;
            case (uint)AID.LevinblossomLanceCW:
                AddSequence(-28f.Degrees());
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.LevinblossomLanceFirst or (uint)AID.LevinblossomLanceRest)
            AdvanceSequence(0, WorldState.CurrentTime);
    }
}
