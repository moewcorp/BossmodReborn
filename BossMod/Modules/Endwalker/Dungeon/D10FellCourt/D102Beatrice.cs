namespace BossMod.Endwalker.Dungeon.D10FellCourtOfTroia.D102Beatrice;

public enum OID : uint
{
    Boss = 0x396D, // R=4.95
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x2 (spawn during fight), EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1 (spawn during fight), EventObj type
    Helper = 0x233C, // R0.500, x26, 523 type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 29820, // Boss->location, no cast, single-target

    BeatificScorn1 = 29811, // Boss->self, 4.0s cast, single-target
    BeatificScorn2 = 29812, // Boss->self, no cast, single-target
    BeatificScorn3 = 29813, // Boss->self, 4.0s cast, single-target
    BeatificScorn4 = 29814, // Boss->self, no cast, single-target
    BeatificScorn5 = 29815, // Boss->self, no cast, single-target
    BeatificScorn6 = 29816, // Boss->self, no cast, single-target
    BeatificScornEnd = 29819, // Boss->self, no cast, single-target
    BeatificScornAOE = 29817, // Helper->self, 10.0s cast, range 9 circle

    DeathForeseen1 = 29821, // Helper->self, 5.0s cast, range 40 circle, gaze
    DeathForeseen2 = 29828, // Helper->self, 8.0s cast, range 40 circle, gaze

    EyeOfTroia = 29818, // Boss->self, 4.0s cast, range 40 circle, raidwide

    Hush = 29824, // Boss->player, 5.0s cast, single-target, tankbuster
    VoidNail = 29823, // Helper->player, 5.0s cast, range 6 circle, spread

    Voidshaker = 29822, // Boss->self, 5.0s cast, range 20 120-degree cone
    ToricVoidVisual1 = 29829, // Boss->self, 4.0s cast, single-target
    ToricVoidVisual2 = 31206, // Boss->self, no cast, single-target
    ToricVoid = 31207, // Helper->self, 4.0s cast, range 10-20 donut
    Antipressure = 31208, // Helper->player, 7.0s cast, range 6 circle, stack
}

public enum SID : uint
{
    Doom = 3364 // Helper->player, extra=0x0
}

class BeatificScorn5(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BeatificScornAOE), 9f, 8);

class DeathForeseen1(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.DeathForeseen1));
class DeathForeseen2(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.DeathForeseen2), maxCasts: 2);

class Voidshaker(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Voidshaker), new AOEShapeCone(20f, 60f.Degrees()));

class VoidNail(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.VoidNail), 6f);
class Hush(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Hush));
class EyeOfTroia(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.EyeOfTroia));
class ToricVoid(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ToricVoid), new AOEShapeDonut(10f, 20f));
class Antipressure(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.Antipressure), 6f, 4, 4);

class Doom(BossModule module) : BossComponent(module)
{

    private readonly List<Actor> _doomed = new(4);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Doom)
            _doomed.Add(actor);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Doom)
            _doomed.Remove(actor);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = _doomed.Count;
        if (count == 0)
            return;
        if (_doomed.Contains(actor) && !(actor.Role == Role.Healer || actor.Class == Class.BRD))
            hints.Add("You were doomed! Get cleansed fast.");
        if (!(actor.Role == Role.Healer || actor.Class == Class.BRD))
            return;
        if (_doomed.Contains(actor))
            hints.Add("Cleanse yourself! (Doom).");
        for (var i = 0; i < count; ++i)
        {
            var doom = _doomed[i];
            if (!_doomed.Contains(actor))
                hints.Add($"Cleanse {doom.Name} (Doom)");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _doomed.Count;
        if (count != 0)
            for (var i = 0; i < count; ++i)
            {
                var doom = _doomed[i];
                if (actor.Role == Role.Healer)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Esuna), doom, ActionQueue.Priority.High, castTime: 1);
                else if (actor.Class == Class.BRD)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(BRD.AID.WardensPaean), doom, ActionQueue.Priority.High);
            }
    }
}

class D102BeatriceStates : StateMachineBuilder
{
    public D102BeatriceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Doom>()
            .ActivateOnEnter<BeatificScorn5>()
            .ActivateOnEnter<DeathForeseen1>()
            .ActivateOnEnter<DeathForeseen2>()
            .ActivateOnEnter<Voidshaker>()
            .ActivateOnEnter<VoidNail>()
            .ActivateOnEnter<Hush>()
            .ActivateOnEnter<EyeOfTroia>()
            .ActivateOnEnter<ToricVoid>()
            .ActivateOnEnter<Antipressure>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 869, NameID = 11384, SortOrder = 5)]
public class D102Beatrice(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(default, -148f), 19.5f * CosPI.Pi32th, 32)], [new Rectangle(new(default, -127.63f), 20f, 1.25f),
    new Rectangle(new(default, -168.32f), 20f, 1.25f)]);
}
