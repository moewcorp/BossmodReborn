namespace BossMod.Stormblood.Dungeon.D06AlaMhigo.D062AulusMalAsina;

public enum OID : uint
{
    Boss = 0x1BA6, // R2.1
    PrototypeBit1 = 0x1BA7, // R0.6
    PrototypeBit2 = 0x1BA8, // R0.6
    EmptyVessel = 0x1BAF, // R0.5
    OutOfBodyPhase = 0x1DD8, // R3.0
    Helper = 0x18D6
}

public enum AID : uint
{
    AutoAttack = 9304, // Boss->player, no cast, single-target

    ManaBurst = 8271, // Boss->self, 3.0s cast, range 40 circle

    OrderToCharge = 8279, // Boss->self, 3.0s cast, single-target
    PrototypeBitVisual = 8275, // PrototypeBit1/PrototypeBit2->location, no cast, ???
    OrderToFire = 8280, // Boss->self, 3.0s cast, single-target
    AetherochemicalGrenado = 8282, // PrototypeBit1->location, 3.5s cast, range 8 circle
    IntegratedAetheromodulator = 8283, // PrototypeBit1->self, 3.0s cast, range 12-15 donut, -R inner and +R outer

    MagitekDisruptor = 8272, // Boss->self, 3.0s cast, range 40 circle, stuns raid
    MindjackPull = 8274, // Helper->player, no cast, single-target, pull 30, between centers
    MindjackVisual1 = 8273, // Helper->self, no cast, single-target
    MindjackVisual2 = 8270, // Boss->self, 3.0s cast, ???
    ReturnToBodyVisual = 8278, // Helper->EmptyVessel, no cast, single-target
    MagitekRay = 8276, // PrototypeBit2->self, 3.0s cast, range 45+R width 2 rect

    DemimagicksVisual = 8285, // Boss->self, 5.0s cast, single-target
    Demimagicks = 8286, // Helper->player, 5.0s cast, range 5 circle, spread
}

public enum TetherID : uint
{
    MindJack = 45 // EmptyVessel->player
}

class AetherochemicalGrenado(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AetherochemicalGrenado), 8);
class IntegratedAetheromodulator(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.IntegratedAetheromodulator), new AOEShapeDonut(11.4f, 15.6f));
class MagitekRay(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MagitekRay), new AOEShapeRect(45.6f, 1));
class ManaBurst(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ManaBurst));
class Demimagicks(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Demimagicks), 5);
class MindJack(BossModule module) : BossComponent(module)
{
    private readonly HashSet<(Actor source, Actor target)> tethers = [];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.MindJack)
            tethers.Add(new(source, WorldState.Actors.Find(tether.Target)!));
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.MindJack)
            tethers.Remove(new(source, WorldState.Actors.Find(tether.Target)!));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (tethers.Any(x => x.target == actor))
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(tethers.FirstOrDefault(x => x.target == actor).source.Position, 2));
    }
}

class D062AulusMalAsinaStates : StateMachineBuilder
{
    public D062AulusMalAsinaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AetherochemicalGrenado>()
            .ActivateOnEnter<IntegratedAetheromodulator>()
            .ActivateOnEnter<ManaBurst>()
            .ActivateOnEnter<MindJack>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<Demimagicks>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 247, NameID = 6038)]
public class D062AulusMalAsina(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Circle(new(250, -70), 19.75f)], [new Rectangle(new(250, -50), 20, 1.25f), new Rectangle(new(250, -90), 20, 1.25f)]);
}
