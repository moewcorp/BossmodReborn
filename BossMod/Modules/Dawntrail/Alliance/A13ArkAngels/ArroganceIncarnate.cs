namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

class ArroganceIncarnate(BossModule module) : Components.StackWithIcon(module, (uint)IconID.ArroganceIncarnate, ActionID.MakeSpell(AID.ArroganceIncarnateAOE), 6f, 5.8f, PartyState.MaxAllianceSize, PartyState.MaxAllianceSize)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ArroganceIncarnate)
            NumFinishedStacks = 0;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == StackAction)
        {
            if (++NumFinishedStacks >= 5)
            {
                Stacks.Clear();
            }
        }
    }
}
