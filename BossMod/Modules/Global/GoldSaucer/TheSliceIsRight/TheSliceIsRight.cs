namespace BossMod.Global.GoldSaucer.SliceIsRight;

public enum OID : uint
{
    Boss = 0x25AB, //R=1.8
    Daigoro = 0x25AC, //R=2.5
    Bamboo = 0x25AD, //R=0.5
    Helper = 0x1EAEDF,
    VisualOutsideArena = 0x2BC8,
    HelperCupPhase1 = 0x1EAEB7,
    HelperCupPhase2 = 0x1EAEB6,
    HelperCupPhase3 = 0x1EAE9D,
    HelperSingleRect = 0x1EAE99,
    HelperDoubleRect = 0x1EAE9A,
    HelperCircle = 0x1EAE9B,
    Pileofgold = 0x1EAE9C,
}

public enum AID : uint
{
    YojimboVisual1 = 19070, // Boss->self, no cast, single-target
    YojimboVisual2 = 18331, // Boss->self, no cast, single-tat
    YojimboVisual3 = 18329, // Boss->self, no cast, single-target
    YojimboVisual4 = 18332, // Boss->self, no cast, single-target
    YojimboVisual5 = 18328, // Boss->location, no cast, single-target
    YojimboVisual6 = 18326, // Boss->self, 3.0s cast, single-target
    YojimboVisual7 = 18339, // Boss->self, 3.0s cast, single-target
    YojimboVisual8 = 18340, // Boss->self, no cast, single-target
    YojimboVisual9 = 19026, // Boss->self, no cast, single-target
    VisualOutsideArena = 18338, // VisualOutsideArena->self, no cast, single-target

    BambooSplit = 18333, // Bamboo->self, 0.7s cast, range 28 width 5 rect
    BambooCircleFall = 18334, // Bamboo->self, 0.7s cast, range 11 circle 
    BambooSpawn = 18327, // Bamboo->self, no cast, range 3 circle
    FirstGilJump = 18335, // Daigoro->location, 2.5s cast, width 7 rect charge
    NextGilJump = 18336, // Daigoro->location, 1.5s cast, width 7 rect charge
    BadCup = 18337, // Daigoro->self, 1.0s cast, range 15+R 120-degree cone
}

class BambooSplits(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    private static readonly AOEShapeRect rect = new(28, 2.5f);
    private static readonly AOEShapeCircle circle = new(11);
    private static readonly AOEShapeCircle bamboospawn = new(3);
    private static readonly Angle a90 = 90.Degrees();

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.HelperCircle or OID.HelperDoubleRect or OID.HelperSingleRect)
            _aoes.Add(new(bamboospawn, actor.Position, default, WorldState.FutureTime(2.7f)));
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00010002)
        {
            var activation = WorldState.FutureTime(7); // activation varies per set, taking the lowest we can find
            if ((OID)actor.OID == OID.HelperCircle)
                _aoes.Add(new(circle, actor.Position, actor.Rotation, activation));
            else if ((OID)actor.OID == OID.HelperSingleRect)
                _aoes.Add(new(rect, actor.Position, actor.Rotation + a90, activation));
            else if ((OID)actor.OID == OID.HelperDoubleRect)
            {
                _aoes.Add(new(rect, actor.Position, actor.Rotation + a90, activation));
                _aoes.Add(new(rect, actor.Position, actor.Rotation - a90, activation));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.BambooSpawn:
                    _aoes.RemoveAll(x => x.Origin == caster.Position && x.Shape == bamboospawn);
                    break;
                case AID.BambooSplit:
                case AID.BambooCircleFall:
                    AOEInstance[] aoesToRemove = [.. _aoes.Where((aoe, index) => aoe.Origin == caster.Position).Take(2)];
                    if (aoesToRemove.Length >= 1)
                        _aoes.Remove(aoesToRemove[0]);
                    if (aoesToRemove.Length == 2 && aoesToRemove[0].Activation == aoesToRemove[1].Activation)
                        _aoes.Remove(aoesToRemove[1]);
                    break;
            }
        }
    }
}

class DaigoroFirstGilJump(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.FirstGilJump), 3.5f);
class DaigoroNextGilJump(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.NextGilJump), 3.5f);

class TheSliceIsRightStates : StateMachineBuilder
{
    public TheSliceIsRightStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BambooSplits>()
            .ActivateOnEnter<DaigoroFirstGilJump>()
            .ActivateOnEnter<DaigoroNextGilJump>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.GoldSaucer, GroupID = 181, NameID = 9066)]
public class TheSliceIsRight(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(70.5f, -36), 15 * CosPI.Pi28th, 28)]);
    protected override bool CheckPull() => PrimaryActor != null;
}
