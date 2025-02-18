namespace BossMod.Dawntrail.Dungeon.D07TenderValley.D071Barreltender;

public enum OID : uint
{
    Boss = 0x4234, // R5.0
    CactusBig = 0x1EBBF1, // R0.5
    CactusSmall = 0x1EBBF0, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 37393, // Boss->location, no cast, single-target

    BarbedBellow = 37392, // Boss->self, 5.0s cast, range 50 circle, raidwide
    HeavyweightNeedlesVisual = 37384, // Boss->self, 6.0s cast, single-target
    HeavyweightNeedles = 37386, // Helper->self, 6.5s cast, range 36 50-degree cone

    TenderDrop = 37387, // Boss->self, 3.0s cast, single-target, spawns cacti
    BarrelBreaker = 37390, // Boss->location, 6.0s cast, range 50 circle, knockback 20, away from source

    NeedleSuperstorm = 37389, // Helper->self, 5.0s cast, range 11 circle
    NeedleStorm = 37388, // Helper->self, 5.0s cast, range 6 circle

    SucculentStomp = 37391, // Boss->players, 5.0s cast, range 6 circle, stack
    PricklyRight = 39154, // Boss->self, 7.0s cast, range 36 270-degree cone
    PricklyLeft = 39155, // Boss->self, 7.0s cast, range 36 270-degree cone

    TenderFury = 39242 // Boss->player, 5.0s cast, single-target, tankbuster
}

class HeavyweightNeedlesArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(D071Barreltender.ArenaCenter, 25)], [new Square(D071Barreltender.ArenaCenter, 20)]);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HeavyweightNeedlesVisual && Arena.Bounds == D071Barreltender.StartingBounds)
            _aoe = new(square, Arena.Center, default, Module.CastFinishAt(spell, 0.7f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x03)
        {
            Arena.Bounds = D071Barreltender.DefaultBounds;
            _aoe = null;
        }
    }
}

class NeedleStormSuperstormHeavyWeightNeedles(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoesCircles = new(16);
    private readonly List<AOEInstance> _aoesCones = new(8);
    private static readonly AOEShapeCircle circleBig = new(11f);
    private static readonly AOEShapeCircle circleSmall = new(6f);
    private static readonly AOEShapeCone cone = new(36f, 25f.Degrees());
    private readonly BarrelBreaker _kb = module.FindComponent<BarrelBreaker>()!;
    private bool cactiActive;
    private bool cactiActive2;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var countCircles = _aoesCircles.Count;
        var countCones = _aoesCones.Count;
        var adjCircleCount = cactiActive ? (countCircles > 8 ? 8 : countCircles) : 0; // next wave of cactus helpers spawns immediately after first wave starts casting, so we need to limit to 8
        var total = adjCircleCount + countCones;
        if (total == 0)
            return [];
        var aoes = new AOEInstance[total];
        if (cactiActive)
        {
            var kb = _kb;
            var isKnockback = kb.Sources(slot, actor).Any();
            var isStillSafe = kb.Activation != default && kb.Activation > WorldState.CurrentTime;
            var isKnockbackImmune = isKnockback && _kb.IsImmune(slot, kb.Sources(slot, actor).First().Activation);
            var isKnockbackButImmune = isKnockback && isKnockbackImmune;
            var areConesActive = countCircles > 0;
            for (var i = 0; i < adjCircleCount; ++i)
                aoes[i] = _aoesCircles[i] with { Risky = cactiActive2 && !areConesActive && (isKnockbackButImmune || !isStillSafe) };
        }
        if (countCones != 0)
            for (var i = 0; i < countCones; ++i)
                aoes[adjCircleCount + i] = _aoesCones[i] with { Color = cactiActive ? Colors.Danger : default };
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.CactusSmall)
            _aoesCircles.Add(new(circleSmall, actor.Position));
        else if (actor.OID == (uint)OID.CactusBig)
            _aoesCircles.Add(new(circleBig, actor.Position));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TenderDrop)
        {
            cactiActive = true;
            var count = _aoesCircles.Count;
            for (var i = 0; i < count; ++i)
                _aoesCircles[i] = _aoesCircles[i] with { Activation = Module.CastFinishAt(spell, 13.7f) };
        }
        else if (spell.Action.ID == (uint)AID.HeavyweightNeedles)
        {
            _aoesCones.Add(new(cone, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            if (cactiActive)
                cactiActive2 = true;
        }
        else if (spell.Action.ID == (uint)AID.BarrelBreaker)
            cactiActive2 = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoesCircles.Count != 0 && spell.Action.ID is (uint)AID.NeedleStorm or (uint)AID.NeedleSuperstorm)
        {
            _aoesCircles.RemoveAt(0);
            cactiActive = false;
            cactiActive2 = false;
        }
        else if (spell.Action.ID == (uint)AID.HeavyweightNeedles)
            _aoesCones.Clear();
    }
}

abstract class Prickly(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(36f, 165f.Degrees()));
class PricklyRight(BossModule module) : Prickly(module, AID.PricklyRight);
class PricklyLeft(BossModule module) : Prickly(module, AID.PricklyLeft);

class SucculentStomp(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.SucculentStomp), 6f, 4, 4);
class BarrelBreaker(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.BarrelBreaker), 20f)
{
    private static readonly Angle a5 = 5f.Degrees(), a135 = 135f.Degrees(), a45 = 45f.Degrees();
    public DateTime Activation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action == WatchedAction)
            Activation = Module.CastFinishAt(spell, 1f);
    }

    public override void Update()
    {
        if (Activation != default && Activation < WorldState.CurrentTime)
            Activation = default;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var source = Sources(slot, actor).FirstOrDefault();
        if (source != default)
        {
            var forbidden = new Func<WPos, float>[2];
            var cactusSmall = Module.Enemies(OID.CactusSmall).FirstOrDefault(x => x.Position == new WPos(-55f, 455f));
            forbidden[0] = ShapeDistance.InvertedDonutSector(source.Origin, 4, 5, cactusSmall != default ? a135 : -a135, a5);
            forbidden[1] = ShapeDistance.InvertedDonutSector(source.Origin, 4, 5, cactusSmall != default ? -a45 : a45, a5);
            hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), source.Activation);
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<NeedleStormSuperstormHeavyWeightNeedles>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.InBounds(pos);
}

class TenderFury(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.TenderFury));
class BarbedBellow(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BarbedBellow));

class D071BarreltenderStates : StateMachineBuilder
{
    public D071BarreltenderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BarrelBreaker>()
            .ActivateOnEnter<HeavyweightNeedlesArenaChange>()
            .ActivateOnEnter<NeedleStormSuperstormHeavyWeightNeedles>()
            .ActivateOnEnter<PricklyRight>()
            .ActivateOnEnter<PricklyLeft>()
            .ActivateOnEnter<TenderFury>()
            .ActivateOnEnter<SucculentStomp>()
            .ActivateOnEnter<BarbedBellow>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 834, NameID = 12889)]
public class D071Barreltender(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(-65f, 470f);
    public static readonly ArenaBoundsSquare StartingBounds = new(24.5f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20f);
}
