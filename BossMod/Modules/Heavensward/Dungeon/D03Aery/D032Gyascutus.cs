namespace BossMod.Heavensward.Dungeon.D03Aery.D032Gyascutus;

public enum OID : uint
{
    Boss = 0x3970, // R5.4
    InflammableFumes = 0x3972, // R1.2
    Helper = 0x233C
}

public enum AID : uint
{
    Attack = 872, // Boss->player, no cast, single-target

    InflammableFumesVisual = 31232, // InflammableFumes->location, no cast, single-target
    InflammableFumes = 30181, // Boss->self, 4.0s cast, single-target
    BurstVisual = 30183, // InflammableFumes->self, no cast, single-target
    Burst = 30184, // Helper->self, 10.0s cast, range 10 circle
    DeafeningBellow = 31233, // Boss->self, 4.0s cast, range 55 circle

    ProximityPyre = 30191, // Boss->self, 4.0s cast, range 12 circle
    AshenOuroboros = 30190, // Boss->self, 8.0s cast, range 11-20 donut
    BodySlam = 31234, // Boss->self, 4.0s cast, range 30 circle, knockback 10, away from source
    CripplingBlow = 30193, // Boss->player, 5.0s cast, single-target
}

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(19.9f, 27);
    private static readonly ArenaBoundsCircle defaultbounds = new(19.9f);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.InflammableFumes && Arena.Bounds == D032Gyascutus.StartingBounds)
            _aoe = new(donut, D032Gyascutus.ArenaCenter, default, Module.CastFinishAt(spell, 0.8f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x00)
        {
            Arena.Bounds = defaultbounds;
            _aoe = null;
        }
    }
}

class ProximityPyre(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ProximityPyre), 12);
class Burst(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Burst), 10);
class CripplingBlow(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.CripplingBlow));
class DeafeningBellow(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DeafeningBellow));
class AshenOuroboros(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AshenOuroboros), new AOEShapeDonut(11, 20));
class BodySlam(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.BodySlam), 10)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var source = Sources(slot, actor).FirstOrDefault();
        if (source != default)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 9), source.Activation);
    }
}

class D032GyascutusStates : StateMachineBuilder
{
    public D032GyascutusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<ProximityPyre>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<DeafeningBellow>()
            .ActivateOnEnter<AshenOuroboros>()
            .ActivateOnEnter<BodySlam>()
            .ActivateOnEnter<CripplingBlow>();

    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 39, NameID = 3455)]
public class D032Gyascutus(WorldState ws, Actor primary) : BossModule(ws, primary, StartingBounds.Center, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(11.978f, 67.979f);
    public static readonly ArenaBoundsComplex StartingBounds = new([new Circle(ArenaCenter, 26.5f)], [new Rectangle(new(38.805f, 66.371f), 20, 0.8f, 86.104f.Degrees()), new Rectangle(new(-11.441f, 56.27f), 20, 0.9f, -64.554f.Degrees()), new Circle(new(-16.5f, 66.1f), 2.5f)]);
}
