namespace BossMod.Dawntrail.Savage.M04SWickedThunder;

class Electray(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeRect _shape = new(40f, 2.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.GunBattery)
            AOEs.Add(new(_shape, actor.Position, actor.Rotation, WorldState.FutureTime(6.8d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Electray)
        {
            ++NumCasts;
            AOEs.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1f));
        }
    }
}
