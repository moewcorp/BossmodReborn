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
class GlidingSwoop(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GlidingSwoop), new AOEShapeRect(18f, 8f));
class PeckingFlurry(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PeckingFlurry), "Raidwide (3x)");

class MarchingSamba(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private Actor? _caster;
    private static readonly AOEShapeCircle _circle = new(12f);
    private static readonly AOEShapeCone _cone = new(40f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_caster == null)
            return CollectionsMarshal.AsSpan(_aoes);
        var count = _aoes.Count;
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (aoe.Shape == _circle || aoe.Shape == _cone)
                aoes[i] = new(aoe.Shape, _caster.Position, _caster.Rotation, aoe.Activation, aoe.Color, aoe.Risky);
            else
                aoes[i] = aoe;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.MarchingSamba or (uint)AID.MesmerizingMarch)
        {
            _caster = caster;
            _aoes.Add(new(_circle, spell.LocXZ, caster.Rotation, WorldState.FutureTime(6.5d), Colors.Danger));
            _aoes.Add(new(_cone, spell.LocXZ, caster.Rotation, WorldState.FutureTime(8d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.MesmerizingMarch or (uint)AID.MesmerizingMarch2)
        {
            _aoes.RemoveAll(a => a.Shape == _circle);
            int idx = _aoes.FindIndex(a => a.Shape == _cone);
            if (idx != -1)
            {
                var c = _aoes[idx];
                _aoes[idx] = new(_cone, c.Origin, c.Rotation, c.Activation, Colors.Danger, true);
            }
        }
        else if (spell.Action.ID is (uint)AID.StirringSamba or (uint)AID.StirringSamba2)
        {
            _aoes.RemoveAll(a => a.Shape == _cone);
        }
    }
}

class PkuuchaStates : StateMachineBuilder
{
    public PkuuchaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GlidingSwoop>()
            .ActivateOnEnter<MarchingSamba>()
            .ActivateOnEnter<PeckingFlurry>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13443)]
public class Pkuucha(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
