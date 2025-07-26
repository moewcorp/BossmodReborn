namespace BossMod.Shadowbringers.Dungeon.D11HeroesGauntlet.D113SpectralBerserker;

public enum OID : uint
{
    Boss = 0x2EFD, // R3.0
    Rubble = 0x2EFE, // R2.5
    Crater = 0x1EA1A1, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    BeastlyFury = 21004, // Boss->self, 4.0s cast, range 50 circle

    WildAnguish1 = 21000, // Boss->players, 5.0s cast, range 6 circle
    WildAnguish2 = 21001, // Boss->players, no cast, range 6 circle
    FallingRock = 20997, // Rubble->self, no cast, range 8 circle

    WildRageVisual = 20994, // Boss->location, 5.0s cast, range 8 circle
    WildRage = 20995, // Helper->location, 5.7s cast, range 8 circle
    WildRageKnockback = 20996, // Helper->self, 5.7s cast, range 8-50 donut, raidwide, knockback 15, away fromsource

    WildRampageVisual = 20998, // Boss->self, 5.0s cast, single-target
    WildRampage = 20999, // Helper->self, 5.5s cast, range 50 width 50 rect

    RagingSliceFirst = 21002, // Boss->self, 3.7s cast, range 50 width 6 rect
    RagingSliceRest = 21003 // Boss->self, 2.5s cast, range 50 width 6 rect
}

public enum IconID : uint
{
    Stackmarker = 93, // player
    Spreadmarker = 229 // player
}

class BeastlyFuryArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom cross = new([new Square(D113SpectralBerserker.ArenaCenter, 23)], D113SpectralBerserker.Cross);

    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BeastlyFury && Arena.Bounds == D113SpectralBerserker.StartingBounds)
            _aoe = new(cross, Arena.Center, default, Module.CastFinishAt(spell, 1.1d));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x0B)
        {
            Arena.Bounds = D113SpectralBerserker.DefaultBounds;
            _aoe = null;
        }
    }
}

