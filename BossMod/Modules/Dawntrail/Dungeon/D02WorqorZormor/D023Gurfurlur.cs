namespace BossMod.Dawntrail.Dungeon.D02WorqorZormor.D023Gurfurlur;

public enum OID : uint
{
    Boss = 0x415F, // R7.000, x1
    AuraSphere = 0x4162, // R1.0
    BitingWind = 0x4160, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    HeavingHaymakerVisual = 36269, // Boss->self, 5.0s cast, single-target
    HeavingHaymaker = 36375, // Helper->self, 5.3s cast, range 60 circle

    Stonework = 36301, // Boss->self, 3.0s cast, single-target
    LithicImpact = 36302, // Helper->self, 6.8s cast, range 4 width 4 rect
    GreatFlood = 36307, // Helper->self, 7.0s cast, range 80 width 60 rect

    Allfire1 = 36303, // Helper->self, 7.0s cast, range 10 width 10 rect
    Allfire2 = 36304, // Helper->self, 8.5s cast, range 10 width 10 rect
    Allfire3 = 36305, // Helper->self, 10.0s cast, range 10 width 10 rect

    VolcanicDrop = 36306, // Helper->player, 5.0s cast, range 6 circle

    SledgeHammerMarker = 36315, // Helper->player, no cast, single-target
    Sledgehammer1 = 36313, // Boss->self/players, 5.0s cast, range 60 width 8 rect, line stack
    Sledgehammer2 = 36314, // Boss->self, no cast, range 60 width 8 rect
    Sledgehammer3 = 39260, // Boss->self, no cast, range 60 width 8 rect

    ArcaneStomp = 36319, // Boss->self, 3.0s cast, single-target

    ShroudOfEons1 = 36321, // AuraSphere->player, no cast, single-target
    ShroudOfEons2 = 36322, // AuraSphere->Boss, no cast, single-target

    EnduringGlory = 36320, // Boss->self, 6.0s cast, range 60 circle

    Windswrath1 = 36310, // Helper->self, 7.0s cast, range 40
    Windswrath2 = 39074, // Helper->self, 15.0s cast, range 40 circle

    Whirlwind = 36311 // Helper->self, no cast, range 5 circle
}

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(D023Gurfurlur.ArenaCenter, 25f)], [new Square(D023Gurfurlur.ArenaCenter, 20f)]);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HeavingHaymaker && Arena.Bounds == D023Gurfurlur.StartingBounds)
            _aoe = new(square, Arena.Center, default, Module.CastFinishAt(spell, 0.6f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x18)
        {
            Arena.Bounds = D023Gurfurlur.DefaultBounds;
            _aoe = null;
        }
    }
}

class AuraSphere(BossModule module) : BossComponent(module)
{
    private readonly IEnumerable<Actor> _orbs = module.Enemies(OID.AuraSphere).Where(orb => !orb.IsDead);

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_orbs.Any())
            hints.Add("Soak the orbs!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        Actor[] orbz = [.. _orbs];
        var len = orbz.Length;
        if (len != 0)
        {
            var orbs = new Func<WPos, float>[len];
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), actor, ActionQueue.Priority.High);
            for (var i = 0; i < len; ++i)
            {
                var o = orbz[i];
                orbs[i] = ShapeDistance.InvertedRect(o.Position + 0.5f * o.Rotation.ToDirection(), new WDir(0f, 1f), 0.5f, 0.5f, 0.5f);
            }
            hints.AddForbiddenZone(ShapeDistance.Intersection(orbs), DateTime.MaxValue);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var orb in _orbs)
            Arena.AddCircle(orb.Position, 1f, Colors.Safe);
    }
}

class SledgeHammer(BossModule module) : Components.LineStack(module, ActionID.MakeSpell(AID.SledgeHammerMarker), ActionID.MakeSpell(AID.Sledgehammer3), 4.9f);
class HeavingHaymaker(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HeavingHaymaker));
class LithicImpact(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LithicImpact), new AOEShapeRect(4f, 2f));
class Whirlwind(BossModule module) : Components.PersistentVoidzone(module, 5f, m => m.Enemies(OID.BitingWind), 7);

class GreatFlood(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.GreatFlood), 25f, kind: Kind.DirForward)
{
    private readonly Allfire _aoe = module.FindComponent<Allfire>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;

        Source source = default;
        var sources = Sources(slot, actor);
        foreach (var s in sources)
        {
            source = s;
            break;
        }

        var component = _aoe.AOEs;
        if (_aoe.AOEs.Count == 0 && source != default)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(source.Origin, source.Direction, 15f, default, 20f), source.Activation);

    }
}

