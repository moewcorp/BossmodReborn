namespace BossMod.Dawntrail.Quest.Role.AnAntidoteForAnarchy;

public enum OID : uint
{
    Boss = 0x4257, // R1.5
    KokonowaTheCallous = 0x4258, // R0.5
    LoashkanaTheLeal = 0x4256, // R0.5
    KAModelMammet = 0x425A, // R0.15
    KRModelMammet = 0x4259, // R0.15
    KBModelMammet = 0x425B, // R0.15
    PoisonCloud = 0x425C, // R1.0-1.5
    SuffocatingCloud = 0x425D, // R1.3
    PoisonVoidzone = 0x1EBABD, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 37596, // Boss->LoashkanaTheLeal, no cast, single-target
    Teleport = 37615, // Boss/PoisonCloud->location, no cast, single-target

    TrickyTonic1 = 37597, // Boss->self, 3.0s cast, single-target
    TrickyTonic2 = 37598, // Boss->self, no cast, single-target

    StingingAnguishVisual = 37599, // Boss->self, 4.3+0,7s cast, single-target
    StingingAnguish = 37600, // Helper->self, 5.0s cast, range 60 circle, raidwide + esuna-able bleed
    StingOfTheScorpionVisual = 37610, // Boss->self, 4.3+0,7s cast, single-target
    StingOfTheScorpion = 37611, // Helper->self, 5.0s cast, range 60 circle, raidwide + esuna-able bleed
    ScornOfTheScorpionVisual = 37628, // Boss->self, 4.3+0,7s cast, single-target
    ScornOfTheScorpion = 37629, // Helper->self, 5.0s cast, range 60 circle, raidwide + esuna-able bleed

    StingingMaladyMarker = 37602, // Helper->player/LoashkanaTheLeal, no cast, single-target
    StingingMaladyVisual = 37601, // Boss->self, 4.3+0,7s cast, single-target
    StingingMalady = 37604, // Helper->self, 5.0s cast, range 50 60-degree cone
    StingingMaladyBait = 37603, // Helper->player/LoashkanaTheLeal, no cast, range 50 60-degree cone, apply hp recovery down
    StingingDeceptionVisual = 37621, // Boss->self, 4.3+0,7s cast, single-target
    StingingDeception = 37622, // Helper->self, 5.0s cast, range 40 circle, applies delusions

    BewilderingBlightVisual = 37613, // Boss->self, 4.3+0,7s cast, single-target
    BewilderingBlight = 37614, // Helper->player/LoashkanaTheLeal, 5.0s cast, range 6 circle, spread, apply temporary misdirection

    SkineaterSurgeVisual1 = 37616, // Boss->self, 7.3+0,7s cast, single-target
    SkineaterSurgeVisual2 = 37623, // Boss->self, 7.3+0,7s cast, single-target
    SkineaterSurge1 = 37617, // Helper->self, 8.0s cast, range 40 180-degree cone
    SkineaterSurge2 = 37624, // Helper->self, 8.0s cast, range 40 180-degree cone
    Burst1 = 37612, // PoisonCloud->self, 5.0s cast, range 10 circle
    Burst2 = 37626, // PoisonCloud->self, no cast, range 10 circle
    SkineaterSurgePoisonCloud = 37625, // PoisonCloud->self, 8.0s cast, range 10 circle

    MammetFort = 37618, // KAModelMammet/KRModelMammet->self, 3.0s cast, ???
    SelfDestruct = 37619, // KAModelMammet/KRModelMammet->self, 3.0s cast, range 5 circle
    SelfDestructBig = 37620, // KRModelMammet->self, 15.0s cast, range 50 circle
    SelfDestructKBMammet = 37627, // KBModelMammet->self, 8.0s cast, range 7 circle

    Fester = 37609, // Boss->LoashkanaTheLeal, 5.0s cast, single-target, tankbuster

    TriDisasterFirst = 37630, // Boss->LoashkanaTheLeal, 5.0s cast, range 5 circle, stack x5
    TriDisasterRest = 37631 // Boss->LoashkanaTheLeal, no cast, range 5 circle
}

public enum SID : uint
{
    Bleeding = 3966, // Helper->LoashkanaTheLeal/player, extra=0x1
    HPRecoveryDown = 3967, // Helper->player/LoashkanaTheLeal, extra=0x0
    TemporaryMisdirection = 1422, // Helper->player, extra=0x168
    TemporaryMisdirectionNPC = 2936 // Helper->LoashkanaTheLeal, extra=0x168
}

