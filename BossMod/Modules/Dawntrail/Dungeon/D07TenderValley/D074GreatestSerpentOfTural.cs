namespace BossMod.Dawntrail.Dungeon.D07TenderValley.D074GreatestSerpentOfTural;

public enum OID : uint
{
    Boss = 0x4164, // R4.5
    LesserSerpentOfTural = 0x41DE, // R2.812
    GreatSerpentOfTural = 0x41E0, // R1.152-3.84
    SludgeVoidzone1 = 0x1EBA86, // R0.5
    SludgeVoidzone2 = 0x1EBA87, // R0.5
    SludgeVoidzone3 = 0x1EBA88, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 36747, // Boss->location, no cast, single-target

    DubiousTulidisaster = 36748, // Boss->self, 5.0s cast, range 40 circle

    BouncyCouncil = 36746, // Boss->self, 3.0s cast, single-target, spawns clones

    MisplacedMystery = 36750, // LesserSerpentOfTural->self, 7.0s cast, range 52 width 5 rect
    ExaltedWobble = 36749, // LesserSerpentOfTural->self, 7.0s cast, range 9 circle

    ScreesOfFuryVisual = 36744, // Boss->self, 4.5+0.5s cast, single-target, AOE tankbuster
    ScreesOfFury = 36757, // Helper->player, no cast, range 3 circle 

    GreatestLabyrinth = 36745, // Boss->self, 4.0s cast, range 40 circle

    MoistSummoning = 36743, // Boss->self, 3.0s cast, single-target, spawns great serpent of tural
    MightyBlorpVisual1 = 36753, // GreatSerpentOfTural->self, 4.5+0.5s cast, single-target, stack
    MightyBlorpVisual2 = 36752, // GreatSerpentOfTural->self, 4.5+0.5s cast, single-target, stack
    MightyBlorpVisual3 = 36751, // GreatSerpentOfTural->self, 4.5+0.5s cast, single-target, stack
    MightyBlorp1 = 39983, // GreatSerpentOfTural->players, no cast, range 6 circle
    MightyBlorp2 = 39982, // GreatSerpentOfTural->players, no cast, range 5 circle
    MightyBlorp3 = 39981, // GreatSerpentOfTural->players, no cast, range 4 circle

    GreatestFloodVisual = 36742, // Boss->self, 5.0s cast, single-target
    GreatestFlood = 36756, // Helper->self, 6.0s cast, range 40 circle, knockback 15, away from source

    GreatTorrentVisual = 36741, // Boss->self, 3.0s cast, single-target
    GreatTorrentAOE = 36754, // Helper->location, 6.0s cast, range 6 circle 
    GreatTorrentSpread = 36755 // Helper->player, no cast, range 6 circle
}

public enum IconID : uint
{
    Tankbuster = 341, // player
    LabyrinthFail = 504, // player
    LabyrinthSuccess = 503, // player
    Stackmarker1 = 62, // player
    Stackmarker2 = 542, // player
    Stackmarker3 = 543, // player
    Spreadmarker = 139 // player
}

class DubiousTulidisasterArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(D074GreatestSerpentOfTural.ArenaCenter, 15)], [new Square(D074GreatestSerpentOfTural.ArenaCenter, 12)]);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DubiousTulidisaster && Arena.Bounds == D074GreatestSerpentOfTural.StartingBounds)
            _aoe = new(square, Arena.Center, default, Module.CastFinishAt(spell, 4.8f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x00)
        {
            Arena.Bounds = D074GreatestSerpentOfTural.DefaultBounds;
            _aoe = null;
        }
    }
}

class ScreesOfFury(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(3), (uint)IconID.Tankbuster, ActionID.MakeSpell(AID.ScreesOfFury), 5.3f, true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

class GreatestFlood(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.GreatestFlood), 15)
{
    private static readonly Angle a45 = 45.Degrees();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var source = Sources(slot, actor).FirstOrDefault();
        if (source != default)
            hints.AddForbiddenZone(ShapeDistance.InvertedCone(source.Origin, 4, source.Direction, a45), source.Activation);
    }
}

