namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

sealed class Beacons(BossModule module) : BossComponent(module)
{
    public readonly List<Actor> Actors = [];
    public IEnumerable<Actor> ActiveActors => Actors.Where(a => a.IsTargetable && !a.IsDead);

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.ThunderousBeacon or OID.FlameKissedBeacon or OID.GlacialBeacon)
            Actors.Add(actor);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Actors);
    }
}

sealed class CalamitousCry : Components.GenericWildCharge
{
    public CalamitousCry(BossModule module) : base(module, 3, (uint)AID.CalamitousCryAOE, 80)
    {
        Reset();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CalamitousCryTargetFirst:
            case AID.CalamitousCryTargetRest:
                Source = Module.PrimaryActor;
                var slot = Raid.FindSlot(spell.MainTargetID);
                if (slot >= 0)
                    PlayerRoles[slot] = PlayerRole.Target;
                break;
            case AID.CalamitousCryAOE:
                ++NumCasts;
                Reset();
                break;
        }
    }

    private void Reset()
    {
        Source = null;
        foreach (var (i, p) in Module.Raid.WithSlot(true, true, true))
            PlayerRoles[i] = p.Role == Role.Tank ? PlayerRole.Share : PlayerRole.ShareNotFirst;
    }
}

sealed class CalamitousEcho(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CalamitousEcho, new AOEShapeCone(40, 10.Degrees()));
