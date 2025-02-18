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
    private static readonly AOEShapeCircle circle = new(10);
    private IEnumerable<Actor> Rods => Module.Enemies(OID.LightningRod).Where(x => x.FindStatus(SID.LightningRod) == null);
    private DateTime activation;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!ActiveBaits.Any(x => x.Target == actor))
            return;
        hints.Add("Pass the lightning to a rod!");
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.LightningRod)
        {
            if (activation == default)
                activation = WorldState.FutureTime(10.8f);
            CurrentBaits.Add(new(actor, actor, circle, activation));
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.LightningRod)
            CurrentBaits.RemoveAll(x => x.Target == actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LightningBolt)
        {
            CurrentBaits.Clear();
            activation = default;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (!ActiveBaits.Any(x => x.Target == actor))
            return;
        var forbidden = new List<Func<WPos, float>>();
        foreach (var a in Rods)
            forbidden.Add(ShapeDistance.InvertedCircle(a.Position, 4));
        if (forbidden.Count != 0)
            hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!ActiveBaits.Any(x => x.Target == pc))
            return;
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var a in Rods)
            Arena.AddCircle(a.Position, 4, Colors.Safe);
    }
}

class Shock(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circleSmall = new(5), circleBig = new(10);
    private readonly List<AOEInstance> _aoes = [];
    private bool first = true;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        var shape = (OID)actor.OID switch
        {
            OID.BallOfLevin => circleSmall,
            OID.SuperchargedLevin => circleBig,
            _ => null
        };
        if (shape != null)
            _aoes.Add(new(shape, actor.Position, default, WorldState.FutureTime(3.7f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ShockSmall or AID.ShockLarge)
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
    private static readonly AOEShapeCone coneWide = new(26, 60.Degrees()), coneNarrow = new(25, 30.Degrees());
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                yield return count == 2 ? aoe with { Color = Colors.Danger } : aoe;
            else if (i == 1)
                yield return aoe;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape, Angle offset = default, float delay = 0)
        => _aoes.Add(new(shape, caster.Position, spell.Rotation + offset, Module.CastFinishAt(spell, delay)));
        if ((AID)spell.Action.ID == AID.WideBlaster)
        {
            AddAOE(coneWide);
            AddAOE(coneNarrow, 180.Degrees(), 2.6f);
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
    private static readonly WPos ArenaCenter = new(-520, 145), LightningRod = new(-538.478f, 137.346f);

    private static List<Shape> GenerateLightningRods()
    {
        const float radius = 1.45f;
        const int edges = 16;
        var polygons = new List<Shape>(8)
        {
            new Polygon(LightningRod, radius, edges)
        };
        for (var i = 1; i < 8; ++i)
        {
            polygons.Add(new Polygon(WPos.RotateAroundOrigin(i * 45, ArenaCenter, LightningRod), radius, edges));
        }
        return polygons;
    }
    private static readonly ArenaBoundsComplex arena = new([new Polygon(ArenaCenter, 19.5f, 48)], [.. GenerateLightningRods(), new Rectangle(new(-540, ArenaCenter.Z), 1.25f, 20),
    new Rectangle(new(-500, ArenaCenter.Z), 1.26f, 20)]);
}
