namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

class AbsoluteAuthorityPuddles(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AbsoluteAuthorityPuddlesAOE), 8);

class AbsoluteAuthorityExpansionBoot(BossModule module) : Components.UniformStackSpread(module, 6, 15, 4, alwaysShowSpreads: true) // TODO: verify falloff
{
    public int NumCasts;
    private readonly Ex3QueenEternalConfig _config = Service.Config.Get<Ex3QueenEternalConfig>();

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.AuthoritysExpansion:
                if (!_config.AbsoluteAuthorityIgnoreFlares)
                    AddSpread(actor, status.ExpireAt);
                break;
            case SID.AuthoritysBoot:
                AddStack(actor, status.ExpireAt);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AbsoluteAuthorityExpansion or AID.AbsoluteAuthorityBoot)
        {
            ++NumCasts;
            Spreads.Clear();
            Stacks.Clear();
        }
    }
}

class AbsoluteAuthorityHeel(BossModule module) : Components.GenericStackSpread(module)
{
    public int NumCasts;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.AuthoritysHeel && Stacks.Count == 0)
            Stacks.Add(new(actor, 1.5f, 8, 8, activation: WorldState.FutureTime(5.1f)));
    }

    public override void Update()
    {
        if (Stacks.Count != 0)
        {
            var player = Raid.Player()!;
            var actor = Raid.WithoutSlot(false, true, true).Exclude(player).OrderBy(a => (player.Position - a.Position).LengthSq()).First();
            Stacks[0] = Stacks[0] with { Target = actor ?? player };
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AbsoluteAuthorityHeel or AID.AbsoluteAuthorityHeelFail)
        {
            Stacks.Clear();
            ++NumCasts;
        }
    }
}

class AbsoluteAuthorityKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.AbsoluteAuthorityKnockback), 30, kind: Kind.DirForward);
