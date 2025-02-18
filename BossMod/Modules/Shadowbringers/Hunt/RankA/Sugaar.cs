namespace BossMod.Shadowbringers.Hunt.RankA.Sugaar;

public enum OID : uint
{
    Boss = 0x2875 // R5.500, x1
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    BodySlam = 18018, // Boss->self, 5.0s cast, range 11 circle

    NumbingNoise = 18015, // Boss->self (front), 5.0s cast, range 13, 120 degree cone
    NumbingNoiseRotation = 18100, // Boss->self, 5.0s cast, range 30 circle, pulls player in from 30 by max 25 units between hitboxes, 3 hits of NumbingNoiseRotationAOE
    NumbingNoiseRotationAOE = 18098, // Boss->self, 0.5s cast, 13 range, 120 degree cone

    TailSnap = 18016, // Boss->self (behind), 5.0s cast, range 18, 120 degree cone
    TailSnapRotation = 18101, // Boss->self, 5.0s cast, range 30 circle, pulls player in from 30 by max 25 units between hitboxes, 3 hits of TailSnapRotationAOE
    TailSnapRotationAOE = 18099 // Boss->self (behind), 0.5 cast, range 18, 120 degree cone
}

class BodySlam(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BodySlam), 11);
class NumbingNoise(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.NumbingNoise), new AOEShapeCone(13, 60.Degrees()));
class TailSnap(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TailSnap), new AOEShapeCone(18, 60.Degrees()));

class NumbingNoiseTailSnapRotating(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCone _shapeNumbingNoise = new(13, 60.Degrees());
    private static readonly AOEShapeCone _shapeTailSnap = new(18, 60.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.NumbingNoiseRotation: // NN always seems to go CCW
                Sequences.Add(new(_shapeNumbingNoise, spell.LocXZ, spell.Rotation, 120.Degrees(), Module.CastFinishAt(spell, 1.1f), 2.7f, 3));
                break;
            case AID.TailSnapRotation: // TS always seems to go CW
                Sequences.Add(new(_shapeTailSnap, spell.LocXZ, spell.Rotation + 180.Degrees(), -120.Degrees(), Module.CastFinishAt(spell, 1.1f), 2.7f, 3));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NumbingNoiseRotationAOE or AID.TailSnapRotationAOE)
            AdvanceSequence(0, WorldState.CurrentTime);
    }
}

class NumbingNoiseTailSnapAttract(BossModule module) : Components.Knockback(module)
{
    private readonly NumbingNoiseTailSnapRotating? _rotating = module.FindComponent<NumbingNoiseTailSnapRotating>();
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(30);

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(Module.PrimaryActor.Position, 25, _activation, _shape, default, Kind.TowardsOrigin, Module.PrimaryActor.HitboxRadius + actor.HitboxRadius);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => _rotating?.ActiveAOEs(slot, actor).Any(aoe => aoe.Check(pos)) ?? false;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NumbingNoiseRotation or AID.TailSnapRotation)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NumbingNoiseRotation or AID.TailSnapRotation)
            _activation = default;
    }
}

class SugaarStates : StateMachineBuilder
{
    public SugaarStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BodySlam>()
            .ActivateOnEnter<NumbingNoise>()
            .ActivateOnEnter<TailSnap>()
            .ActivateOnEnter<NumbingNoiseTailSnapRotating>()
            .ActivateOnEnter<NumbingNoiseTailSnapAttract>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 8902)]
public class Sugaar(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
