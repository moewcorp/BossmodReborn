namespace BossMod.Dawntrail.Ultimate.FRU;

abstract class P2Banish(BossModule module) : Components.UniformStackSpread(module, 5, 5, 2, 2, true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BanishStack:
                // TODO: this can target either supports or dd
                AddStacks(Raid.WithoutSlot(true, true, true).Where(p => p.Class.IsSupport()), Module.CastFinishAt(spell, 0.1f));
                break;
            case AID.BanishSpread:
                AddSpreads(Raid.WithoutSlot(true, true, true), Module.CastFinishAt(spell, 0.1f));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BanishStackAOE:
                Stacks.Clear();
                break;
            case AID.BanishSpreadAOE:
                Spreads.Clear();
                break;
        }
    }
}
