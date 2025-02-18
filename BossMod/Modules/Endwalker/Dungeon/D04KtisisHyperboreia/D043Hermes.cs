namespace BossMod.Endwalker.Dungeon.D04KtisisHyperboreia.D043Hermes;

public enum OID : uint
{
    Boss = 0x348A, // R4.2
    Meteor = 0x348C, // R2.4
    Karukeion = 0x348B, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    CosmicKiss = 25891, // Meteor->self, 5.0s cast, range 40 circle
    Double = 25892, // Boss->self, 3.0s cast, single-target

    Hermetica1 = 25888, // Boss->self, 3.0s cast, single-target
    Hermetica2 = 25893, // Boss->self, 6.0s cast, single-target
    Hermetica3 = 25895, // Boss->self, 12.0s cast, single-target

    Meteor = 25890, // Boss->self, 3.0s cast, single-target
    Quadruple = 25894, // Boss->self, 3.0s cast, single-target
    Trismegistos = 25886, // Boss->self, 5.0s cast, range 40 circle, raidwide

    TrueAeroVisual = 25899, // Boss->self, 5.0s cast, single-target
    TrueAeroTarget = 25887, // Helper->player, no cast, single-target
    TrueAeroFirst = 25900, // Helper->player, no cast, range 40 width 6 rect
    TrueAeroRepeat = 25901, // Helper->self, 2.5s cast, range 40 width 6 rect

    TrueAeroII1 = 25896, // Boss->self, 5.0s cast, single-target
    TrueAeroII2 = 25897, // Helper->player, 5.0s cast, range 6 circle
    TrueAeroII3 = 25898, // Helper->location, 3.5s cast, range 6 circle

    TrueAeroIV1 = 25889, // Karukeion->self, 4.0s cast, range 50 width 10 rect
    TrueAeroIVLOS = 27836, // Karukeion->self, 4.0s cast, range 50 width 10 rect
    TrueAeroIV3 = 27837, // Karukeion->self, 10.0s cast, range 50 width 10 rect

    TrueBravery = 25907, // Boss->self, 5.0s cast, single-target

    TrueTornado1 = 25902, // Boss->self, 5.0s cast, single-target
    TrueTornado2 = 25903, // Boss->self, no cast, single-target
    TrueTornado3 = 25904, // Boss->self, no cast, single-target
    TrueTornado4 = 25905, // Helper->player, no cast, range 4 circle
    TrueTornadoAOE = 25906 // Helper->location, 2.5s cast, range 4 circle
}

public enum IconID : uint
{
    Tankbuster = 218 // player
}

class TrismegistosArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(20, 22);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Trismegistos && Arena.Bounds == D043Hermes.StartingBounds)
            _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell, 0.5f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x08)
        {
            Arena.Bounds = D043Hermes.DefaultBounds;
            _aoe = null;
        }
    }
}

class TrueBraveryInterruptHint(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.TrueBravery));
class Trismegistos(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Trismegistos));

class TrueTornadoTankbuster(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(4), (uint)IconID.Tankbuster, ActionID.MakeSpell(AID.TrueTornado4), 5.1f, true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count != 0)
            hints.Add("Tankbuster cleave");
    }
}

class TrueTornadoAOE(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TrueTornadoAOE), 4);

class TrueAeroFirst(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect rect = new(40, 3);
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TrueAeroTarget)
            CurrentBaits.Add(new(Module.PrimaryActor, WorldState.Actors.Find(spell.MainTargetID)!, rect, WorldState.FutureTime(5.7f)));
        else if ((AID)spell.Action.ID == AID.TrueAeroFirst)
            CurrentBaits.Clear();
    }
}

class TrueAeroRepeat(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TrueAeroRepeat), new AOEShapeRect(40, 3));

class TrueAeroII2(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.TrueAeroII2), 6);
class TrueAeroII3(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TrueAeroII3), 6);

class TrueAeroIV1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TrueAeroIV1), new AOEShapeRect(50, 5));
class TrueAeroIV3(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TrueAeroIV3), new AOEShapeRect(50, 5), 4);

class CosmicKiss(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CosmicKiss), 10);

class TrueAeroIVLOS(BossModule module) : Components.CastLineOfSightAOE(module, ActionID.MakeSpell(AID.TrueAeroIVLOS), 50, false, true)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies(OID.Meteor).Count > 0 ? Module.Enemies(OID.Meteor).Where(x => x.ModelState.AnimState2 != 1) : Module.Enemies(OID.Meteor);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var component = Module.FindComponent<CosmicKiss>()!.ActiveAOEs(slot, actor).Any();
        if (BlockerActors().Any() && !component) // force AI to move closer to the meteor as soon as they become visible
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(BlockerActors().First().Position, 8));
    }
}

class D043HermesStates : StateMachineBuilder
{
    public D043HermesStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TrismegistosArenaChange>()
            .ActivateOnEnter<TrueBraveryInterruptHint>()
            .ActivateOnEnter<CosmicKiss>()
            .ActivateOnEnter<TrueAeroFirst>()
            .ActivateOnEnter<TrueAeroRepeat>()
            .ActivateOnEnter<TrueAeroII2>()
            .ActivateOnEnter<TrueAeroII3>()
            .ActivateOnEnter<TrueAeroIV1>()
            .ActivateOnEnter<TrueAeroIVLOS>()
            .ActivateOnEnter<TrueAeroIV3>()
            .ActivateOnEnter<TrueTornadoTankbuster>()
            .ActivateOnEnter<TrueTornadoAOE>()
            .ActivateOnEnter<Trismegistos>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 787, NameID = 10363)]
public class D043Hermes(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    private static readonly WPos ArenaCenter = new(0, -50);
    public static readonly ArenaBoundsComplex StartingBounds = new([new Polygon(ArenaCenter, 21.5f, 64)]);
    public static readonly ArenaBoundsComplex DefaultBounds = new([new Polygon(ArenaCenter, 20, 64)]);
}
