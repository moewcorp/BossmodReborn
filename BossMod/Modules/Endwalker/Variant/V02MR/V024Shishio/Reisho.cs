namespace BossMod.Endwalker.VariantCriterion.V02MR.V024Shishio;

class Reisho1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ReishoFirst), 6);
class Reisho2(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID.HauntingThrall), 10)
{
    private bool started;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ReishoFirst)
            started = true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (started && NumCasts != 20)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (started && NumCasts != 20)
            base.DrawArenaBackground(pcSlot, pc);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ReishoRest)
            ++NumCasts;
        else if ((AID)spell.Action.ID == AID.ThunderVortex) // not sure if mechanic can repeat if fight takes long enough
        {
            started = false;
            NumCasts = 0;
        }
    }
}
