namespace BossMod.Shadowbringers.Dungeon.D13Paglthan.D131Amhuluk;

public enum OID : uint
{
    Boss = 0x3169, // R7.008, x1
    LightningRod = 0x31B6, // R1.0
    BallOfLevin = 0x31A2, // R1.3
    SuperchargedLevin = 0x31A3, // R2.3
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Teleport = 23633, // Boss->location, no cast, single-target

    CriticalRip = 23630, // Boss->player, 5.0s cast, single-target
    LightningBoltVisual = 23627, // Boss->self, 10.0s cast, single-target
    LightningBolt = 23628, // Helper->location, no cast, range 10 circle
    ElectricBurst = 23629, // Boss->self, 4.5s cast, range 50 width 40 rect, raidwide (rect also goes to the back of boss)
    Thundercall = 23632, // Boss->self, 4.0s cast, single-target
    ShockSmall = 23634, // BallOfLevin->self, no cast, range 5 circle
    ShockLarge = 23635, // SuperchargedLevin->self, no cast, range 10 circle
    WideBlaster = 24773, // Boss->self, 4.0s cast, range 26 120-degree cone
    SpikeFlail = 23631 // Boss->self, 1.0s cast, range 25 60-degree cone
}

public enum SID : uint
{
    LightningRod = 2574 // none->player/LightningRod, extra=0x114
}

class ElectricBurst(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ElectricBurst));
class CriticalRip(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.CriticalRip));

class LightningBolt(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.LightningBolt), centerAtTarget: true)
{
    private static readonly AOEShapeCircle circle = new(10f);
    private DateTime activation;

    public static List<Actor> GetRods(BossModule module)
    {
        var rods = module.Enemies((uint)OID.LightningRod);
        var count = rods.Count;
        if (count == 0)
            return [];

        var filteredrods = new List<Actor>(count);
        for (var i = 0; i < count; ++i)
        {
            var z = rods[i];
            if (z.FindStatus(SID.LightningRod) == null)
                filteredrods.Add(z);
        }
        return filteredrods;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveBaitsOn(actor).Count == 0)
            return;
        hints.Add("Pass the lightning to a rod!");
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.LightningRod)
        {
            if (activation == default)
                activation = WorldState.FutureTime(10.8f);
            CurrentBaits.Add(new(actor, actor, circle, activation));
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.LightningRod)
        {
            var count = CurrentBaits.Count;
            for (var i = 0; i < count; ++i)
            {
                if (CurrentBaits[i].Target == actor)
                {
                    CurrentBaits.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LightningBolt)
        {
            CurrentBaits.Clear();
            activation = default;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (ActiveBaitsOn(actor).Count == 0)
            return;
        var rods = GetRods(Module);
        var count = rods.Count;
        var forbidden = new Func<WPos, float>[count];
        for (var i = 0; i < count; ++i)
            forbidden[i] = ShapeDistance.InvertedCircle(rods[i].Position, 4f);
        if (count != 0)
            hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (ActiveBaitsOn(pc).Count == 0)
            return;
        base.DrawArenaForeground(pcSlot, pc);
        var rods = GetRods(Module);
        var count = rods.Count;
        for (var i = 0; i < count; ++i)
            Arena.AddCircle(rods[i].Position, 4f, Colors.Safe);
    }
}

class Shock(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circleSmall = new(5f), circleBig = new(10f);
    private readonly List<AOEInstance> _aoes = [];
    private bool first = true;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        var shape = actor.OID switch
        {
            (uint)OID.BallOfLevin => circleSmall,
            (uint)OID.SuperchargedLevin => circleBig,
            _ => null
        };
        if (shape != null)
            _aoes.Add(new(shape, WPos.ClampToGrid(actor.Position), default, WorldState.FutureTime(3.7d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ShockSmall or (uint)AID.ShockLarge)
        {
            if (++NumCasts == (first ? 72 : 30))
            {
                _aoes.Clear();
                NumCasts = 0;
                first = false;
            }
        }
    }
}

class WideBlasterSpikeFlail(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone coneWide = new(26f, 60f.Degrees()), coneNarrow = new(25f, 30f.Degrees());
    private readonly List<AOEInstance> _aoes = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe with { Risky = false };
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape, Angle offset = default, float delay = 0)
        => _aoes.Add(new(shape, caster.Position, spell.Rotation + offset, Module.CastFinishAt(spell, delay)));
        if (spell.Action.ID == (uint)AID.WideBlaster)
        {
            AddAOE(coneWide);
            AddAOE(coneNarrow, 180f.Degrees(), 2.6f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && (AID)spell.Action.ID is AID.WideBlaster or AID.SpikeFlail)
            _aoes.RemoveAt(0);
    }
}

class D131AmhulukStates : StateMachineBuilder
{
    public D131AmhulukStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectricBurst>()
            .ActivateOnEnter<CriticalRip>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<WideBlasterSpikeFlail>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 777, NameID = 10075)]
public class D131Amhuluk(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos ArenaCenter = new(-520f, 145f), LightningRod = new(-538.478f, 137.346f);

    private static Polygon[] GenerateLightningRods()
    {
        const float radius = 1.45f;
        const int edges = 16;
        var polygons = new Polygon[8];
        polygons[0] = new(LightningRod, radius, edges);
        for (var i = 1; i < 8; ++i)
        {
            polygons[i] = new Polygon(WPos.RotateAroundOrigin(i * 45f, ArenaCenter, LightningRod), radius, edges);
        }
        return polygons;
    }
    private static readonly ArenaBoundsComplex arena = new([new Polygon(ArenaCenter, 19.5f, 48)], [.. GenerateLightningRods(), new Rectangle(new(-540, ArenaCenter.Z), 1.25f, 20),
    new Rectangle(new(-500, ArenaCenter.Z), 1.26f, 20)]);
}
