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

class HydroRing(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(12, 24);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HydroRing)
            _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x13)
        {
            if (state == 0x00020001)
            {
                Arena.Bounds = D031FeatherRay.CircleBounds;
                _aoe = null;
            }
            else if (state == 0x00080004)
                Arena.Bounds = D031FeatherRay.NormalBounds;
        }
    }
}

class AiryBubble(BossModule module) : Components.GenericAOEs(module)
{
    private const float Radius = 1.1f;
    private readonly IReadOnlyList<Actor> _orbs = module.Enemies(OID.AiryBubble);
    private IEnumerable<Actor> Orbs => _orbs.Where(x => x.HitboxRadius == Radius);
    private readonly List<Actor> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _aoes.Select(a =>
        {
            var position = a.Position;
            var directionOffset = 3 * a.Rotation.ToDirection();
            var shapes = new Shape[]
            {
                new Circle(a.Position, Radius),
                new Circle(a.Position + directionOffset, Radius),
                new RectangleSE(a.Position, a.Position + directionOffset, Radius)
            };
            return new AOEInstance(new AOEShapeCustom(shapes), Arena.Center);
        });
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E46 && Orbs.Contains(actor))
            _aoes.Add(actor);
        else if (id == 0x1E3C && Orbs.Contains(actor))
            _aoes.Remove(actor);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoes.Count == 0)
            return;
        var forbidden = new List<Func<WPos, float>>();
        foreach (var o in _aoes)
            forbidden.Add(ShapeDistance.Capsule(o.Position, o.Rotation, 2.5f, Radius));
        forbidden.Add(ShapeDistance.Circle(Arena.Center, Module.PrimaryActor.HitboxRadius));
        hints.AddForbiddenZone(p => forbidden.Select(f => f(p)).Min());
    }
}

class Burst(BossModule module) : Components.GenericAOEs(module)
{
    private readonly IReadOnlyList<Actor> _orbs = module.Enemies(OID.AiryBubble);
    private IEnumerable<Actor> Orbs => _orbs.Where(x => x.HitboxRadius != 1.1f);
    private static readonly AOEShapeCircle circle = new(6);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var activation = Module.CastFinishAt(spell, 3.4f);
        switch ((AID)spell.Action.ID)
        {
            case AID.RollingCurrentWest:
                AddAOEs(8, activation);
                break;
            case AID.RollingCurrentEast:
                AddAOEs(-8, activation);
                break;
        }
    }

    private void AddAOEs(float offset, DateTime activation)
    {
        foreach (var orb in Orbs)
            _aoes.Add(new(circle, orb.Position + new WDir(offset, 0), default, activation));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Burst)
            _aoes.Clear();
    }
}

class Immersion(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Immersion));
class WorrisomeWaveBoss(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WorrisomeWave1), new AOEShapeCone(24, 15.Degrees()));

class WorrisomeWavePlayer(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private static readonly AOEShapeCone cone = new(24, 15.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WorrisomeWave1)
            CurrentBaits.AddRange(Raid.WithoutSlot().Select(p => new Bait(p, p, cone, WorldState.FutureTime(6.3f))));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.WorrisomeWave2)
            CurrentBaits.Clear();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (CurrentBaits.Any(x => x.Source == actor))
            hints.Add("Bait away!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var b in ActiveBaitsOn(actor))
            foreach (var p in Raid.WithoutSlot().Exclude(actor))
                hints.ForbiddenDirections.Add((Angle.FromDirection(p.Position - actor.Position), 15.Degrees(), b.Activation));
    }
}

class D031FeatherRayStates : StateMachineBuilder
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
public class D031FeatherRay(WorldState ws, Actor primary) : BossModule(ws, primary, new(-105, -160), NormalBounds)
{
    public static readonly ArenaBoundsSquare NormalBounds = new(15.5f);
    public static readonly ArenaBoundsCircle CircleBounds = new(12);
}