class FallingRock(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, (uint)AID.WildAnguish2, 8.5f, 6) // 8.5 instead of 6 to prevent aoe from intersecting additional rubble hitboxes
{
    public override void Update()
    {
        if (Spreads.Count != 0)
        {
            var count = Spreads.Count;
            for (var i = 0; i < count; ++i)
            {
                if (Spreads[i].Target.IsDead)
                {
                    Spreads.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsSpreadTarget(actor))
        {
            var rubble = Module.Enemies((uint)OID.Rubble);
            var count = rubble.Count;
            for (var i = 0; i < count; ++i)
            {
                if (!rubble[i].IsDead)
                {
                    hints.Add("Stack alone with rubble!");
                    return;
                }
            }
        }
    }
}

class WildAnguish1(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.WildAnguish1, 6f, 4, 4)
{
    public static bool IsQuadrupleStack(BossModule module)
    {
        var rubble = module.Enemies((uint)OID.Rubble);
        var count = rubble.Count;
        for (var i = 0; i < count; ++i)
        {
            if (!rubble[i].IsDead)
                return true;
        }
        return false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (IsQuadrupleStack(Module))
        { }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsQuadrupleStack(Module))
        { }
        else
            base.AddHints(slot, actor, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (IsQuadrupleStack(Module))
        { }
        else
            base.DrawArenaForeground(pcSlot, pc);
    }
}

class WildAnguish2(BossModule module) : Components.GenericTowers(module)
{
    private readonly FallingRock _sp = module.FindComponent<FallingRock>()!;

    public override void Update()
    {
        if (Towers.Count != 0 && _sp.Spreads.Count == 0)
            Towers.Clear();
    }

    public override void OnActorCreated(Actor actor)
    {
        // theoretically it would be 8.5 (rubble hitboxradius + aoe hitbox radius), but that makes it harder to spread out correctly, because then we would need to spread rubbles twice as far apart
        if (actor.OID == (uint)OID.Rubble)
            Towers.Add(new(actor.Position, actor.HitboxRadius, activation: WorldState.FutureTime(6.1d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.WildAnguish1 or (uint)AID.WildAnguish2)
        {
            var t = WorldState.Actors.Find(spell.MainTargetID);
            var count = Towers.Count;
            for (var i = count - 1; i >= 0; --i)
            {
                if (Towers[i].Position.InCircle(t!.Position, 8.5f))
                {
                    Towers.RemoveAt(i);
                }
            }
            var count2 = _sp.Spreads.Count;
            if (count2 > 0 && spell.Action.ID == (uint)AID.WildAnguish1)
            {
                for (var i = count2 - 1; i >= 0; --i)
                {
                    if (_sp.Spreads[i].Target == t)
                    {
                        _sp.Spreads.RemoveAt(i);
                    }
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) { }
}

class WildRageKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.WildRageKnockback, 15f)
{
    private static readonly Angle a10 = 10f.Degrees(), a45 = 45f.Degrees();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var forbidden = new Func<WPos, float>[2];
            var pos = c.Origin;
            var dir = pos.X == 738f ? 1 : -1;
            forbidden[0] = ShapeDistance.InvertedDonutSector(pos, 8f, 9f, a45 * dir, a10);
            forbidden[1] = ShapeDistance.InvertedDonutSector(pos, 8f, 9f, 3f * a45 * dir, a10);
            hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), c.Activation);
        }
    }
}

class WildRageRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.WildRageKnockback);
class WildRage(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WildRage, 8f);
class BeastlyFury(BossModule module) : Components.RaidwideCast(module, (uint)AID.BeastlyFury);

class CratersWildRampage(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly WPos pos1 = new(738f, 482f), pos2 = new(762f, 482f);
    private static readonly Circle circle1 = new(pos1, 7f), circle2 = new(pos2, 7f);
    public readonly List<Circle> Circles = new(2);
    private bool invert;
    private DateTime activation;
    private AOEShapeCustom? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe is AOEShapeCustom aoe)
        {
            return new AOEInstance[1] { new(aoe with { InvertForbiddenZone = invert }, Arena.Center, default, activation, invert ? Colors.SafeFromAOE : 0) };
        }
        return [];
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00010002 && actor.OID == (uint)OID.Crater)
        {
            if (actor.Position == pos1 && !Circles.Any(x => x.Center == pos1))
                Circles.Add(circle1);
            else if (actor.Position == pos2 && !Circles.Any(x => x.Center == pos2))
                Circles.Add(circle2);
            _aoe = new AOEShapeCustom([.. Circles]);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WildRampage)
        {
            invert = true;
            activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WildRampage)
            invert = false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (invert)
        {
            var aoes = ActiveAOEs(slot, actor);
            var len = aoes.Length;
            var isRisky = true;
            for (var i = 0; i < len; ++i)
            {
                if (aoes[i].Check(actor.Position))
                {
                    isRisky = false;
                    break;
                }
            }
            hints.Add("Go inside crater!", isRisky);
        }
        else
            base.AddHints(slot, actor, hints);
    }
}

sealed class RagingSlice(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RagingSliceFirst, (uint)AID.RagingSliceRest], new AOEShapeRect(50f, 3f));

class D113SpectralBerserkerStates : StateMachineBuilder
{
    public D113SpectralBerserkerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BeastlyFuryArenaChange>()
            .ActivateOnEnter<BeastlyFury>()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<CratersWildRampage>()
            .ActivateOnEnter<WildAnguish1>()
            .ActivateOnEnter<WildAnguish2>()
            .ActivateOnEnter<WildRageKnockback>()
            .ActivateOnEnter<WildRageRaidwide>()
            .ActivateOnEnter<WildRage>()
            .ActivateOnEnter<RagingSlice>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 737, NameID = 9511)]
public class D113SpectralBerserker(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(750f, 482f);
    public static readonly ArenaBoundsSquare StartingBounds = new(22.5f);
    public static readonly Cross[] Cross = [new Cross(ArenaCenter, 20f, 10f)];
    public static readonly ArenaBounds DefaultBounds = new ArenaBoundsComplex(Cross);
}
