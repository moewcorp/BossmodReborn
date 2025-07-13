namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

sealed class SuspendedStone(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.SuspendedStone, (uint)AID.SuspendedStone, 6f, 5.1d);
sealed class Heavensearth(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Heavensearth, (uint)AID.Heavensearth, 6f, 5.1d, 4, 4)
{
    private BitMask forbidden;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        base.OnEventIcon(actor, iconID, targetID);
        if (iconID == (uint)IconID.SuspendedStone)
        {
            forbidden.Set(Raid.FindSlot(targetID));
        }
    }

    public override void Update()
    {
        base.Update();
        var count = Stacks.Count;
        if (count == 0)
        {
            forbidden = default;
        }
        else
        {
            Stacks.Ref(0).ForbiddenPlayers = forbidden;
        }
    }
}

sealed class FangedCharge(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(40f, 3f);
    private readonly List<AOEInstance> _aoes = new(8);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref readonly var aoe0 = ref aoes[0];
        var deadline = aoe0.Activation.AddSeconds(1d);

        var index = 0;
        while (index < count)
        {
            ref readonly var aoe = ref aoes[index];
            if (aoe.Activation >= deadline)
            {
                break;
            }
            ++index;
        }

        return aoes[..index];
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.GleamingFang2 && id == 0x11D2u)
        {
            AddAOE();
            AddAOE(180f.Degrees());
        }
        void AddAOE(Angle offset = default)
        => _aoes.Add(new(rect, WPos.ClampToGrid(actor.Position), actor.Rotation + offset, WorldState.FutureTime(6d)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FangedCharge)
        {
            var count = _aoes.Count - 1;
            var pos = caster.Position;
            for (var i = count; i >= 0; --i)
            {
                if (_aoes[i].Origin.AlmostEqual(pos, 1f))
                {
                    _aoes.RemoveAt(i);
                }
            }
            _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.FangedCharge)
        {
            _aoes.RemoveAt(0);
            ++NumCasts;
        }
    }
}

sealed class Shadowchase(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(40f, 4f);
    private readonly List<AOEInstance> _aoes = new(5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.HowlingBladeShadow && id == 0x11D1u)
        {
            _aoes.Add(new(rect, WPos.ClampToGrid(actor.Position), actor.Rotation, WorldState.FutureTime(3.1d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Shadowchase)
        {
            ++NumCasts;
        }
    }
}
