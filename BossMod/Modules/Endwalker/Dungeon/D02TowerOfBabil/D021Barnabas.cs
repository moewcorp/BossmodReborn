namespace BossMod.Endwalker.Dungeon.D02TowerOfBabil.D021Barnabas;

public enum OID : uint
{
    Boss = 0x33F7, // R=5.52
    Thunderball = 0x33F8, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    DynamicPoundMinus = 25157, // Boss->self, 7.0s cast, range 40 width 6 rect
    DynamicPoundPlus = 25326, // Boss->self, 7.0s cast, range 40 width 6 rect
    DynamicPoundPull = 24693, // Helper->self, no cast, range 50 width 50 rect, pull 9, between centers
    DynamicPoundKB = 24694, // Helper->self, no cast, range 50 width 50 rect, knockback 9, dir left/right
    DynamicScraplineMinus = 25158, // Boss->self, 7.0s cast, range 8 circle
    DynamicScraplinePlus = 25328, // Boss->self, 7.0s cast, range 8 circle
    DynamicScraplineKB = 25053, // Helper->self, no cast, range 50 circle, pull 5, between centers
    DynamicScraplinePull = 25054, // Helper->self, no cast, range 50 circle, knockback 5, away from source

    ElectromagneticRelease1 = 25327, // Helper->self, 9.5s cast, range 40 width 6 rect
    ElectromagneticRelease2 = 25329, // Helper->self, 9.5s cast, range 8 circle
    GroundAndPound1 = 25159, // Boss->self, 3.5s cast, range 40 width 6 rect
    GroundAndPound2 = 25322, // Boss->self, 3.5s cast, range 40 width 6 rect

    RollingScrapline = 25323, // Boss->self, 3.0s cast, range 8 circle
    Shock = 25330, // Thunderball->self, 3.0s cast, range 8 circle
    ShockingForce = 25324, // Boss->players, 5.0s cast, range 6 circle, stack
    Thundercall = 25325 // Boss->self, 3.0s cast, single-target
}

public enum IconID : uint
{
    Plus = 162, // player
    Minus = 163, // player
    BossMinus = 290, // Boss
    BossPlus = 291, // Boss
    Stackmarker = 62 // player
}

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(15, 19.5f);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x00 && state == 0x00020001)
        {
            Arena.Bounds = D021Barnabas.SmallerBounds;
            _aoe = null;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.GroundAndPound1 or AID.GroundAndPound2 && Arena.Bounds == D021Barnabas.StartingBounds)
            _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell, 6.1f));
    }
}

class Magnetism(BossModule module) : Components.Knockback(module, ignoreImmunes: true)
{
    private readonly ElectromagneticRelease1 _aoe1 = module.FindComponent<ElectromagneticRelease1>()!;
    private readonly ElectromagneticRelease2 _aoe2 = module.FindComponent<ElectromagneticRelease2>()!;
    private enum MagneticPole { None, Plus, Minus }
    private enum Shape { None, Rect, Circle }
    private MagneticPole CurrentPole;
    private Shape CurrentShape;
    private readonly List<(Actor, uint)> iconOnActor = [];
    private DateTime activation;
    private Angle rotation;
    private const int RectDistance = 9;
    private const int CircleDistance = 5;
    private static readonly Angle offset = 90f.Degrees();
    private static readonly AOEShapeCone _shape = new(30f, offset);

    private bool IsKnockback(Actor actor, Shape shape, MagneticPole pole)
        => CurrentShape == shape && CurrentPole == pole && iconOnActor.Contains((actor, (uint)(pole == MagneticPole.Plus ? IconID.Plus : IconID.Minus)));

    private bool IsPull(Actor actor, Shape shape, MagneticPole pole)
        => CurrentShape == shape && CurrentPole == pole && iconOnActor.Contains((actor, (uint)(pole == MagneticPole.Plus ? IconID.Minus : IconID.Plus)));

