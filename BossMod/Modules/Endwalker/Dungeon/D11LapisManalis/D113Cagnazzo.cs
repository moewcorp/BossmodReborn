namespace BossMod.Endwalker.Dungeon.D11LapisManalis.D113Cagnazzo;

public enum OID : uint
{
    Boss = 0x3AE2, //R=8.0
    FearsomeFlotsam = 0x3AE3, //R=2.4
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Teleport = 31131, // Boss->location, no cast, single-target, boss teleports 

    StygianDeluge = 31139, // Boss->self, 5.0s cast, range 80 circle
    AntediluvianVisual = 31119, // Boss->self, 5.0s cast, single-target
    Antediluvian = 31120, // Helper->self, 6.5s cast, range 15 circle
    BodySlamVisual = 31121, // Boss->location, 6.5s cast, single-target
    BodySlamKB = 31122, // Helper->self, 7.5s cast, range 60 circle, knockback 10, away from source
    BodySlam = 31123, // Helper->self, 7.5s cast, range 8 circle

    HydrobombTelegraph = 32695, // Helper->location, 2.0s cast, range 4 circle
    HydraulicRamTelegraph = 32693, // Helper->location, 2.0s cast, width 8 rect charge
    HydraulicRamVisual = 32692, // Boss->self, 6.0s cast, single-target
    HydraulicRam = 32694, // Boss->location, no cast, width 8 rect charge
    Hydrobomb = 32696, // Helper->location, no cast, range 4 circle
    StartHydrofall = 31126, // Boss->self, no cast, single-target
    Hydrofall = 31375, // Boss->self, 5.0s cast, single-target
    Hydrofall2 = 31376, // Helper->players, 5.5s cast, range 6 circle
    CursedTide = 31130, // Boss->self, 5.0s cast, single-target
    StartLimitbreakPhase = 31132, // Boss->self, no cast, single-target
    NeapTide = 31134, // Helper->player, no cast, range 6 circle
    Hydrovent = 31136, // Helper->location, 5.0s cast, range 6 circle
    SpringTide = 31135, // Helper->players, no cast, range 6 circle
    Tsunami = 31137, // Helper->self, no cast, range 80 width 60 rect
    TsunamiEnrage = 31138, // Helper->self, no cast, range 80 width 60 rect
    VoidcleaverVisual = 31110, // Boss->self, 4.0s cast, single-target
    Voidcleaver = 31111, // Helper->self, no cast, range 100 circle
    VoidMiasma = 32691, // Helper->self, 3.0s cast, range 50 30-degree cone
    LifescleaverVisual = 31112, // Boss->self, 4.0s cast, single-target
    Lifescleaver = 31113, // Helper->self, 5.0s cast, range 50 30-degree cone
    VoidTorrent = 31118 // Boss->self/player, 5.0s cast, range 60 width 8 rect
}

public enum IconID : uint
{
    Stackmarker = 161, // player
    Spreadmarker = 139, // player
    Tankbuster = 230 // player
}

public enum TetherID : uint
{
    LimitBreakCharger = 3, // FearsomeFlotsam->Boss
    BaitAway = 1 // 3E97->player
}

public enum NPCYell : uint
{
    LimitBreakStart = 15175
}

class StygianDelugeArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(D113Cagnazzo.ArenaCenter, 30)], [new Square(D113Cagnazzo.ArenaCenter, 20)]);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.StygianDeluge && Arena.Bounds == D113Cagnazzo.StartingBounds)
            _aoe = new(square, Arena.Center, default, Module.CastFinishAt(spell, 0.7f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x00)
        {
            Arena.Bounds = D113Cagnazzo.DefaultBounds;
            _aoe = null;
        }
    }
}

class VoidTorrent(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.VoidTorrent), new AOEShapeRect(60, 4))
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

class Voidcleaver(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Voidcleaver));
class VoidMiasmaBait(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(50, 15.Degrees()), (uint)TetherID.BaitAway);

