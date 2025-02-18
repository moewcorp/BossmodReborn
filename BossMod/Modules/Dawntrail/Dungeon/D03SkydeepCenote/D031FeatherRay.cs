namespace BossMod.Dawntrail.Dungeon.D03SkydeepCenote.D031FeatherRay;

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
    private static readonly AOEShapeDonut donut = new(12f, 24f);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HydroRing)
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
    private const float Length = 3f;
    private static readonly AOEShapeCapsule capsule = new(Radius, Length);
    private readonly List<Actor> bubbles = module.Enemies(OID.AiryBubble);
    private readonly List<Actor> _aoes = new(36);
    private bool active;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
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
        if (bubbles.Any(x => x.HitboxRadius == 1.1f && x == actor))
            if (id == 0x1E46)
                _aoes.Add(actor);
            else if (id == 0x1E3C)
                _aoes.Remove(actor);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _aoes.Count;
        if (active)
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, Module.PrimaryActor.HitboxRadius));
        if (count == 0)
            return;
        var forbidden = new Func<WPos, float>[count + 1];
        for (var i = 0; i < count; ++i)
        {
            var o = _aoes[i];
            forbidden[i] = ShapeDistance.Capsule(o.Position, o.Rotation, Length, Radius);
        }
        forbidden[count] = ShapeDistance.Circle(Arena.Center, Module.PrimaryActor.HitboxRadius);

        hints.AddForbiddenZone(ShapeDistance.Union(forbidden), WorldState.FutureTime(1.1d));
    }
}

class Burst(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(6f);
    private readonly List<Actor> bubbles = module.Enemies(OID.AiryBubble);
    private readonly List<AOEInstance> _aoes = new(18);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var activation = Module.CastFinishAt(spell, 3.4f);
        switch (spell.Action.ID)
        {
            case (uint)AID.RollingCurrentWest:
                AddAOEs(8f, activation);
                break;
            case (uint)AID.RollingCurrentEast:
                AddAOEs(-8f, activation);
                break;
        }
    }

    private void AddAOEs(float offset, DateTime activation)
    {
        foreach (var orb in bubbles.Where(x => x.HitboxRadius != 1.1f))
            _aoes.Add(new(circle, orb.Position + new WDir(offset, 0f), default, activation));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Burst)
            _aoes.Clear();
    }
}

class Immersion(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Immersion));
class WorrisomeWaveBoss(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WorrisomeWave1), WorrisomeWavePlayer.Cone);

class WorrisomeWavePlayer(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
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
                var p = party[i];
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
        if (CurrentBaits.Any(x => x.Source == actor))
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
                var p = party[i];
                var direction = Angle.FromDirection(p.Position - actor.Position);
                hints.ForbiddenDirections.Add((direction, 15f.Degrees(), b.Activation));
            }
        }
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
public class D031FeatherRay(WorldState ws, Actor primary) : BossModule(ws, primary, arenaCenter, NormalBounds)
{
    private static readonly WPos arenaCenter = new(-105f, -160f);
    public static readonly ArenaBoundsSquare NormalBounds = new(15.5f);
    public static readonly ArenaBoundsComplex CircleBounds = new([new Polygon(arenaCenter, 12f, 48)]);
}
