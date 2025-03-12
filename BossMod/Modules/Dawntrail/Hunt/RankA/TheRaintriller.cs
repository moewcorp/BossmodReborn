namespace BossMod.Dawntrail.Hunt.RankA.TheRaintriller;

public enum OID : uint
{
    Boss = 0x457F, // R4.800, x1
}

public enum AID : uint
{
    // Do Re Misery has multiple AIDs with variant casts, 1 -> One Yell, 2 -> 2 Yells, 3 -> 3 Yells.
    // Croakdown/Ribbitygibbet/ChirpOfTheWisp are NOT what they correspond to in-game...
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 39804, // Boss->location, no cast, single-target (I think this corresponds to yells??)
    DoReMisery1 = 39758, // Boss->self, 5.0s cast, single-target
    Croakdown = 39750, // Boss->self, 1.0s cast, range 12 circle "Chirp!"
    DoReMisery2 = 39751, // Boss->self, 6.2s cast, single-target
    Ribbitygibbet = 39752, // Boss->self, 1.0s cast, range 10-40 donut "Ribbit!"
    DoReMisery3 = 39749, // Boss->self, 7.5s cast, single-target
    ChirpOTheWisp = 39753, // Boss->self, 1.0s cast, range 40 135-degree cone "Croak!"
    DropOfVenom = 39754, // Boss->player, 5.0s cast, range 6 circle
}

class DoReMisery(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _caster;
    private readonly List<AOEInstance> _aoes = [];
    private readonly Queue<uint> _doReMiseryActions = new();
    private static readonly AOEShapeCircle _shapeOut = new(12f);
    private static readonly AOEShapeDonut _shapeIn = new(10f, 40f);
    private static readonly AOEShapeCone _shapeBehind = new(40f, 135f.Degrees());
    private int _actionCount;

    private static readonly Dictionary<ushort, uint[]> DoReMiseryNpcYellMap = new()
        {
            { 17815, new[] { (uint)AID.Croakdown } },
            { 17719, new[] { (uint)AID.Croakdown, (uint)AID.Ribbitygibbet } },
            { 17720, new[] { (uint)AID.Ribbitygibbet, (uint)AID.Croakdown, (uint)AID.ChirpOTheWisp } },
            { 17721, new[] { (uint)AID.Croakdown, (uint)AID.Ribbitygibbet, (uint)AID.ChirpOTheWisp } },
            { 17722, new[] { (uint)AID.ChirpOTheWisp, (uint)AID.Ribbitygibbet, (uint)AID.Croakdown } },
            { 17723, new[] { (uint)AID.Ribbitygibbet, (uint)AID.ChirpOTheWisp, (uint)AID.Croakdown } },
            { 17724, new[] { (uint)AID.ChirpOTheWisp, (uint)AID.Croakdown, (uint)AID.Ribbitygibbet } },
            { 17725, new[] { (uint)AID.Croakdown, (uint)AID.ChirpOTheWisp, (uint)AID.Ribbitygibbet } },
            { 17734, new[] { (uint)AID.Croakdown, (uint)AID.ChirpOTheWisp, (uint)AID.Croakdown } },
            { 17793, new[] { (uint)AID.Ribbitygibbet, (uint)AID.Ribbitygibbet, (uint)AID.Croakdown } }
        };

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (aoe.Shape == _shapeBehind && _caster != null)
                _aoes[i] = new(_shapeBehind, _caster.Position, _caster.Rotation, aoe.Activation, aoe.Color, aoe.Risky);
            else
                aoes[i] = aoe;
        }
        return aoes;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (caster != Module.PrimaryActor || spell.Action.ID is not (uint)AID.Croakdown and not (uint)AID.Ribbitygibbet and not (uint)AID.ChirpOTheWisp || _aoes.Count == 0)
            return;

        _aoes.RemoveAt(0);
        _actionCount++;

        if (_actionCount == 1 && _aoes.Count > 0 || _actionCount == 2 && _aoes.Count > 0)
            _aoes[0] = new(_aoes[0].Shape, _aoes[0].Origin, _aoes[0].Rotation, _aoes[0].Activation, Colors.Danger);

        if (_doReMiseryActions.Count > 0 && _caster != null)
            _aoes.Add(CreateAOEInstance(_doReMiseryActions.Dequeue(), WorldState.FutureTime(10), _actionCount >= 2 ? Colors.Danger : 0, false));

        if (_doReMiseryActions.Count == 0 && _aoes.Count == 0)
        {
            _aoes.Clear();
            _doReMiseryActions.Clear();
            _actionCount = 0;
        }
    }

    public override void OnActorNpcYell(Actor actor, ushort id)
    {
        _caster ??= actor;
        if (!DoReMiseryNpcYellMap.TryGetValue(id, out var actions))
            return;

        _doReMiseryActions.Clear();
        _aoes.Clear();
        _actionCount = 0;
        foreach (var action in actions)
            _doReMiseryActions.Enqueue(action);
        if (_caster != null)
        {
            if (_doReMiseryActions.Count > 0)
                _aoes.Add(CreateAOEInstance(_doReMiseryActions.Dequeue(), WorldState.FutureTime(10d), Colors.Danger));

            if (_doReMiseryActions.Count > 0)
                _aoes.Add(CreateAOEInstance(_doReMiseryActions.Dequeue(), WorldState.FutureTime(10d), Colors.AOE, false));
        }
    }

    private AOEInstance CreateAOEInstance(uint aid, DateTime activation, uint color, bool risky = true) => aid switch
    {
        (uint)AID.Ribbitygibbet => new(_shapeIn, _caster!.Position, default, activation, color, risky),
        (uint)AID.Croakdown => new(_shapeOut, _caster!.Position, default, activation, color, risky),
        (uint)AID.ChirpOTheWisp => new(_shapeBehind, _caster!.Position, _caster.Rotation, activation, color, risky),
        _ => new(_shapeIn, _caster!.Position, default, activation, color, risky)
    };
}

class DropOfVenom(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DropOfVenom), 6f);

class TheRaintrillerStates : StateMachineBuilder
{
    public TheRaintrillerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DoReMisery>()
            .ActivateOnEnter<DropOfVenom>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13442)]
public class TheRaintriller(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