class Allfire(BossModule module) : Components.GenericAOEs(module)
{
    private const string Hint = "Be inside safespot for knockback!";
    private static readonly AOEShapeRect rect = new(10f, 5f);
    public readonly List<AOEInstance> AOEs = new(16);
    private static readonly AOEShapeRect safespot = new(15f, 10f, InvertForbiddenZone: true);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        Components.Knockback.Source source = default;
        var sources = Module.FindComponent<GreatFlood>()!.Sources(slot, actor);
        foreach (var s in sources)
        {
            source = s;
            break;
        }
        if (source != default)
            return [new(safespot, source.Origin, source.Direction, source.Activation, Colors.SafeFromAOE)];
        else
        {
            var max = count >= 12 ? 12 : count == 8 ? 8 : 4;
            var aoes = new AOEInstance[max];
            var act0 = AOEs[0].Activation;
            for (var i = 0; i < max; ++i)
            {
                var aoe = AOEs[i];
                aoes[i] = (aoe.Activation - act0).TotalSeconds < 1d ? aoe with { Color = count > 4 ? Colors.Danger : 0 } : aoe with { Risky = false };
            }
            return aoes;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Allfire1 or (uint)AID.Allfire2 or (uint)AID.Allfire3)
        {
            AOEs.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            if (AOEs.Count == 16)
                AOEs.SortBy(x => x.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID is (uint)AID.Allfire1 or (uint)AID.Allfire2 or (uint)AID.Allfire3)
            AOEs.RemoveAt(0);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (AOEs.Count == 0)
            return;

        var activeAOEs = ActiveAOEs(slot, actor);
        var hasSafeSpot = false;
        var isActorInSafeSpot = true;

        foreach (var aoe in activeAOEs)
        {
            if (aoe.Shape == safespot)
                hasSafeSpot = true;
            if (hasSafeSpot)
            {
                if (!aoe.Check(actor.Position))
                    isActorInSafeSpot = false;
                break;
            }
        }

        if (!hasSafeSpot)
            base.AddHints(slot, actor, hints);
        else
        {
            if (!isActorInSafeSpot)
                hints.Add(Hint);
            else
                hints.Add(Hint, false);
        }
    }
}

class VolcanicDrop(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.VolcanicDrop), 6f);
class EnduringGlory(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.EnduringGlory));
class Windswrath1Raidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Windswrath1));
class Windswrath2Raidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Windswrath2));
class GreatFloodRaidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.GreatFlood));

class Windswrath(BossModule module, AID aid) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(aid), 15f);
class Windswrath1(BossModule module) : Windswrath(module, AID.Windswrath1)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;
        Source source = default;
        var sources = Sources(slot, actor);
        foreach (var s in sources)
        {
            source = s;
            break;
        }
        if (source != default)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(source.Origin, 5f), source.Activation);
    }
}

class Windswrath2(BossModule module) : Windswrath(module, AID.Windswrath2)
{
    private enum Pattern { None, EWEW, WEWE }
    private Pattern CurrentPattern;
    private static readonly Angle a15 = 15f.Degrees(), a165 = 165f.Degrees(), a105 = 105f.Degrees(), a75 = 75f.Degrees();
    private readonly Whirlwind _aoe = module.FindComponent<Whirlwind>()!;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.BitingWind)
        {
            if (actor.Position == new WPos(-74f, -180f))
                CurrentPattern = Pattern.EWEW;
            else if (actor.Position == new WPos(-74f, -210f))
                CurrentPattern = Pattern.WEWE;
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => _aoe.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) || !Module.InBounds(pos);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;
        Source source = default;
        var sources = Sources(slot, actor);
        foreach (var s in sources)
        {
            source = s;
            break;
        }
        var forbidden = new List<Func<WPos, float>>(4);
        Components.GenericAOEs.AOEInstance[] component = [.. _aoe.ActiveAOEs(slot, actor)];
        if (component.Length != 0 && source != default)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            var timespan = (source.Activation - WorldState.CurrentTime).TotalSeconds;
            if (timespan <= 3d)
            {
                var patternWEWE = CurrentPattern == Pattern.WEWE;
                var origin = source.Origin;
                forbidden.Add(ShapeDistance.InvertedCone(origin, 5f, patternWEWE ? a15 : -a15, a15));
                forbidden.Add(ShapeDistance.InvertedCone(origin, 5f, patternWEWE ? -a165 : a165, a15));
                forbidden.Add(ShapeDistance.InvertedCone(origin, 5f, patternWEWE ? a105 : -a105, a15));
                forbidden.Add(ShapeDistance.InvertedCone(origin, 5f, patternWEWE ? -a75 : a75, a15));
            }
            else
                forbidden.Add(ShapeDistance.InvertedCircle(source.Origin, 8f));
            hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), source.Activation);
        }
    }
}

class D023GurfurlurStates : StateMachineBuilder
{
    public D023GurfurlurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<HeavingHaymaker>()
            .ActivateOnEnter<Whirlwind>()
            .ActivateOnEnter<AuraSphere>()
            .ActivateOnEnter<LithicImpact>()
            .ActivateOnEnter<GreatFloodRaidwide>()
            .ActivateOnEnter<Allfire>()
            .ActivateOnEnter<GreatFlood>()
            .ActivateOnEnter<VolcanicDrop>()
            .ActivateOnEnter<EnduringGlory>()
            .ActivateOnEnter<SledgeHammer>()
            .ActivateOnEnter<Windswrath1>()
            .ActivateOnEnter<Windswrath1Raidwide>()
            .ActivateOnEnter<Windswrath2>()
            .ActivateOnEnter<Windswrath2Raidwide>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 824, NameID = 12705)]
public class D023Gurfurlur(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(-54f, -195f);
    public static readonly ArenaBoundsSquare StartingBounds = new(24.5f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20f);
}
