﻿namespace BossMod.Dawntrail.Dungeon.D03SkydeepCenote.D031FeatherRay;

public enum OID : uint
{
    Boss = 0x41D3, // R5.0
    AiryBubble = 0x41D4, // R1.1-2.2
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Immersion = 36739, // Boss->self, 5.0s cast, range 24 circle
    TroublesomeTail = 36727, // Boss->self, 4.0s cast, range 24 circle 

    WorrisomeWave1 = 36728, // Boss->self, 4.0s cast, range 24 30-degree cone
    WorrisomeWave2 = 36729, // Helper->self, no cast, range 24 30-degree cone

    HydroRing = 36733, // Boss->self, 5.0s cast, range 12-24 donut
    BlowingBubbles = 36732, // Boss->self, 3.0s cast, single-target
    Pop = 36734, // AiryBubble->player, no cast, single-target
    BubbleBomb = 36735, // Boss->self, 3.0s cast, single-target

    RollingCurrentEast = 36737, // Boss->self, 5.0s cast, single-target
    RollingCurrentWest = 36736, // Boss->self, 5.0s cast, single-target

    WaterWave = 38185, // Helper->self, 5.0s cast, range 68 width 32 rect, only affects the bubbles
    Burst = 36738, // AiryBubble->self, 1.5s cast, range 6 circle
    TroubleBubbles = 38787 // Boss->self, 3.0s cast, single-target
}

sealed class HydroRing(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(12f, 24f);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HydroRing)
            _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x13u)
        {
            if (state == 0x00020001u)
            {
                Arena.Bounds = D031FeatherRay.CircleBounds;
                _aoe = null;
            }
            else if (state == 0x00080004u)
                Arena.Bounds = D031FeatherRay.NormalBounds;
        }
    }
}

sealed class AiryBubble(BossModule module) : Components.GenericAOEs(module)
{
    private const float Radius = 1.1f;
    private const float Length = 3f;
    private static readonly AOEShapeCapsule capsule = new(Radius, Length);
    private readonly List<Actor> _aoes = new(36);
    private bool active;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        Span<AOEInstance> aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var o = _aoes[i];
            aoes[i] = new(capsule, o.Position, o.Rotation);
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.TroubleBubbles or (uint)AID.BlowingBubbles)
            active = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.TroubleBubbles or (uint)AID.BlowingBubbles)
            active = false;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.HitboxRadius == Radius)
            if (id == 0x1E46u)
                _aoes.Add(actor);
            else if (id == 0x1E3Cu)
                _aoes.Remove(actor);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _aoes.Count;
        if (active)
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, Module.PrimaryActor.HitboxRadius));
        if (count == 0)
            return;
        var forbiddenImminent = new Func<WPos, float>[count + 1];
        var forbiddenFuture = new Func<WPos, float>[count];
        for (var i = 0; i < count; ++i)
        {
            var o = _aoes[i];
            forbiddenFuture[i] = ShapeDistance.Capsule(o.Position, o.Rotation, Length, Radius);
            forbiddenImminent[i] = ShapeDistance.Circle(o.Position, Radius);
        }
        forbiddenImminent[count] = ShapeDistance.Circle(Arena.Center, Module.PrimaryActor.HitboxRadius);

        hints.AddForbiddenZone(ShapeDistance.Union(forbiddenFuture), WorldState.FutureTime(1.5d));
        hints.AddForbiddenZone(ShapeDistance.Union(forbiddenImminent));
    }
}

sealed class Burst(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(6f);
    private readonly List<AOEInstance> _aoes = new(18);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.RollingCurrentWest:
                AddAOEs(8f);
                break;
            case (uint)AID.RollingCurrentEast:
                AddAOEs(-8f);
                break;
        }
        void AddAOEs(float offset)
        {
            var bubbles = Module.Enemies((uint)OID.AiryBubble);
            var count = bubbles.Count;
            for (var i = 0; i < count; ++i)
            {
                var b = bubbles[i];
                if (b.HitboxRadius != 1.1f)
                    _aoes.Add(new(circle, b.Position + new WDir(offset, default), default, Module.CastFinishAt(spell, 3.4f)));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Burst)
            _aoes.Clear();
    }
}

sealed class Immersion(BossModule module) : Components.RaidwideCast(module, (uint)AID.Immersion);
sealed class WorrisomeWaveBoss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WorrisomeWave1, WorrisomeWavePlayer.Cone);

sealed class WorrisomeWavePlayer(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    public static readonly AOEShapeCone Cone = new(24f, 15f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WorrisomeWave1)
        {
            var party = Raid.WithoutSlot(false, true, true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i];
                CurrentBaits.Add(new(p, p, Cone, WorldState.FutureTime(6.3d)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WorrisomeWave2)
            CurrentBaits.Clear();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (ActiveBaitsOn(actor).Count != 0)
            hints.Add("Bait away!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count == 0)
            return;
        base.AddAIHints(slot, actor, assignment, hints);
        var activeBaits = ActiveBaitsOn(actor);
        foreach (var b in activeBaits)
        {
            var party = Raid.WithoutSlot(false, true, true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i];
                var direction = Angle.FromDirection(p.Position - actor.Position);
                hints.ForbiddenDirections.Add((direction, 15f.Degrees(), b.Activation));
            }
        }
    }
}

sealed class D031FeatherRayStates : StateMachineBuilder
{
    public D031FeatherRayStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AiryBubble>()
            .ActivateOnEnter<Immersion>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<HydroRing>()
            .ActivateOnEnter<WorrisomeWaveBoss>()
            .ActivateOnEnter<WorrisomeWavePlayer>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 829, NameID = 12755)]
public sealed class D031FeatherRay(WorldState ws, Actor primary) : BossModule(ws, primary, arenaCenter, NormalBounds)
{
    private static readonly WPos arenaCenter = new(-105f, -160f);
    public static readonly ArenaBoundsSquare NormalBounds = new(15.5f);
    public static readonly ArenaBoundsComplex CircleBounds = new([new Polygon(arenaCenter, 12f, 48)]);
}
