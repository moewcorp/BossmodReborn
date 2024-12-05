namespace BossMod.Components;

// generic temporary misdirection component
public class TemporaryMisdirection(BossModule module, ActionID aid, string hint = "Applies temporary misdirection") : CastHint(module, aid, hint)
{
    private static readonly uint[] tempMisdirectionSIDs = [1422, 2936, 3694, 3909];
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < 4; ++i)
            if (actor.FindStatus(tempMisdirectionSIDs[i]) != null)
            {
                hints.AddSpecialMode(AIHints.SpecialMode.Misdirection, default);
                break;
            }
    }
}
