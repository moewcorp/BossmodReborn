namespace BossMod.Stormblood.Dungeon.D06AlaMhigo.D063ZenosYaeGalvus;

public enum OID : uint
{
    Boss = 0x1BAA, // R0.96
    ZenosYaeGalvusClone1 = 0x1BAB, // R0.96
    ZenosYaeGalvusClone2 = 0xF88C5, // R0.5-0.8
    TheSwell = 0x1BAC, // R3.0
    AmeNoHabakiri = 0x1BAE, // R3.0
    TheStorm = 0x1BAD, // R3.0
    ArenaVoidzone = 0x1EA1A1,
    Helper1 = 0x18D6,
    Helper2 = 0x1DD3,
    Helper3 = 0x1DCB,
    Helper4 = 0x1DCC,
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/Helper4/Helper2/Helper3->player/Helper2/Helper3/player/Helper4, no cast, single-target
    Teleport1 = 8292, // Boss->location, no cast, ???
    Teleport2 = 9119, // ZenosYaeGalvusClone1->location, no cast, ???
    Begin = 8287, // Boss->self, no cast, single-target

    ArtOfTheSwell1 = 8293, // Boss->self, 6.0s cast, range 33 circle, knockback 15, away from source
    ArtOfTheSwell2 = 9606, // ZenosYaeGalvusClone1->self, 6.0s cast, range 33 circle, knockback 15, away from source
    Hakaze = 9140, // Helper4->Helper2, no cast, single-target
    SpinningEdge = 9300, // Helper3->Helper2, no cast, single-target

    UnmovingTroikaFirst = 8288, // Boss->self, no cast, range 9+R 120-degree cone, starts 3.2s after kill NPC yell
    UnmovingTroikaSecond = 8289, // Helper1->self, 1.7s cast, range 9+R 120-degree cone
    UnmovingTroikaLast = 8290, // Helper1->self, 2.1s cast, range 9+R 120-degree cone

    ArtOfTheStormVisual = 8295, // Helper1->self, 1.0s cast, range 15 circle
    ArtOfTheStorm1 = 8294, // Boss->self, 6.0s cast, range 8 circle
    ArtOfTheStorm2 = 9607, // ZenosYaeGalvusClone1->self, 8.0s cast, range 8 circle

    ArtOfTheSwordVisual1 = 8296, // Boss->self, 5.5s cast, single-target, proteans
    ArtOfTheSwordVisual2 = 9608, // ZenosYaeGalvusClone1->self, 5.5s cast, single-target
    ArtOfTheSword1 = 8297, // Helper1->self, no cast, range 40+R width 6 rect
    ArtOfTheSword2 = 9609, // Helper1->self, no cast, range 40+R width 6 rect

    FastBlade = 717, // Helper2->player/Helper4/Helper3, no cast, single-target

    VeinSplitter1 = 9398, // Boss->self, 3.5s cast, range 10 circle
    VeinSplitter2 = 8300, // ZenosYaeGalvusClone1->self, 3.0s cast, range 10 circle

    LightlessSpark = 8299, // Boss->self, 3.0s cast, range 40+R 90-degree cone, bait away
    Concentrativity = 8301, // Boss->self, 3.0s cast, range 40 circle, raidwide

    LimitBreakStart = 8302, // Boss->self, no cast, single-target
    StormSwellSwordVisual = 8303, // Boss->self, no cast, single-target
    LimitBreakVisual = 9118, // TheSwell/TheStorm/AmeNoHabakiri->self, no cast, single-target
    StormSwellSwordFinish = 8304 // Helper1->self, 7.0s cast, range 40 circle
}

public enum NPCYell : ushort
{
    Begin = 5371,
    Kill = 5372
}

public enum TetherID : uint
{
    BaitAway = 41 // Boss->player
}

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(20, 35);
    private AOEInstance? _aoe;
    private bool begin;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008 && (OID)actor.OID == OID.ArenaVoidzone)
        {
            Arena.Bounds = D063ZenosYaeGalvus.DefaultBounds;
            _aoe = null;
            begin = true;
        }
    }

    public override void Update()
    {
        if (!begin && _aoe == null)
            _aoe = new(donut, Arena.Center, default, WorldState.FutureTime(10));
    }
}

class UnmovingTroikaFirst(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.UnmovingTroikaFirst), new AOEShapeCone(9.96f, 60.Degrees()))
{
    private bool imminent;

