namespace BossMod.Shadowbringers.Alliance.A33RedGirl;

sealed class PointBlackWhite(BossModule module) : Components.GenericAOEs(module)
{
    private readonly ArenaChanges _arena = module.FindComponent<ArenaChanges>()!;
    private readonly List<AOEInstance> _aoes = new(8);
    private static readonly AOEShapeRect rectShort = new(24f, 3f), rectLong = new(50f, 3f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (Module.StateMachine.ActivePhaseIndex != 0)
        {
            return; // in phase 2 we can't use this
        }
        var id = actor.OID;
        if (id == (uint)OID.WhiteLance)
        {
            AddAOE(actor, _arena.WhiteWalls);
        }
        else if (id == (uint)OID.BlackLance)
        {
            AddAOE(actor, _arena.BlackWalls);
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (Module.StateMachine.ActivePhaseIndex != 2)
        {
            return; // in phase 1 we can't use this
        }
        var oid = actor.OID;
        if (oid == (uint)OID.WhiteLance && id == 0x11D1u)
        {
            AddAOE(actor, _arena.WhiteWalls);
        }
        else if (oid == (uint)OID.BlackLance && id == 0x11D2u)
        {
            AddAOE(actor, _arena.BlackWalls);
        }
    }

    private void AddAOE(Actor actor, RelSimplifiedComplexPolygon wallPolygon)
    {
        var pos = actor.Position;
        _aoes.Add(new(!wallPolygon.Contains(pos + 25f * actor.Rotation.ToDirection() - Arena.Center) ? rectShort : rectLong, WPos.ClampToGrid(pos), actor.Rotation, WorldState.FutureTime(7.8d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.PointBlack1 or (uint)AID.PointBlack2 or (uint)AID.PointWhite1 or (uint)AID.PointWhite2)
        {
            _aoes.Clear();
        }
    }
}
