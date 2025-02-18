namespace BossMod.Dawntrail.Dungeon.D02WorqorZormor.D021RyoqorTerteh;

public enum OID : uint
{
    Boss = 0x4159, // R5.28
    RorrlohTeh = 0x415B, // R1.5
    QorrlohTeh1 = 0x415A, // R3.0
    QorrlohTeh2 = 0x43A2, // R0.5
    Snowball = 0x415C, // R2.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    FrostingFracasVisual = 36279, // Boss->self, 5.0s cast, single-target
    FrostingFracas = 36280, // Helper->self, 5.0s cast, range 60 circle

    FluffleUp = 36265, // Boss->self, 4.0s cast, single-target
    ColdFeat = 36266, // Boss->self, 4.0s cast, single-target
    IceScream = 36270, // RorrlohTeh->self, 12.0s cast, range 20 width 20 rect

    FrozenSwirlVisual = 36271, // QorrlohTeh1->self, 12.0s cast, single-target
    FrozenSwirl = 36272, // QorrlohTeh2->self, 12.0s cast, range 15 circle

    Snowscoop = 36275, // Boss->self, 4.0s cast, single-target
    SnowBoulder = 36278, // Snowball->self, 4.0s cast, range 50 width 6 rect

    SparklingSprinklingVisual = 36713, // Boss->self, 5.0s cast, single-target
    SparklingSprinkling = 36281 // Helper->player, 5.0s cast, range 5 circle
}

public enum TetherID : uint
{
    Freeze = 272 // RorrlohTeh/QorrlohTeh1->Boss
}

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(20f, 23f);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FrostingFracas && Arena.Bounds == D021RyoqorTerteh.StartingBounds)
            _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell, 0.6f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x17)
        {
            Arena.Bounds = D021RyoqorTerteh.DefaultBounds;
            _aoe = null;
        }
    }
}

class IceScreamFrozenSwirl(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(20f, 10f);
    private static readonly AOEShapeCircle circle = new(15f);
    private readonly List<AOEInstance> _aoesCircle = new(4), _aoesRect = new(4);
    private readonly List<Actor> circleAOE = new(4), rectAOE = new(4);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var countCircle = _aoesCircle.Count;
        var countRect = _aoesRect.Count;
        if (countCircle == 0 && countRect == 0)
            return [];
        var result = new List<AOEInstance>(4);
        for (var i = 0; i < 2 && i < countCircle; ++i)
            result.Add(_aoesCircle[i]);
        for (var i = 0; i < 2 && i < countRect; ++i)
            result.Add(_aoesRect[i]);
        return result;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.QorrlohTeh1)
            circleAOE.Add(actor);
        else if (actor.OID == (uint)OID.RorrlohTeh)
            rectAOE.Add(actor);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Freeze)
        {
            var activation1 = WorldState.FutureTime(9.9f);
            var activation2 = WorldState.FutureTime(14.9f);
            if (circleAOE.Contains(source))
            {
                _aoesCircle.Add(new(circle, source.Position, default, activation2));
                circleAOE.Remove(source);
                if (_aoesCircle.Count == 2)
                {
                    for (var i = 0; i < circleAOE.Count; ++i)
                        _aoesCircle.Add(new(circle, circleAOE[i].Position, default, activation1));
                    circleAOE.Clear();
                    _aoesCircle.Sort((x, y) => x.Activation.CompareTo(y.Activation));
                }
            }
            else if (rectAOE.Contains(source))
            {
                _aoesRect.Add(new(rect, source.Position, source.Rotation, activation2));
                rectAOE.Remove(source);
                if (_aoesRect.Count == 2)
                {
                    for (var i = 0; i < rectAOE.Count; ++i)
                    {
                        var e = rectAOE[i];
                        _aoesRect.Add(new(rect, e.Position, e.Rotation, activation1));
                    }
                    rectAOE.Clear();
                    _aoesRect.Sort((x, y) => x.Activation.CompareTo(y.Activation));
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoesRect.Count != 0 && spell.Action.ID == (uint)AID.IceScream)
            _aoesRect.RemoveAt(0);
        else if (_aoesCircle.Count != 0 && spell.Action.ID == (uint)AID.FrozenSwirl)
            _aoesCircle.RemoveAt(0);
    }
}

class FrostingFracas(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FrostingFracas));
class SnowBoulder(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SnowBoulder), new AOEShapeRect(50f, 3f), 6);
class SparklingSprinkling(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.SparklingSprinkling), 5f);

class D021RyoqorTertehStates : StateMachineBuilder
{
    public D021RyoqorTertehStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<FrostingFracas>()
            .ActivateOnEnter<IceScreamFrozenSwirl>()
            .ActivateOnEnter<SnowBoulder>()
            .ActivateOnEnter<SparklingSprinkling>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 824, NameID = 12699)]
public class D021RyoqorTerteh(WorldState ws, Actor primary) : BossModule(ws, primary, StartingBounds.Center, StartingBounds)
{
    private static readonly WPos arenaCenter = new(-108f, 119f);
    public static readonly ArenaBoundsComplex StartingBounds = new([new Polygon(arenaCenter, 22.5f, 52)]);
    public static readonly ArenaBoundsComplex DefaultBounds = new([new Polygon(arenaCenter, 20f, 52)]);
}
