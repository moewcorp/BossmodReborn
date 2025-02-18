namespace BossMod.Dawntrail.Hunt.RankA.SallyTheSweeper;

public enum OID : uint
{
    Boss = 0x4395 // R6.0
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    ExecutionModel1 = 38454, // Boss->self, 2.0s cast, range 10 circle
    ExecutionModel2 = 38455, // Boss->self, 2.0s cast, range 10-40 donut
    ExecutionModel3 = 38456, // Boss->self, 2.0s cast, range 40 width 10 cross
    CodeExecution1 = 38457, // Boss->self, 5.0s cast, range 10 circle
    CodeExecution2 = 38462, // Boss->self, no cast, range -40 donut
    CodeExecution3 = 38463, // Boss->self, no cast, range 40 width 10 cross
    CodeExecution4 = 38461, // Boss->self, no cast, range 10 circle
    CodeExecution5 = 38458, // Boss->self, 5.0s cast, range 10-40 donut
    TargetedAdvance = 38466, // Boss->location, 7.0s cast, range 18 circle
    Reversal = 40056, // Boss->self, 5.0s cast, single-target
    ReverseCode1 = 38460, // Boss->self, 5.0s cast
    ReverseCode2 = 38459, // Boss->self, 5.0s cast
}

class TargetedAdvance(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TargetedAdvance), 18);

class CodeExecution(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(10);
    private static readonly AOEShapeDonut donut = new(10, 40);
    private static readonly AOEShapeCross cross = new(40, 5, 45.Degrees());

    private readonly List<AOEShape> _pendingShapes = [];
    private readonly List<AOEInstance> _activeAOEs = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _activeAOEs;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (caster != Module.PrimaryActor)
            return;

        switch ((AID)spell.Action.ID)
        {
            case AID.CodeExecution1:
            case AID.CodeExecution5:
                UpdateAOEs(caster);
                break;
            case AID.ExecutionModel1:
                _pendingShapes.Add(circle);
                break;
            case AID.ExecutionModel2:
                _pendingShapes.Add(donut);
                break;
            case AID.ExecutionModel3:
                _pendingShapes.Add(cross);
                break;
            case AID.Reversal:
                _pendingShapes.Reverse();
                UpdateAOEs(caster);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (caster == Module.PrimaryActor && _pendingShapes.Count > 0 &&
            (AID)spell.Action.ID is AID.CodeExecution1 or AID.CodeExecution2 or AID.CodeExecution3 or AID.CodeExecution4 or AID.CodeExecution5 or AID.ReverseCode1 or AID.ReverseCode2 or AID.TargetedAdvance)
        {
            _pendingShapes.RemoveAt(0);
            UpdateAOEs(caster);
        }
    }

    private void UpdateAOEs(Actor actor)
    {
        _activeAOEs.Clear();

        if (_pendingShapes.Count > 0)
        {
            _activeAOEs.Add(new(_pendingShapes[0], actor.Position, actor.Rotation, WorldState.FutureTime(15), Colors.Danger));

            if (_pendingShapes.Count > 1)
            {
                _activeAOEs.Add(new(_pendingShapes[1], actor.Position, actor.Rotation, WorldState.FutureTime(15), Risky: false));
            }
        }
    }
}

class SallyTheSweeperStates : StateMachineBuilder
{
    public SallyTheSweeperStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CodeExecution>()
            .ActivateOnEnter<TargetedAdvance>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13435)]
public class SallyTheSweeper(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
