namespace BossMod.Stormblood.Dungeon.D13TheBurn.D132DefectiveDrone;

public enum OID : uint
{
    Boss = 0x23AA, // R3.2
    MiningDrone = 0x23AB, // R1.92
    RockBiter = 0x23AC, // R3.0
    RepurposedDreadnaught = 0x23AD, // R2.4
    SludgeVoidzone = 0x1EA9EF
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/RepurposedDreadnaught->player, no cast, single-target

    AetherochemicalFlame = 11635, // Boss->self, 3.0s cast, range 40 circle
    AetherochemicalResidue = 11636, // Boss->location, no cast, range 5 circle

    ThrottleVisual1 = 13526, // Boss->self, no cast, single-target
    ThrottleVisual2 = 13529, // MiningDrone->self, no cast, single-target
    ThrottleVisual3 = 13525, // Boss->self, no cast, single-target
    Throttle = 11638, // MiningDrone->location, 3.0s cast, width 3 rect charge
    FullThrottle = 11637, // Boss->location, 3.0s cast, width 5 rect charge

    SelfDetonate = 11639, // MiningDrone->self, 3.0s cast, range 5 circle, outside arena bounds
    AetherochemicalCoil = 11634, // Boss->player, 3.0s cast, single-target
    AditDriver = 11640 // RockBiter->self, 6.0s cast, range 30+R width 6 rect
}

public enum IconID : uint
{
    Baitaway = 2 // player
}

class Throttle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rectSmall = new(50, 1.5f);
    private static readonly AOEShapeRect rectBig = new(50, 2.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ThrottleVisual1)
        {
            var activation = WorldState.FutureTime(8.2f);
            foreach (var e in Module.Enemies(OID.MiningDrone).Where(x => x.ModelState.AnimState1 != 1))
                _aoes.Add(new(rectSmall, e.Position, e.Rotation, activation));
            var offset = _aoes[0].Origin.X < 0 ? -1 : 1;
            _aoes.Add(new(rectBig, new(offset * 18, -71.5f), -90.Degrees() * offset, activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FullThrottle)
            _aoes.Clear();
    }
}

class AetherochemicalFlame(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AetherochemicalFlame));
class AetherochemicalResidue(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(5), (uint)IconID.Baitaway, ActionID.MakeSpell(AID.AetherochemicalResidue), 4.1f, true);
class AditDriver(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AditDriver), new AOEShapeRect(33, 3));
class AetherochemicalCoil(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.AetherochemicalCoil));
class SludgeVoidzone(BossModule module) : Components.PersistentVoidzone(module, 2.5f, m => m.Enemies(OID.SludgeVoidzone).Where(z => z.EventState != 7));

class D132DefectiveDroneStates : StateMachineBuilder
{
    public D132DefectiveDroneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Throttle>()
            .ActivateOnEnter<AetherochemicalFlame>()
            .ActivateOnEnter<AetherochemicalResidue>()
            .ActivateOnEnter<AetherochemicalCoil>()
            .ActivateOnEnter<AditDriver>()
            .ActivateOnEnter<SludgeVoidzone>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 585, NameID = 7669)]
public class D132DefectiveDrone(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -70), new ArenaBoundsRect(10, 9.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.RepurposedDreadnaught));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.RepurposedDreadnaught => 1,
                _ => 0
            };
        }
    }
}
