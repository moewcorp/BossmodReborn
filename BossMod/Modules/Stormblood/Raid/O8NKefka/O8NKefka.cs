namespace BossMod.Stormblood.Raid.O8NKefka;

public enum OID : uint
{
    Boss = 0x2153, // R2.700, x1
    GravenImage = 0x18D6, // R0.500, x39, mixed types
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Hyperdrive = 10542, // Boss->player, 4.0s cast, single-target

    BlizzardBlitzVisual = 10519, // Boss->self, 5.0s cast, single-target
    BlizzardBlitzCircleFake1 = 10515, // GravenImage->self, 5.0s cast, range 10 circle
    BlizzardBlitzCircleFake2 = 10517, // GravenImage->self, 5.0s cast, range 40 circle
    BlizzardBlitzDonut1 = 10516, // GravenImage->self, 5.0s cast, range 10-40 donut
    BlizzardBlitzDonut2 = 10521, // GravenImage->self, 5.0s cast, range 10-40 donut
    BlizzardBlitzCircle1 = 10518, // GravenImage->self, 5.0s cast, range 10 circle
    BlizzardBlitzCircle2 = 10520, // GravenImage->self, 5.0s cast, range 10 circle

    ThrummingThunderVisual = 10522, // Boss->self, 5.0s cast, single-target
    ThrummingThunderFake = 10523, // GravenImage->self, 5.0s cast, range 40+R width 10 rect
    ThrummingThunder1 = 10524, // GravenImage->self, 5.0s cast, range 40+R width 10 rect
    ThrummingThunder2 = 10525, // GravenImage->self, 5.0s cast, range 40+R width 10 rect

    FlagrantFireVisual = 10526, // Boss->self, 4.2s cast, single-target
    FlagrantFireSpread = 10527, // GravenImage->players, 5.2s cast, range 5 circle
    FlagrantFireStack = 10528, // GravenImage->players, 5.2s cast, range 6 circle

    TimelyTeleportVisual1 = 10529, // Boss->self, 4.0s cast, single-target
    TimelyTeleportVisual2 = 10530, // GravenImage->self, 4.0s cast, range 6 circle
    RevoltingRuin = 10531, // Boss->self, no cast, range 100+R 120-degree cone
    AeroAssault = 10532, // Boss->self, 3.0s cast, range 100 circle
    GravenImage = 10533, // Boss->self, 5.0s cast, single-target
    Shockwave = 10535, // GravenImage->self, 13.0s cast, range 100+R width 40 rect
    WaveCannon = 10536, // GravenImage->self, 3.0s cast, range 70+R width 6 rect

    GravitationalWave = 10537, // GravenImage->self, 6.0s cast, range 100+R 180-degree cone, west half cleave
    IntemperateWill = 10538, // GravenImage->self, 6.0s cast, range 100+R 180-degree cone, east half cleave
    AveMaria = 10539, // GravenImage->self, 6.0s cast, range 100 circle, reverse gaze, must look at tower
    IndolentWill = 10540, // GravenImage->self, 6.0s cast, range 100 circle, gaze

    UltimaUpsurge = 10541, // Boss->self, 4.0s cast, range 100 circle
}

class Hyperdrive(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Hyperdrive));

abstract class BlizzardBlitzDonut(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeDonut(10, 40));
class BlizzardBlitzDonut1(BossModule module) : BlizzardBlitzDonut(module, AID.BlizzardBlitzDonut1);
class BlizzardBlitzDonut2(BossModule module) : BlizzardBlitzDonut(module, AID.BlizzardBlitzDonut2);

abstract class BlizzardBlitzCircle(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 10);
class BlizzardBlitzCircle1(BossModule module) : BlizzardBlitzCircle(module, AID.BlizzardBlitzCircle1);
class BlizzardBlitzCircle2(BossModule module) : BlizzardBlitzCircle(module, AID.BlizzardBlitzCircle2);

class FlagrantFireSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.FlagrantFireSpread), 5);
class FlagrantFireStack(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.FlagrantFireStack), 6, 8, 8);

abstract class ThrummingThunder(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(40, 5));
class ThrummingThunder1(BossModule module) : ThrummingThunder(module, AID.ThrummingThunder1);
class ThrummingThunder2(BossModule module) : ThrummingThunder(module, AID.ThrummingThunder2);

class UltimaUpsurge(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.UltimaUpsurge));

class AeroAssault(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.AeroAssault), 10)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
            hints.AddForbiddenZone(ShapeDistance.InvertedCone(c.Position, 15, Angle.FromDirection(Arena.Center - c.Position).Normalized(), 45.Degrees()), Module.CastFinishAt(c.CastInfo));
    }
}

class Shockwave(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Shockwave), 15, kind: Kind.DirForward)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
            hints.AddForbiddenZone(ShapeDistance.Cone(c.CastInfo!.Rotation.AlmostEqual(90.Degrees(), Angle.DegToRad) ? c.Position - new WDir(-33, 0) : c.Position - new WDir(33, 0), 40, c.Rotation, 135.Degrees()), Module.CastFinishAt(c.CastInfo));
    }
}

class WaveCannon(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WaveCannon), new AOEShapeRect(70, 3));

abstract class Cleave(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(100, 90.Degrees()));
class GravitationalWave(BossModule module) : Cleave(module, AID.GravitationalWave);
class IntemperateWill(BossModule module) : Cleave(module, AID.IntemperateWill);

class AveMaria(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.AveMaria), true);
class IndolentWill(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.IndolentWill));

class RevoltingRuin(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCone cone = new(102.7f, 60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TimelyTeleportVisual2 && !caster.Position.AlmostEqual(Module.PrimaryActor.Position, 1))
            _aoe = new(cone, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 3.1f));
        else if ((AID)spell.Action.ID == AID.AeroAssault) // sometimes it does AeroAssault directly and skipping Revolting Ruin
            _aoe = null;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RevoltingRuin)
            _aoe = null;
    }
}

class O8NKefkaStates : StateMachineBuilder
{
    public O8NKefkaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hyperdrive>()
            .ActivateOnEnter<BlizzardBlitzDonut1>()
            .ActivateOnEnter<BlizzardBlitzDonut2>()
            .ActivateOnEnter<BlizzardBlitzCircle1>()
            .ActivateOnEnter<BlizzardBlitzCircle2>()
            .ActivateOnEnter<FlagrantFireSpread>()
            .ActivateOnEnter<FlagrantFireStack>()
            .ActivateOnEnter<ThrummingThunder1>()
            .ActivateOnEnter<ThrummingThunder2>()
            .ActivateOnEnter<UltimaUpsurge>()
            .ActivateOnEnter<RevoltingRuin>()
            .ActivateOnEnter<AeroAssault>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<WaveCannon>()
            .ActivateOnEnter<GravitationalWave>()
            .ActivateOnEnter<IntemperateWill>()
            .ActivateOnEnter<AveMaria>()
            .ActivateOnEnter<IndolentWill>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (LTS, Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 289, NameID = 7131)]
public class O8NKefka(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsCircle(20));
