namespace BossMod.Components;

// generic temporary misdirection component
public class TemporaryMisdirection(BossModule module, ActionID aid, string hint = "Applies temporary misdirection") : CastHint(module, aid, hint)
{
    public BitMask Mask;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID is 1422 or 2936 or 3694 or 3909)
            Mask.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID is 1422 or 2936 or 3694 or 3909)
            Mask[Raid.FindSlot(actor.InstanceID)] = default;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Mask[slot] != default)
            hints.AddSpecialMode(AIHints.SpecialMode.Misdirection, default);
    }
}