    public override void OnActorNpcYell(Actor actor, ushort id)
    {
        if ((ushort)NPCYell.Kill == id)
            imminent = true;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (imminent)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (imminent)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (imminent)
            base.DrawArenaForeground(pcSlot, pc);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.UnmovingTroikaFirst)
            imminent = false;
    }
}

abstract class UnmovingTroika(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(9.96f, 60.Degrees()));
class UnmovingTroikaSecond(BossModule module) : UnmovingTroika(module, AID.UnmovingTroikaSecond);
class UnmovingTroikaLast(BossModule module) : UnmovingTroika(module, AID.UnmovingTroikaLast);

abstract class ArtOfTheStorm(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 8);
class ArtOfTheStorm1(BossModule module) : ArtOfTheStorm(module, AID.ArtOfTheStorm1);
class ArtOfTheStorm2(BossModule module) : ArtOfTheStorm(module, AID.ArtOfTheStorm2);

abstract class VeinSplitter(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 10);
class VeinSplitter1(BossModule module) : VeinSplitter(module, AID.VeinSplitter1);
class VeinSplitter2(BossModule module) : VeinSplitter(module, AID.VeinSplitter2);

abstract class ArtOfTheSwell(BossModule module, AID aid) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(aid), 15)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 5), Module.CastFinishAt(Casters.FirstOrDefault()!.CastInfo));
    }
}
class ArtOfTheSwell1(BossModule module) : ArtOfTheSwell(module, AID.ArtOfTheSwell1);
class ArtOfTheSwell2(BossModule module) : ArtOfTheSwell(module, AID.ArtOfTheSwell2);

class ArtOfTheSword(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect rect = new(40.5f, 3);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ArtOfTheSwordVisual1 or AID.ArtOfTheSwordVisual2)
            foreach (var a in Raid.WithoutSlot(false, true, true))
                CurrentBaits.Add(new(caster, a, rect, WorldState.FutureTime(6.3f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ArtOfTheSword1 or AID.ArtOfTheSword2)
            CurrentBaits.Clear();
    }
}

class LightlessSparkBaitaway(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(40.96f, 45.Degrees()), (uint)TetherID.BaitAway, ActionID.MakeSpell(AID.LightlessSpark), activationDelay: 8)
{
    private readonly ArtOfTheSwell1 _kb = module.FindComponent<ArtOfTheSwell1>()!;
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb.Casters.Count > 0) // resolve knockback before baits
            return;
        base.AddAIHints(slot, actor, assignment, hints);
        var bait = CurrentBaits.FirstOrDefault(x => x.Target == actor).Source;
        if (CurrentBaits.Any(x => x.Target == actor))
            hints.AddForbiddenZone(ShapeDistance.InvertedCone(bait.Position, 10, Angle.FromDirection(bait.Position - Arena.Center), 45.Degrees()), WorldState.FutureTime(ActivationDelay));
    }
}

class LightlessSparkAOE(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LightlessSpark), new AOEShapeCone(40.96f, 45.Degrees()));

class Concentrativity(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Concentrativity));

class D063ZenosYaeGalvusStates : StateMachineBuilder
{
    public D063ZenosYaeGalvusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<ArtOfTheSwell1>()
            .ActivateOnEnter<ArtOfTheSwell2>()
            .ActivateOnEnter<UnmovingTroikaFirst>()
            .ActivateOnEnter<UnmovingTroikaSecond>()
            .ActivateOnEnter<UnmovingTroikaLast>()
            .ActivateOnEnter<ArtOfTheStorm1>()
            .ActivateOnEnter<ArtOfTheStorm2>()
            .ActivateOnEnter<ArtOfTheSword>()
            .ActivateOnEnter<Concentrativity>()
            .ActivateOnEnter<VeinSplitter1>()
            .ActivateOnEnter<VeinSplitter2>()
            .ActivateOnEnter<LightlessSparkAOE>()
            .ActivateOnEnter<LightlessSparkBaitaway>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 247, NameID = 6039)]
public class D063ZenosYaeGalvus(WorldState ws, Actor primary) : BossModule(ws, primary, new(250, -353), new ArenaBoundsSquare(22.5f))
{
    public static readonly ArenaBoundsCircle DefaultBounds = new(20);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.TheStorm).Concat([PrimaryActor]).Concat(Enemies(OID.TheSwell)).Concat(Enemies(OID.AmeNoHabakiri)));
    }
}
