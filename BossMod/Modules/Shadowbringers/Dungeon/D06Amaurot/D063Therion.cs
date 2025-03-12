namespace BossMod.Shadowbringers.Dungeon.D06Amaurot.D063Therion;

public enum OID : uint
{
    Boss = 0x27C1, // R=25.84
    TheFaceOfTheBeast = 0x27C3, // R=2.1
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 15574, // Boss->player, no cast, single-target
    ShadowWreck = 15587, // Boss->self, 4.0s cast, range 100 circle
    ApokalypsisFirst = 15575, // Boss->self, 6.0s cast, range 76 width 20 rect
    ApokalypsisRest = 15577, // Helper->self, no cast, range 76 width 20 rect
    TherionCharge = 15578, // Boss->location, 7.0s cast, range 100 circle, damage fall off AOE

    DeathlyRayVisualFaces1 = 15579, // Boss->self, 3.0s cast, single-target
    DeathlyRayVisualFaces2 = 16786, // Boss->self, no cast, single-target
    DeathlyRayVisualThereion1 = 17107, // Helper->self, 5.0s cast, range 80 width 6 rect
    DeathlyRayVisualThereion2 = 15582, // Boss->self, 3.0s cast, single-target
    DeathlyRayVisualThereion3 = 16785, // Boss->self, no cast, single-target

    DeathlyRayFacesFirst = 15580, // TheFaceOfTheBeast->self, no cast, range 60 width 6 rect
    DeathlyRayFacesRest = 15581, // Helper->self, no cast, range 60 width 6 rect
    DeathlyRayThereionFirst = 15583, // Helper->self, no cast, range 60 width 6 rect
    DeathlyRayThereionRest = 15585, // Helper->self, no cast, range 60 width 6 rect
    Misfortune = 15586, // Helper->location, 3.0s cast, range 6 circle
}

class ShadowWreck(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ShadowWreck));
class Misfortune(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Misfortune), 6f);
class ThereionCharge(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TherionCharge), 20f);

class Border(BossModule module) : Components.GenericAOEs(module, warningText: "Platform will be removed during next Apokalypsis!")
{
    private const float SquareHalfWidth = 2f;
    private const float RectangleHalfWidth = 10.1f;
    private const float MaxError = 5f;
    private static readonly AOEShapeRect _square = new(2f, 2f, 2f);

    public readonly List<AOEInstance> BreakingPlatforms = new(2);

    public static readonly WPos[] positions = [new(-12f, -71f), new(12f, -71f), new(-12f, -51f),
    new(12f, -51f), new(-12f, -31f), new(12f, -31f), new(-12f, -17f), new(12f, -17f), new(default, -65f), new(default, -45f)];

    private static readonly Square[] shapes = [new(positions[0], SquareHalfWidth), new(positions[1], SquareHalfWidth), new(positions[2], SquareHalfWidth),
    new(positions[3], SquareHalfWidth), new(positions[4], SquareHalfWidth), new(positions[5], SquareHalfWidth), new(positions[6], SquareHalfWidth),
    new(positions[7], SquareHalfWidth), new(positions[8], RectangleHalfWidth), new(positions[9], RectangleHalfWidth)];

    private static readonly Rectangle[] rect = [new(new(default, -45f), 10f, 30f)];
    public readonly List<Shape> UnionRefresh = Union();
    private readonly List<Shape> difference = new(8);
    public static readonly ArenaBoundsComplex DefaultArena = new([.. Union()]);

