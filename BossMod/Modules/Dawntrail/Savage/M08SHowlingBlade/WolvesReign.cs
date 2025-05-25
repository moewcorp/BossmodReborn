namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

sealed class WolvesReignConeCircle(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(40f, 60f.Degrees());
    private static readonly AOEShapeCircle circle = new(14f);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.WolvesReignTeleport1 => cone,
            (uint)AID.WolvesReignTeleport2 => circle,
            _ => null
        };
        if (shape != null)
        {
            var pos = spell.LocXZ;
            _aoe = new(shape, pos, Angle.FromDirection(Arena.Center - pos), Module.CastFinishAt(spell, 3.6f), Colors.Danger);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.WolvesReignCone:
            case (uint)AID.WolvesReignCircleBig:
                ++NumCasts;
                break;
        }
    }
}

sealed class WolvesReignRect(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.WolvesReignRect1, (uint)AID.WolvesReignRect2], new AOEShapeRect(28f, 5f));
sealed class WolvesReignCircle(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.WolvesReignCircle1, (uint)AID.WolvesReignCircle2,
(uint)AID.WolvesReignCircle3, (uint)AID.EminentReign, (uint)AID.RevolutionaryReign], 6f);

sealed class SovereignScar(BossModule module) : Components.GenericBaitStack(module)
{
    private static readonly AOEShapeCone cone = new(40f, 15f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WolvesReignTeleport1 or (uint)AID.WolvesReignTeleport2)
        {
            var act = Module.CastFinishAt(spell);
            var party = Raid.WithSlot(true, true, true);
            var source = new Actor(default, default, default, default!, default, default, default, default, spell.LocXZ.ToVec4());
            var len = party.Length;
            BitMask forbidden = new();
            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i];
                if (p.Item2.Role == Role.Tank)
                    forbidden[p.Item1] = true;
            }

            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i].Item2;
                if (p.Role == Role.Healer)
                    CurrentBaits.Add(new(source, p, cone, act, forbidden));
            }
        }
    }
}

sealed class ReignsEnd(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeCone cone = new(40f, 30f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WolvesReignTeleport1 or (uint)AID.WolvesReignTeleport2)
        {
            var act = Module.CastFinishAt(spell);
            var party = Raid.WithoutSlot(true, true, true);
            var source = new Actor(default, default, default, default!, default, default, default, default, spell.LocXZ.ToVec4());
            var len = party.Length;

            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i];
                if (p.Role == Role.Tank)
                    CurrentBaits.Add(new(source, p, cone, act));
            }
        }
    }
}

sealed class RoaringWind(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(40f, 4f);
    private readonly List<AOEInstance> _aoes = new(4);
    public bool Draw;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Draw ? CollectionsMarshal.AsSpan(_aoes) : [];

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.WolfOfWind4 && id == 0x11D2u)
            _aoes.Add(new(rect, WPos.ClampToGrid(actor.Position), actor.Rotation, WorldState.FutureTime(5.6d)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RoaringWind)
        {
            ++NumCasts;
        }
    }
}

sealed class WealOfStone(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeRect rect = new(40f, 3f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WealOfStone1 or (uint)AID.WealOfStone2)
            _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WealOfStone1 or (uint)AID.WealOfStone2)
            ++NumCasts;
    }
}
