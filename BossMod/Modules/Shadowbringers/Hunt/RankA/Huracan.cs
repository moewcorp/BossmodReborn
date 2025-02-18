namespace BossMod.Shadowbringers.Hunt.RankA.Huracan;

public enum OID : uint
{
    Boss = 0x28B5 // R=4.9
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    WindsEnd = 17494, // Boss->player, no cast, single-target
    WinterRain = 17497, // Boss->location, 4.0s cast, range 6 circle
    Windburst = 18042, // Boss->self, no cast, range 80 width 10 rect
    SummerHeat = 17499, // Boss->self, 4.0s cast, range 40 circle
    DawnsEdge = 17495, // Boss->self, 3.5s cast, range 15 width 10 rect
    SpringBreeze = 17496, // Boss->self, 3.5s cast, range 80 width 10 rect
    AutumnWreath = 17498 // Boss->self, 4.0s cast, range 10-20 donut
}

class SpringBreeze(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SpringBreeze), new AOEShapeRect(80, 5));
class SummerHeat(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SummerHeat));

class Combos(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(10, 20);
    private static readonly AOEShapeCircle circle = new(6);
    private static readonly AOEShapeRect rect = new(15, 5);
    private static readonly AOEShapeRect rect2 = new(40, 5, 40);
    private readonly List<AOEInstance> _aoes = new(2);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        List<AOEInstance> aoes = new(count);
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes.Add(count > 1 ? aoe with { Color = Colors.Danger } : aoe);
            else if (i == 1)
                aoes.Add(aoe);
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.AutumnWreath => donut,
            AID.DawnsEdge => rect,
            AID.WinterRain => circle,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.Add(new(rect2, spell.LocXZ, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell, 3.1f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && (AID)spell.Action.ID is AID.AutumnWreath or AID.DawnsEdge or AID.WinterRain)
            _aoes.RemoveAt(0);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && (AID)spell.Action.ID == AID.Windburst)
            _aoes.RemoveAt(0);
    }
}

class HuracanStates : StateMachineBuilder
{
    public HuracanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SpringBreeze>()
            .ActivateOnEnter<SummerHeat>()
            .ActivateOnEnter<Combos>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 8912)]
public class Huracan(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
