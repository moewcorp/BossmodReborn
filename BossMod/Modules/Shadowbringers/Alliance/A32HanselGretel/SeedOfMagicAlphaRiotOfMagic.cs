namespace BossMod.Shadowbringers.Alliance.A32HanselGretel;

sealed class RiotOfMagicSeedOfMagicAlpha(BossModule module) : Components.CastStackSpread(module, (uint)AID.RiotOfMagic, (uint)AID.SeedOfMagicAlpha, 5f, 5f, 18, 18, true)
{
    private BitMask forbidden;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == SpreadAction)
        {
            forbidden.Set(Raid.FindSlot(spell.TargetID));
            if (Stacks.Count != 0)
            {
                Stacks.Ref(0).ForbiddenPlayers = forbidden;
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action.ID == SpreadAction)
        {
            forbidden.Clear(Raid.FindSlot(spell.TargetID));
            if (Stacks.Count != 0)
            {
                Stacks.Ref(0).ForbiddenPlayers = forbidden;
            }
        }
    }
}
