namespace BossMod.Dawntrail.Dungeon.D08StrayboroughDeadwalk.D081HisRoyalHeadnessLeonoggI;

public enum OID : uint
{
    Boss = 0x4183, // R3.6
    LittleLadyNogginette = 0x41BD, // R1.0
    LittleLordNoggington = 0x41BB, // R1.0
    NobleNoggin = 0x4205, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    MaliciousMist = 36529, // Boss->self, 5.0s cast, range 50 circle, raidwide

    FallingNightmareVisual = 36526, // Boss->self, 3.0s cast, single-target
    FallingNightmare1 = 36532, // NobleNoggin->self, 2.0s cast, range 2 circle
    FallingNightmare2 = 36536, // NobleNoggin->self, 1.0s cast, range 2 circle, only happens if caught by add

    MorbidFascination = 36528, // Boss->self, no cast, single-target

    TeamSpirit = 36527, // Boss->self, 3.0s cast, single-target, summons dolls
    SpiritedChargeVisual = 36598, // Boss->self, 3.0s cast, single-target
    SpiritedChargeStart = 36533, // LittleLordNoggington/LittleLadyNogginette->self, no cast, single-target
    SpiritedCharge = 36534, // Helper->self, no cast, range 2 width 1 rect
    Overattachment = 36535, // LittleLadyNogginette/LittleLordNoggington->player, no cast, single-target

    EvilSchemeVisual = 39682, // Boss->self, 6.0s cast, single-target, exaflare
    EvilSchemeFirst = 39683, // Helper->self, 6.0s cast, range 4 circle
    EvilSchemeRest = 39684, // Helper->self, no cast, range 4 circle

    LoomingNightmareVisual = 39685, // Boss->self, 5.0s cast, single-target, chasing AOE
    LoomingNightmareFirst = 39686, // Helper->self, 2.0s cast, range 4 circle
    LoomingNightmareRest = 39687, // Helper->self, no cast, range 4 circle

    ScreamVisual1 = 36530, // Boss->self, 5.0s cast, single-target
    ScreamVisual2 = 36541, // Boss->self, no cast, single-target
    Scream = 36531 // Helper->self, 5.0s cast, range 20 60-degree cone
}

public enum IconID : uint
{
    ChasingAOE = 197 // player
}

class MaliciousMistArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(14, 20);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MaliciousMist && Arena.Bounds == D081HisRoyalHeadnessLeonoggI.StartingBounds)
            _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell, 0.9f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x00)
        {
            Arena.Bounds = D081HisRoyalHeadnessLeonoggI.DefaultBounds;
            _aoe = null;
        }
    }
}

class LoomingNightmare(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(4), ActionID.MakeSpell(AID.LoomingNightmareFirst), ActionID.MakeSpell(AID.LoomingNightmareRest), 3, 1.6f, 5, true, (uint)IconID.ChasingAOE)
{
    private int totalChasers;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action == ActionFirst)
        {
            ++totalChasers;
            if (totalChasers > 1)
                MaxCasts = 4;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Actors.Contains(actor))
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 13.5f), Activation);
    }
}

class FallingNightmare(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(2);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.NobleNoggin && id == 0x11D1)
            _aoes.Add(new(circle, actor.Position, default, WorldState.FutureTime(3))); // can be 3 or 4 seconds depending on mechanic
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.FallingNightmare1 or AID.FallingNightmare2)
            _aoes.RemoveAt(0);
    }
}

class SpiritedCharge(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(6, 1);
    private readonly List<Actor> _charges = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _charges)
            yield return new(rect, c.Position, c.Rotation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SpiritedChargeStart)
            _charges.Add(caster);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E3C)
            _charges.Remove(actor);
    }
}

class EvilScheme(BossModule module) : Components.Exaflare(module, 4)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.EvilSchemeFirst)
            Lines.Add(new() { Next = spell.LocXZ, Advance = 4 * spell.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell, 1.6f), TimeToMove = 1.5f, ExplosionsLeft = 5, MaxShownExplosions = 5 });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.EvilSchemeFirst or AID.EvilSchemeRest)
        {
            var index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
            if (index < 0)
                return;
            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}

class MaliciousMist(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MaliciousMist));

class Scream : Components.SimpleAOEs
{
    public Scream(BossModule module) : base(module, ActionID.MakeSpell(AID.Scream), new AOEShapeCone(20, 30.Degrees()), 4) { MaxDangerColor = 2; }
}

class D081HisRoyalHeadnessLeonoggIStates : StateMachineBuilder
{
    public D081HisRoyalHeadnessLeonoggIStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MaliciousMistArenaChange>()
            .ActivateOnEnter<MaliciousMist>()
            .ActivateOnEnter<LoomingNightmare>()
            .ActivateOnEnter<EvilScheme>()
            .ActivateOnEnter<FallingNightmare>()
            .ActivateOnEnter<SpiritedCharge>()
            .ActivateOnEnter<Scream>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 981, NameID = 13073)]
public class D081HisRoyalHeadnessLeonoggI(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(0, 150);
    public static readonly ArenaBoundsCircle StartingBounds = new(19.5f);
    public static readonly ArenaBoundsCircle DefaultBounds = new(14);
}
