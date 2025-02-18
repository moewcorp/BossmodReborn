namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

class DominionSlash(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeCircle _shape = new(6f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.DominionSlashHelper && state == 0x00010002)
            AOEs.Add(new(_shape, actor.Position, default, WorldState.FutureTime(6.5d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.DivineDominion or (uint)AID.DivineDominionFail)
        {
            ++NumCasts;
            AOEs.RemoveAll(aoe => aoe.Origin == caster.Position);
        }
    }
}