class GreatestLabyrinth(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly WPos center = new(-130, -554);
    private const string TileHint = "Walk onto safe square!";
    private static readonly Square middle = new(center, 4);
    private const int Radius = 2;

    private static readonly (Square correctTile, Square goalTile)[] tilePairs = [
        (new(new(-124, -552), Radius), new(new(-140, -564), Radius)),
        (new(new(-128, -560), Radius), new(new(-120, -544), Radius)),
        (new(new(-132, -548), Radius), new(new(-120, -564), Radius)),
        (new(new(-136, -556), Radius), new(new(-140, -544), Radius))];

    private static readonly List<Shape> wholeArena = [new Square(center, 12)];
    private static readonly AOEShapeCustom[] forbiddenShapes = [.. tilePairs.Select(tp => new AOEShapeCustom(wholeArena, [middle, tp.correctTile, tp.goalTile]))];
    private static readonly AOEShapeCustom[] safeShapes = [.. tilePairs.Select(tp => new AOEShapeCustom([tp.correctTile, tp.goalTile], InvertForbiddenZone: true))];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index != 0x01)
            return;

        var activation = WorldState.FutureTime(10);

        switch (state)
        {
            case 0x01000080:
                AddAOEs(0, activation);
                break;
            case 0x04000200:
                AddAOEs(1, activation);
                break;
            case 0x10000800:
                AddAOEs(2, activation);
                break;
            case 0x00020001:
                AddAOEs(3, activation);
                break;
            case 0x00100004 or 0x00200004 or 0x00400004 or 0x00080004:
                _aoes.Clear();
                break;
        }
    }

    private void AddAOEs(int index, DateTime activation)
    {
        _aoes.Add(new(forbiddenShapes[index], center));
        _aoes.Add(new(safeShapes[index], center, default, activation, Colors.SafeFromAOE));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var activeSafeShapes = new[] { safeShapes[0], safeShapes[1], safeShapes[2], safeShapes[3] };

        if (ActiveAOEs(slot, actor).Any(c => activeSafeShapes.Contains(c.Shape) && !c.Check(actor.Position)))
            hints.Add(TileHint);
        else if (ActiveAOEs(slot, actor).Any(c => activeSafeShapes.Contains(c.Shape) && c.Check(actor.Position)))
            hints.Add(TileHint, false);
    }
}

class MightyBlorp(BossModule module, IconID iconID, AID aid, int radius) : Components.StackWithIcon(module, (uint)iconID, ActionID.MakeSpell(aid), radius, 4.6f, 4, 4);
class MightyBlorp1(BossModule module) : MightyBlorp(module, IconID.Stackmarker1, AID.MightyBlorp1, 6);
class MightyBlorp2(BossModule module) : MightyBlorp(module, IconID.Stackmarker2, AID.MightyBlorp2, 5);
class MightyBlorp3(BossModule module) : MightyBlorp(module, IconID.Stackmarker3, AID.MightyBlorp3, 4);

class SludgeVoidzone(BossModule module, int radius, OID oid) : Components.PersistentVoidzone(module, radius, m => m.Enemies(oid).Where(z => z.EventState != 7));
class SludgeVoidzone1(BossModule module) : SludgeVoidzone(module, 6, OID.SludgeVoidzone1);
class SludgeVoidzone2(BossModule module) : SludgeVoidzone(module, 5, OID.SludgeVoidzone2);
class SludgeVoidzone3(BossModule module) : SludgeVoidzone(module, 4, OID.SludgeVoidzone3);

class DubiousTulidisaster(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DubiousTulidisaster));
class GreatestLabyrinthRaidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.GreatestLabyrinth));
class GreatestFloodRaidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.GreatestFlood));
class ExaltedWobble(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ExaltedWobble), new AOEShapeCircle(9));
class MisplacedMystery(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MisplacedMystery), new AOEShapeRect(25.5f, 2.5f, 25.5f));
class GreatTorrent(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.GreatTorrentAOE), 6, maxCasts: 10);
class GreatTorrentSpread(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, ActionID.MakeSpell(AID.GreatTorrentSpread), 6, 5.1f);

class D074GreatestSerpentOfTuralStates : StateMachineBuilder
{
    public D074GreatestSerpentOfTuralStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Components.StayInBounds>()
            .ActivateOnEnter<DubiousTulidisasterArenaChange>()
            .ActivateOnEnter<DubiousTulidisaster>()
            .ActivateOnEnter<ScreesOfFury>()
            .ActivateOnEnter<MightyBlorp1>()
            .ActivateOnEnter<MightyBlorp2>()
            .ActivateOnEnter<MightyBlorp3>()
            .ActivateOnEnter<SludgeVoidzone1>()
            .ActivateOnEnter<SludgeVoidzone2>()
            .ActivateOnEnter<SludgeVoidzone3>()
            .ActivateOnEnter<GreatestFlood>()
            .ActivateOnEnter<GreatestFloodRaidwide>()
            .ActivateOnEnter<GreatestLabyrinth>()
            .ActivateOnEnter<GreatestLabyrinthRaidwide>()
            .ActivateOnEnter<ExaltedWobble>()
            .ActivateOnEnter<MisplacedMystery>()
            .ActivateOnEnter<GreatTorrent>()
            .ActivateOnEnter<GreatTorrentSpread>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 834, NameID = 12709)]
public class D074GreatestSerpentOfTural(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(-130, -554);
    public static readonly ArenaBoundsSquare StartingBounds = new(14.5f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(12);
}
