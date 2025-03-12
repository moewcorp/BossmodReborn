namespace BossMod.Dawntrail.Savage.M04SWickedThunder;

class WickedFire(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WickedFireAOE), 10f);

class TwilightSabbath(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeCone _shape = new(60f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(AOEs)[..max];
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID != (uint)OID.WickedReplica)
            return;
        var (offset, delay) = id switch
        {
            0x11D6 => (-90f.Degrees(), 7.1d),
            0x11D7 => (-90f.Degrees(), 15.2d),
            0x11D8 => (90f.Degrees(), 7.1d),
            0x11D9 => (90f.Degrees(), 15.2d),
            _ => default
        };
        if (offset != default)
        {
            AOEs.Add(new(_shape, WPos.ClampToGrid(actor.Position), actor.Rotation + offset, WorldState.FutureTime(delay)));
            AOEs.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TwilightSabbathSidewiseSparkR or (uint)AID.TwilightSabbathSidewiseSparkL)
        {
            ++NumCasts;
            if (AOEs.Count != 0)
                AOEs.RemoveAt(0);
        }
    }
}