    public override ReadOnlySpan<Source> ActiveSources(int slot, Actor actor)
    {
        var sources = new List<Source>(2);
        if (IsKnockback(actor, Shape.Rect, MagneticPole.Plus) || IsKnockback(actor, Shape.Rect, MagneticPole.Minus))
        {
            sources.Add(new(Arena.Center, RectDistance, activation, _shape, rotation + offset, Kind.DirForward));
            sources.Add(new(Arena.Center, RectDistance, activation, _shape, rotation - offset, Kind.DirForward));
        }
        else if (IsPull(actor, Shape.Rect, MagneticPole.Plus) || IsPull(actor, Shape.Rect, MagneticPole.Minus))
        {
            sources.Add(new(Arena.Center, RectDistance, activation, _shape, rotation + offset, Kind.DirBackward));
            sources.Add(new(Arena.Center, RectDistance, activation, _shape, rotation - offset, Kind.DirBackward));
        }
        else if (IsKnockback(actor, Shape.Circle, MagneticPole.Plus) || IsKnockback(actor, Shape.Circle, MagneticPole.Minus))
            sources.Add(new(Arena.Center, CircleDistance, activation));
        else if (IsPull(actor, Shape.Circle, MagneticPole.Plus) || IsPull(actor, Shape.Circle, MagneticPole.Minus))
            sources.Add(new(Arena.Center, CircleDistance, activation, default, default, Kind.TowardsOrigin));
        return CollectionsMarshal.AsSpan(sources);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (_aoe1.Casters.Count != 0 && _aoe1.Casters[0].Check(pos))
            return true;
        if (_aoe2.Casters.Count != 0 && _aoe2.Casters[0].Check(pos))
            return true;
        return !Module.InBounds(pos);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is ((uint)IconID.Plus) or ((uint)IconID.Minus))
        {
            iconOnActor.Add((actor, iconID));
            activation = WorldState.FutureTime(8.1d);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DynamicPoundPlus:
            case (uint)AID.DynamicScraplinePlus:
                CurrentPole = MagneticPole.Plus;
                rotation = spell.Rotation;
                break;
            case (uint)AID.DynamicScraplineMinus:
            case (uint)AID.DynamicPoundMinus:
                CurrentPole = MagneticPole.Minus;
                rotation = spell.Rotation;
                break;
            case (uint)AID.ElectromagneticRelease1:
                CurrentShape = Shape.Rect;
                break;
            case (uint)AID.ElectromagneticRelease2:
                CurrentShape = Shape.Circle;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.DynamicPoundKB or (uint)AID.DynamicPoundPull or (uint)AID.DynamicScraplinePull or (uint)AID.DynamicScraplinePull)
        {
            CurrentPole = MagneticPole.None;
            CurrentShape = Shape.None;
            iconOnActor.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        Func<WPos, float>? forbidden = null;
        if (IsKnockback(actor, Shape.Circle, MagneticPole.Plus) || IsKnockback(actor, Shape.Circle, MagneticPole.Minus))
            forbidden = ShapeDistance.InvertedCircle(Arena.Center, 10f);
        else if (IsPull(actor, Shape.Circle, MagneticPole.Plus) || IsPull(actor, Shape.Circle, MagneticPole.Minus))
            forbidden = ShapeDistance.Circle(Arena.Center, 13f);
        else if (IsKnockback(actor, Shape.Rect, MagneticPole.Plus) || IsKnockback(actor, Shape.Rect, MagneticPole.Minus))
            forbidden = ShapeDistance.InvertedCircle(Arena.Center, 6f);
        else if (IsPull(actor, Shape.Rect, MagneticPole.Plus) || IsPull(actor, Shape.Rect, MagneticPole.Minus))
            forbidden = ShapeDistance.Rect(Arena.Center, rotation, 15f, 15f, 12f);
        if (forbidden != null)
            hints.AddForbiddenZone(forbidden, activation);
    }
}

class Cleave(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(40f, 3f));
class ElectromagneticRelease1(BossModule module) : Cleave(module, AID.ElectromagneticRelease1);
class GroundAndPound1(BossModule module) : Cleave(module, AID.GroundAndPound1);
class GroundAndPound2(BossModule module) : Cleave(module, AID.GroundAndPound2);
class DynamicPoundMinus(BossModule module) : Cleave(module, AID.DynamicPoundMinus);
class DynamicPoundPlus(BossModule module) : Cleave(module, AID.DynamicPoundPlus);

class Circles(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 8f);
class ElectromagneticRelease2(BossModule module) : Circles(module, AID.ElectromagneticRelease2);
class DynamicScraplineMinus(BossModule module) : Circles(module, AID.DynamicScraplineMinus);
class DynamicScraplinePlus(BossModule module) : Circles(module, AID.DynamicScraplinePlus);
class RollingScrapline(BossModule module) : Circles(module, AID.RollingScrapline);
class Shock(BossModule module) : Circles(module, AID.Shock);

class ShockingForce(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.ShockingForce), 6f, 4, 4);

class D021BarnabasStates : StateMachineBuilder
{
    public D021BarnabasStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<ElectromagneticRelease1>()
            .ActivateOnEnter<ElectromagneticRelease2>()
            .ActivateOnEnter<Magnetism>()
            .ActivateOnEnter<GroundAndPound1>()
            .ActivateOnEnter<GroundAndPound2>()
            .ActivateOnEnter<DynamicPoundMinus>()
            .ActivateOnEnter<DynamicPoundPlus>()
            .ActivateOnEnter<DynamicScraplineMinus>()
            .ActivateOnEnter<DynamicScraplinePlus>()
            .ActivateOnEnter<RollingScrapline>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<ShockingForce>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 785, NameID = 10279)]
public class D021Barnabas(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300f, 71f), StartingBounds)
{
    public static readonly ArenaBoundsCircle StartingBounds = new(19.5f);
    public static readonly ArenaBoundsCircle SmallerBounds = new(15f);
}
