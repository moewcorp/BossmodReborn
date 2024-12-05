namespace BossMod.Dawntrail.TreasureHunt.CenoteJaJaGural.GoldenMolter;

public enum OID : uint
{
    Boss = 0x4306, // R4.8
    VasoconstrictorPool = 0x1EBCB8, // R0.5

    TuraliOnion = 0x4300, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    TuraliEggplant = 0x4301, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    TuraliGarlic = 0x4302, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    TuraliTomato = 0x4303, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    TuligoraQueen = 0x4304, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    UolonOfFortune = 0x42FF, // R3.5
    AlpacaOfFortune = 0x42FE, // R1.800, x0 (spawn during fight)

    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // UolonOfFortune->player, no cast, single-target
    Teleport = 38264, // Boss->location, no cast, single-target

    Lap = 38274, // Boss->player, 5s cast, single-target tankbuster

    LightburstVisual = 38276, // Helper->location, no cast, range 40 circle visual
    Lightburst = 38275, // Boss->self, 5s cast, single-target raidwide

    Crypsis1 = 38265, // Boss->self, 3s cast, single-target
    Crypsis2 = 38266, // Boss->self, 3s cast, single-target
    GoldenGall = 38267, // Boss->self, 7s cast, range 40 180-degree cone

    GoldenRadianceVisual = 38272, // Boss->self, 2.7+0.3s cast, single-target visual
    GoldenRadiance = 38273, // Helper->location, 3s cast, range 5 circle

    BlindingLightVisual = 38248, // Boss->self, no cast, single-target
    BlindingLight = 38580, // Helper->players, 5s cast, range 6 circle spread

    AetherialLightVisual = 38270, // Boss->self, 4.7+0.3s cast, single-target visual
    AetherialLight = 38271, // Helper->self, 5s cast, range 40 60-degree cone

    VasoconstrictorVisual = 38269, // Boss->location, no cast, range 6 circle visual
    Vasoconstrictor1 = 38335, // Helper->self, 5.7s cast, range 6 circle
    Vasoconstrictor2 = 38336, // Helper->self, 7.3s cast, range 6 circle
    Vasoconstrictor3 = 38337, // Helper->self, 9.3s cast, range 6 circle
    VasoconstrictorPool = 38268, // Boss->location, 4+0.6s cast, range 6 circle

    Inhale = 38280, // UolonOfFortune->self, 0.5s cast, range 27 120-degree cone
    Spin = 38279, // UolonOfFortune->self, 3.0s cast, range 11 circle
    TearyTwirl = 32301, // TuraliOnion->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // TuraliTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // TuraliGarlic->self, 3.5s cast, range 7 circle
    PluckAndPrune = 32302, // TuraliEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // TuligoraQueen->self, 3.5s cast, range 7 circle
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus adds disappear
}

public enum SID : uint
{
    Concealed = 3997, // Boss->Boss, extra=0x2
    Slime = 569, // Boss->player, extra=0x0
}

class Lap(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Lap));
class Lightburst(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Lightburst));

class Crypsis(BossModule module) : BossComponent(module)
{
    private bool IsConcealed;
    private const int RevealDistance = 9;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (IsConcealed)
        {
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), actor, ActionQueue.Priority.High);
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Module.PrimaryActor.Position, RevealDistance));
        }
    }
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (IsConcealed)
            Arena.AddCircle(Module.PrimaryActor.Position, RevealDistance, Colors.Safe);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Crypsis1 or AID.Crypsis2)
            IsConcealed = true;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Concealed)
            IsConcealed = false;
    }
}

class GoldenGall(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GoldenGall), new AOEShapeCone(40, 90.Degrees()));
class GoldenRadiance(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.GoldenRadiance), 5);
class BlindingLight(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.BlindingLight), 6);

class AetherialLight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AetherialLight), new AOEShapeCone(40, 30.Degrees()), 4)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => ActiveCasters.Select((c, i) => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo), i < 2 ? Colors.Danger : Colors.AOE));
}

abstract class Vasoconstrictor(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(6));
class Vasoconstrictor1(BossModule module) : Vasoconstrictor(module, AID.Vasoconstrictor1);
class Vasoconstrictor2(BossModule module) : Vasoconstrictor(module, AID.Vasoconstrictor2);
class Vasoconstrictor3(BossModule module) : Vasoconstrictor(module, AID.Vasoconstrictor3);

class VasoconstrictorPool(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeCircle circle = new(17);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorEState(Actor actor, ushort state)
    {
        if (state == 0x004)
            _aoes.RemoveAll(x => x.Origin == actor.Position);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.VasoconstrictorPool)
            _aoes.Add(new(circle, actor.Position));
    }
}

class Spin(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCircle(11));

abstract class Mandragoras(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(7));
class PluckAndPrune(BossModule module) : Mandragoras(module, AID.PluckAndPrune);
class TearyTwirl(BossModule module) : Mandragoras(module, AID.TearyTwirl);
class HeirloomScream(BossModule module) : Mandragoras(module, AID.HeirloomScream);
class PungentPirouette(BossModule module) : Mandragoras(module, AID.PungentPirouette);
class Pollen(BossModule module) : Mandragoras(module, AID.Pollen);

class GoldenMolterStates : StateMachineBuilder
{
    public GoldenMolterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Lap>()
            .ActivateOnEnter<Lightburst>()
            .ActivateOnEnter<Crypsis>()
            .ActivateOnEnter<GoldenGall>()
            .ActivateOnEnter<GoldenRadiance>()
            .ActivateOnEnter<BlindingLight>()
            .ActivateOnEnter<AetherialLight>()
            .ActivateOnEnter<Vasoconstrictor1>()
            .ActivateOnEnter<Vasoconstrictor2>()
            .ActivateOnEnter<Vasoconstrictor3>()
            .ActivateOnEnter<VasoconstrictorPool>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => module.Enemies(GoldenMolter.All).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Kismet, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 993, NameID = 13248)]
public class GoldenMolter(WorldState ws, Actor primary) : SharedBounds(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.TuraliEggplant, (uint)OID.TuraliTomato, (uint)OID.TuligoraQueen, (uint)OID.TuraliGarlic,
    (uint)OID.TuraliOnion, (uint)OID.UolonOfFortune, (uint)OID.AlpacaOfFortune];
    public static readonly uint[] All = [(uint)OID.Boss, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, allowDeadAndUntargetable: true);
        Arena.Actors(Enemies(bonusAdds), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.TuraliOnion => 5,
                OID.TuraliEggplant => 4,
                OID.TuraliGarlic => 3,
                OID.TuraliTomato or OID.AlpacaOfFortune => 2,
                OID.TuligoraQueen or OID.UolonOfFortune => 1,
                _ => 0
            };
        }
    }
}