class StingingMalady(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.StingingMalady), StingingMaladyBait.Cone);
class StingingMaladyBait(BossModule module) : Components.GenericBaitAway(module)
{
    public static readonly AOEShapeCone Cone = new(50f, 30f.Degrees());

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.StingingMaladyMarker)
            CurrentBaits.Add(new(caster, WorldState.Actors.Find(spell.MainTargetID)!, Cone, WorldState.FutureTime(5d)));
        else if (spell.Action.ID == (uint)AID.StingingMaladyBait)
            CurrentBaits.Clear();
    }
}

class BewilderingBlight(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.BewilderingBlight), 6f);
class BewilderingBlightTM(BossModule module) : Components.TemporaryMisdirection(module, ActionID.MakeSpell(AID.BewilderingBlight));

abstract class SkineaterSurge(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(40f, 90f.Degrees()));
class SkineaterSurge1(BossModule module) : SkineaterSurge(module, AID.SkineaterSurge1);
class SkineaterSurge2(BossModule module) : SkineaterSurge(module, AID.SkineaterSurge2);

abstract class PoisonCloud(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 10f);
class Burst(BossModule module) : PoisonCloud(module, AID.Burst1);
class SkineaterSurgePoisonCloud(BossModule module) : PoisonCloud(module, AID.SkineaterSurgePoisonCloud);

class SelfDestruct(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SelfDestruct), 5f);
class SelfDestructKBMammet(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SelfDestructKBMammet), 7f, 8);
class Fester(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Fester));

class TriDisaster(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.TriDisasterFirst), 5f, 2, 2)
{
    private int numCasts;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell) { }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TriDisasterRest)
        {
            if (++numCasts == 4)
                Stacks.Clear();
        }
    }
}

class SuffocatingCloud(BossModule module) : Components.PersistentVoidzone(module, 9f, GetVoidzone)
{
    private static List<Actor> GetVoidzone(BossModule module) => module.Enemies((uint)OID.PoisonVoidzone);
}

abstract class Raidwides(BossModule module, AID aid) : Components.RaidwideCast(module, ActionID.MakeSpell(aid), "Raidwide + bleed (Esuna!)");
class StingingAnguish(BossModule module) : Raidwides(module, AID.StingingAnguish);
class StingOfTheScorpion(BossModule module) : Raidwides(module, AID.StingOfTheScorpion);
class ScornOfTheScorpion(BossModule module) : Raidwides(module, AID.ScornOfTheScorpion);

class Esuna(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _affected = new(2);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID is (uint)SID.Bleeding or (uint)SID.HPRecoveryDown)
            _affected.Add(actor);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID is (uint)SID.Bleeding or (uint)SID.HPRecoveryDown)
            _affected.Remove(actor);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var loashkanaL = Module.Enemies(OID.LoashkanaTheLeal);
        var loashkana = loashkanaL.Count != 0 ? loashkanaL[0] : null;
        if (_affected.Contains(actor))
            hints.Add("Use Esuna on yourself.");
        if (loashkana != null && _affected.Contains(loashkana))
            hints.Add($"Use Esuna on {loashkana.Name}!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _affected.Count;
        for (var i = 0; i < count; ++i)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Esuna), _affected[i], ActionQueue.Priority.High);
    }
}

class AnAntidoteForAnarchyStates : StateMachineBuilder
{
    public AnAntidoteForAnarchyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StingingMalady>()
            .ActivateOnEnter<StingingMaladyBait>()
            .ActivateOnEnter<BewilderingBlight>()
            .ActivateOnEnter<BewilderingBlightTM>()
            .ActivateOnEnter<SkineaterSurge1>()
            .ActivateOnEnter<SkineaterSurge2>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<SkineaterSurgePoisonCloud>()
            .ActivateOnEnter<SelfDestruct>()
            .ActivateOnEnter<SelfDestructKBMammet>()
            .ActivateOnEnter<Fester>()
            .ActivateOnEnter<TriDisaster>()
            .ActivateOnEnter<SuffocatingCloud>()
            .ActivateOnEnter<StingingAnguish>()
            .ActivateOnEnter<ScornOfTheScorpion>()
            .ActivateOnEnter<StingOfTheScorpion>()
            .ActivateOnEnter<Esuna>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70365, NameID = 12743)]
public class AnAntidoteForAnarchy(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(-5.65f, -84.73f), 14.5f, 20)]);
    private static readonly uint[] all = [(uint)OID.Boss, (uint)OID.KAModelMammet, (uint)OID.KRModelMammet, (uint)OID.SuffocatingCloud, (uint)OID.PoisonCloud];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(all));
    }
}