class Cleaver(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(50, 15.Degrees()));
class VoidMiasma(BossModule module) : Cleaver(module, AID.VoidMiasma);
class Lifescleaver(BossModule module) : Cleaver(module, AID.Lifescleaver);

class Tsunami(BossModule module) : Components.RaidwideAfterNPCYell(module, ActionID.MakeSpell(AID.Tsunami), (uint)NPCYell.LimitBreakStart, 4.5f);
class StygianDeluge(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.StygianDeluge));
class Antediluvian(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Antediluvian), new AOEShapeCircle(15))
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (NumCasts == 6 && (AID)spell.Action.ID == AID.Antediluvian)
            NumCasts = 0;
    }
}

class BodySlam(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BodySlam), new AOEShapeCircle(8));
class BodySlamKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.BodySlamKB), 10, true)
{
    private WPos data;
    private DateTime activation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if ((AID)spell.Action.ID == AID.BodySlamKB)
        {
            activation = Module.CastFinishAt(spell, 0.6f);
            data = caster.Position;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if ((Sources(slot, actor).Any() || activation > WorldState.CurrentTime) && Module.FindComponent<Antediluvian>()!.NumCasts >= 4)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(data, 10), activation.AddSeconds(-0.6f));
    }
}

class HydraulicRam(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return _aoes[0] with { Color = Colors.Danger };
        for (var i = 1; i < _aoes.Count; ++i)
            yield return _aoes[i];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HydraulicRamTelegraph)
        {
            var dir = spell.LocXZ - caster.Position;
            _aoes.Add(new(new AOEShapeRect(dir.Length(), 4), caster.Position, Angle.FromDirection(dir), Module.CastFinishAt(spell, 5.7f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.HydraulicRam)
            _aoes.RemoveAt(0);
    }
}

class Hydrobomb(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(4);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 1)
            for (var i = 0; i < 2; ++i)
                yield return _aoes[i] with { Color = Colors.Danger };
        for (var i = 1; i < _aoes.Count; ++i)
            yield return _aoes[i];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HydrobombTelegraph)
            _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 6.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.Hydrobomb)
            _aoes.RemoveAt(0);
    }
}

class Hydrovent(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Hydrovent), 6);
class NeapTide(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, ActionID.MakeSpell(AID.NeapTide), 6, 5);

class SpringTideHydroFall(BossModule module) : Components.UniformStackSpread(module, 6, 0, 4) // both use the same icon
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Stackmarker)
            AddStack(actor, WorldState.FutureTime(5));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SpringTide or AID.Hydrofall2)
            Stacks.Clear();
    }
}

class D113CagnazzoStates : StateMachineBuilder
{
    public D113CagnazzoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Components.StayInBounds>()
            .ActivateOnEnter<StygianDelugeArenaChange>()
            .ActivateOnEnter<Voidcleaver>()
            .ActivateOnEnter<Lifescleaver>()
            .ActivateOnEnter<VoidMiasma>()
            .ActivateOnEnter<VoidMiasmaBait>()
            .ActivateOnEnter<Antediluvian>()
            .ActivateOnEnter<BodySlam>()
            .ActivateOnEnter<BodySlamKB>()
            .ActivateOnEnter<HydraulicRam>()
            .ActivateOnEnter<Hydrobomb>()
            .ActivateOnEnter<SpringTideHydroFall>()
            .ActivateOnEnter<NeapTide>()
            .ActivateOnEnter<StygianDeluge>()
            .ActivateOnEnter<Hydrovent>()
            .ActivateOnEnter<VoidTorrent>()
            .ActivateOnEnter<Tsunami>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 896, NameID = 11995)]
public class D113Cagnazzo(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(-250, 130);
    public static readonly ArenaBoundsSquare StartingBounds = new(29.5f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.FearsomeFlotsam));
    }
}
