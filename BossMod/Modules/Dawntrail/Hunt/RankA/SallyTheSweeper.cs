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

class TargetedAdvance(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TargetedAdvance), 18f);

class CodeExecution(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(10f);
    private static readonly AOEShapeDonut donut = new(10f, 40f);
    private static readonly AOEShapeCross cross = new(40f, 5f, 45f.Degrees());

    private readonly List<AOEShape> _pendingShapes = [];
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CodeExecution1:
            case (uint)AID.CodeExecution5:
                UpdateAOEs(caster);
                break;
            case (uint)AID.ExecutionModel1:
                _pendingShapes.Add(circle);
                break;
            case (uint)AID.ExecutionModel2:
                _pendingShapes.Add(donut);
                break;
            case (uint)AID.ExecutionModel3:
                _pendingShapes.Add(cross);
                break;
            case (uint)AID.Reversal:
                _pendingShapes.Reverse();
                UpdateAOEs(caster);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_pendingShapes.Count != 0 &&
            spell.Action.ID is (uint)AID.CodeExecution1 or (uint)AID.CodeExecution2 or (uint)AID.CodeExecution3
            or (uint)AID.CodeExecution4 or (uint)AID.CodeExecution5 or (uint)AID.ReverseCode1 or (uint)AID.ReverseCode2 or (uint)AID.TargetedAdvance)
        {
            _pendingShapes.RemoveAt(0);
            UpdateAOEs(caster);
        }
    }

    private void UpdateAOEs(Actor actor)
    {
        _aoes.Clear();

        if (_pendingShapes.Count > 0)
        {
            _aoes.Add(new(_pendingShapes[0], actor.Position, actor.Rotation, WorldState.FutureTime(15d), Colors.Danger));

            if (_pendingShapes.Count > 1)
            {
                _aoes.Add(new(_pendingShapes[1], actor.Position, actor.Rotation, WorldState.FutureTime(15d), Risky: false));
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
