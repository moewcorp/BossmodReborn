namespace BossMod.Dawntrail.Hunt.RankA.Pkuucha;

public enum OID : uint
{
    Boss = 0x4580 // R4.34
}

public enum AID : uint
{
    AutoAttack = 872, //  Boss->player, no cast, single-target
    MesmerizingMarch = 39863, //  Boss->self, 4.0s cast, range 12 circle
    StirringSamba = 39864, //  Boss->self, 4.0s cast, range 40 90.000-degree cone
    GlidingSwoop = 39757, //  Boss->self, 3.5s cast, range 18 width 16 rect
    MarchingSamba = 39797, //  Boss->self, 5.0s cast, single-target
    MesmerizingMarch2 = 39755, //  Boss->self, 1.5s cast, range 12 circle
    StirringSamba2 = 39756, //  Boss->self, 1.0s cast, range 40 90.000-degree cone
    PeckingFlurry = 39760, //  Boss->self, 5.0s cast, range 40 circle
    PeckingFlurry2 = 39761, // Boss->self, no cast, range 40 circle
}

class MesmerizingMarch(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MesmerizingMarch), new AOEShapeCircle(12));
class StirringSamba(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.StirringSamba), new AOEShapeCone(40, 90.Degrees()));
class GlidingSwoop(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GlidingSwoop), new AOEShapeRect(18, 8));
class MarchingSambaHint(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.MarchingSamba), "Get out, then behind!");

class MarchingSamba(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _caster;
    private readonly List<AOEInstance> _activeAOEs = [];
    private static readonly AOEShapeCircle _shapeCircle = new(12);
    private static readonly AOEShapeCone _shapeCone = new(40, 90.Degrees());
    private DateTime _castStartTime;
    private bool _circleDangerSet;
    private bool _coneDrawn;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_caster == null)
            yield break;

        if (!_circleDangerSet && WorldState.CurrentTime >= _castStartTime.AddSeconds(4))
        {
            _activeAOEs[0] = new(_shapeCircle, _caster.Position, default, _castStartTime.AddSeconds(4), Colors.Danger);
            _circleDangerSet = true;
        }

        if (!_coneDrawn && WorldState.CurrentTime >= _castStartTime.AddSeconds(7))
        {
            _activeAOEs.Add(new(_shapeCone, _caster.Position, _caster.Rotation, _castStartTime.AddSeconds(8), Colors.Danger));
            _coneDrawn = true;
        }

        foreach (var aoe in _activeAOEs)
        {
            yield return aoe;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.MarchingSamba)
            return;

        _caster = caster;
        _castStartTime = WorldState.CurrentTime;
        _circleDangerSet = false;
        _coneDrawn = false;
        _activeAOEs.Add(new(_shapeCircle, _caster.Position, default, _castStartTime.AddSeconds(10), Risky: false));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MesmerizingMarch2)
        {
            _activeAOEs.RemoveAll(aoe => aoe.Shape == _shapeCircle);
        }
        else if (spell.Action.ID == (uint)AID.StirringSamba2)
        {
            _activeAOEs.Clear();
            _caster = null;
            _circleDangerSet = false;
            _coneDrawn = false;
            _castStartTime = DateTime.MinValue;
        }
    }
}

class PeckingFlurry(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PeckingFlurry), "Raidwide x3!");

class PkuuchaStates : StateMachineBuilder
{
    public PkuuchaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MesmerizingMarch>()
            .ActivateOnEnter<StirringSamba>()
            .ActivateOnEnter<GlidingSwoop>()
            .ActivateOnEnter<MarchingSambaHint>()
            .ActivateOnEnter<MarchingSamba>()
            .ActivateOnEnter<PeckingFlurry>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13443)]
public class Pkuucha(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