    private static List<Shape> Union()
    {
        var union = new List<Shape>(rect);
        for (var i = 0; i < 8; ++i)
            union.Add(shapes[i]);
        return union;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = BreakingPlatforms.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var p = BreakingPlatforms[i];
            aoes[i] = p with { Risky = Module.FindComponent<Apokalypsis>()?.NumCasts == 0 };
        }
        return aoes;
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008)
        {
            for (var i = 0; i < 8; ++i)
            {
                if (actor.Position.AlmostEqual(positions[i], MaxError))
                {
                    if (UnionRefresh.Remove(shapes[i]))
                    {
                        if (UnionRefresh.Count == 7)
                            difference.Add(shapes[8]);
                        else if (UnionRefresh.Count == 5)
                            difference.Add(shapes[9]);
                        ArenaBoundsComplex arena = new([.. UnionRefresh], [.. difference]);
                        Arena.Bounds = arena;
                        Arena.Center = arena.Center;
                    }
                    BreakingPlatforms.Remove(new(_square, positions[i], Color: Colors.FutureVulnerable));
                }
            }
        }
        else if (state == 0x00100020)
        {
            for (var i = 0; i < 8; ++i)
            {
                if (actor.Position.AlmostEqual(positions[i], MaxError))
                    BreakingPlatforms.Add(new(_square, positions[i], Color: Colors.FutureVulnerable));
            }
        }
    }
}

class Apokalypsis(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeRect _rect = new(76f, 10f);
    private readonly Border _arena = module.FindComponent<Border>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe is AOEInstance aoe && (_arena.UnionRefresh.Count - _arena.BreakingPlatforms.Count) > 1)
            return new AOEInstance[1] { aoe };
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ApokalypsisFirst)
            _aoe = new(_rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ApokalypsisFirst:
            case (uint)AID.ApokalypsisRest:
                if (++NumCasts == 5)
                {
                    _aoe = null;
                    NumCasts = 0;
                }
                break;
        }
    }
}

class DeathlyRayThereion(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeRect rect = new(60f, 3f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DeathlyRayVisualThereion1)
            _aoe = new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DeathlyRayThereionFirst:
            case (uint)AID.DeathlyRayThereionRest:
                if (++NumCasts == 5)
                {
                    _aoe = null;
                    NumCasts = 0;
                }
                break;
        }
    }
}

class DeathlyRayFaces(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _rect = new(60f, 3f);
    private readonly List<AOEInstance> _aoesFirst = new(5), _aoesRest = new(5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var countFirst = _aoesFirst.Count;
        var countRest = _aoesRest.Count;
        var total = countFirst + countRest;
        if (total == 0)
            return [];
        var aoes = new AOEInstance[total];
        var index = 0;
        for (var i = 0; i < countFirst; ++i)
            aoes[index++] = _aoesFirst[i];
        for (var i = 0; i < countRest; ++i)
            aoes[index++] = _aoesRest[i] with { Risky = countFirst == 0 };
        return aoes;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var countFirst = _aoesFirst.Count;
        var countRest = _aoesRest.Count;
        if (spell.Action.ID == (uint)AID.DeathlyRayFacesFirst && countFirst == 0 && countRest == 0)
        {
            var faces = Module.Enemies((uint)OID.TheFaceOfTheBeast);
            var count = faces.Count;
            for (var i = 0; i < count; ++i)
            {
                var f = faces[i];
                if (f.Rotation.AlmostEqual(caster.Rotation, Angle.DegToRad))
                    _aoesFirst.Add(new(_rect, f.Position, f.Rotation, default, Colors.Danger));
                else
                    _aoesRest.Add(new(_rect, f.Position, f.Rotation, WorldState.FutureTime(8.5d)));
            }
        }
        if (spell.Action.ID is (uint)AID.DeathlyRayFacesFirst or (uint)AID.DeathlyRayFacesRest)
        {
            ++NumCasts;
            if (NumCasts == 5 * countFirst)
            {
                _aoesFirst.Clear();
                NumCasts = 0;
            }
            if (countFirst == 0 && NumCasts == 5 * countRest)
            {
                _aoesRest.Clear();
                NumCasts = 0;
            }
        }
    }
}

class D063TherionStates : StateMachineBuilder
{
    public D063TherionStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ThereionCharge>()
            .ActivateOnEnter<Misfortune>()
            .ActivateOnEnter<ShadowWreck>()
            .ActivateOnEnter<DeathlyRayFaces>()
            .ActivateOnEnter<DeathlyRayThereion>()
            .ActivateOnEnter<Border>()
            .ActivateOnEnter<Apokalypsis>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 652, NameID = 8210)]
public class D063Therion(WorldState ws, Actor primary) : BossModule(ws, primary, Border.DefaultArena.Center, Border.DefaultArena);
