namespace BossMod.Endwalker.Dungeon.D06DeadEnds.D062Peacekeeper;

public enum OID : uint
{
    Boss = 0x34C6, // R=9.0
    PerpetualWarMachine = 0x384B, // R=0.9
    ElectricVoidzone = 0x1EB5F7,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 25977, // Boss->player, no cast, single-target

    Decimation = 25936, // Boss->self, 5.0s cast, range 40 circle
    DisengageHatch = 28356, // Boss->self, no cast, single-target
    EclipsingExhaust = 25931, // Boss->self, 5.0s cast, range 40 circle, knockback 11, away from source
    ElectromagneticRepellant = 28360, // Boss->self, 4.0s cast, range 9 circle, voidzone
    Elimination = 25935, // Boss->self/player, 5.0s cast, range 46 width 10 rect, tankbuster
    InfantryDeterrentVisual = 28358, // Boss->self, no cast, single-target
    InfantryDeterrent = 28359, // Helper->player, 5.0s cast, range 6 circle, spread
    NoFutureVisual = 25925, // Boss->self, 4.0s cast, single-target
    NoFutureAOE = 25927, // Helper->self, 4.0s cast, range 6 circle
    NoFutureSpread = 25928, // Helper->player, 5.0s cast, range 6 circle, spread
    OrderToFire = 28351, // Boss->self, 5.0s cast, single-target
    PeacefireVisual = 25933, // Boss->self, 3.0s cast, single-target
    Peacefire = 25934, // Helper->self, 7.0s cast, range 10 circle
    SmallBoreLaser = 28352, // PerpetualWarMachine->self, 5.0s cast, range 20 width 4 rect
    Teleport = 28350, // PerpetualWarMachine->location, no cast, single-target
    VisualModelChange1 = 28357, // Boss->self, no cast, single-target
    VisualModelChange2 = 25926 // Boss->self, no cast, single-target
}

class DecimationArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(16, 20);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x17 && state == 0x00020001)
        {
            Arena.Bounds = D062Peacekeeper.SmallerBounds;
            Arena.Center = D062Peacekeeper.ArenaCenter;
            _aoe = null;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Decimation && Arena.Bounds == D062Peacekeeper.StartingBounds)
            _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell, 0.4f));
    }
}

class ElectromagneticRepellant(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 9, ActionID.MakeSpell(AID.ElectromagneticRepellant), m => m.Enemies(OID.ElectricVoidzone).Where(z => z.EventState != 7), 0.7f);
class InfantryDeterrent(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.InfantryDeterrent), 6);
class NoFutureSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.NoFutureSpread), 6);

class NoFutureAOE(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.NoFutureAOE), 6);
class Peacefire(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Peacefire), 10);
class SmallBoreLaser(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SmallBoreLaser), new AOEShapeRect(20, 2));

class Elimination(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.Elimination), new AOEShapeRect(46, 5), endsOnCastEvent: true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count != 0)
            hints.Add("Tankbuster cleave");
    }
}

class Decimation(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Decimation));
class EclipsingExhaust(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.EclipsingExhaust));

class EclipsingExhaustKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.EclipsingExhaust), 11)
{
    private static readonly Angle a36 = 36.Degrees();
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<Peacefire>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.InBounds(pos);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var component = Module.FindComponent<Peacefire>()?.ActiveAOEs(slot, actor)?.ToList();
        var source = Sources(slot, actor).FirstOrDefault();
        if (component != null && source != default)
        {
            var forbidden = new List<Func<WPos, float>>(6);
            for (var i = 0; i < component.Count; ++i)

                forbidden.Add(ShapeDistance.Cone(Arena.Center, 16, Angle.FromDirection(component[i].Origin - Arena.Center), a36));
            forbidden.Add(ShapeDistance.InvertedCircle(Arena.Center, 4));
            hints.AddForbiddenZone(ShapeDistance.Union(forbidden), source.Activation);
        }
    }
}

class D062PeacekeeperStates : StateMachineBuilder
{
    public D062PeacekeeperStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DecimationArenaChange>()
            .ActivateOnEnter<ElectromagneticRepellant>()
            .ActivateOnEnter<InfantryDeterrent>()
            .ActivateOnEnter<NoFutureSpread>()
            .ActivateOnEnter<NoFutureAOE>()
            .ActivateOnEnter<Peacefire>()
            .ActivateOnEnter<SmallBoreLaser>()
            .ActivateOnEnter<Elimination>()
            .ActivateOnEnter<Decimation>()
            .ActivateOnEnter<EclipsingExhaust>()
            .ActivateOnEnter<EclipsingExhaustKnockback>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 792, NameID = 10315)]
public class D062Peacekeeper(WorldState ws, Actor primary) : BossModule(ws, primary, StartingBounds.Center, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(-105, -210);
    private static readonly Angle offset = 5.625f.Degrees();
    public static readonly ArenaBoundsComplex StartingBounds = new([new Polygon(ArenaCenter, 19.5f * CosPI.Pi32th, 32, offset)], [new Rectangle(new(-105, -229), 20, 0.78f),
    new Rectangle(new(-105, -190), 20, 1.1f)]);
    public static readonly ArenaBoundsComplex SmallerBounds = new([new Polygon(ArenaCenter, 16, 32, offset)]);
}
