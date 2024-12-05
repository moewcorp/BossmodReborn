namespace BossMod.Dawntrail.Hunt.RankA.QueenHawk;

public enum OID : uint
{
    Boss = 0x452B // R2.4
}

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target
    BeeBeGone = 39482, // Boss->self, 4.0s cast, range 12 circle
    BeeBeHere = 39483, // Boss->self, 4.0s cast, range 10-40 donut
    ResonantBuzz = 39486, // Boss->self, 5.0s cast, range 40 circle
    StingingVenom = 39488, // Boss->player, no cast, single-target
    FrenziedSting = 39489, // Boss->player, 5.0s cast, tankbuster.
    StraightSpindle = 39490 // Boss->self, 4.0s cast, rect 50 range 5 width
}

public enum SID : uint
{
    BeeBeHere = 4148,
    BeeBeGone = 4147,
    RightFace = 2164,
    LeftFace = 2163,
    ForwardMarch = 2161,
    AboutFace = 2162
}

class ResonantBuzz(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ResonantBuzz), "Applies Forced March!");
class ResonantBuzzMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Module.FindComponent<BeeBeAOE>()?.ActiveAOEs(slot, actor).Any(a => a.Shape.Check(pos, a.Origin, a.Rotation)) ?? false;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var last = ForcedMovements(actor).LastOrDefault();
        if (last.from != last.to && DestinationUnsafe(slot, actor, last.to))
            hints.Add("Aim outside of the AOE!");
    }
}

class StraightSpindle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.StraightSpindle), new AOEShapeRect(50, 2.5f));
class FrenziedSting(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.FrenziedSting));

class BeeBeAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _activeAOEs = [];
    private static readonly AOEShapeCircle _shapeCircle = new(12);
    private static readonly AOEShapeDonut _shapeDonut = new(10, 40);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _activeAOEs;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (caster != Module.PrimaryActor)
            return;
        var activation = WorldState.FutureTime(15);
        switch ((AID)spell.Action.ID)
        {
            case AID.BeeBeGone:
                _activeAOEs.Add(new(_shapeCircle, caster.Position, default, activation));
                break;
            case AID.BeeBeHere:
                _activeAOEs.Add(new(_shapeDonut, caster.Position, default, activation));
                break;
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (caster != Module.PrimaryActor)
            return;

        switch ((AID)spell.Action.ID)
        {
            case AID.BeeBeGone:
            case AID.BeeBeHere:
                var currentAOE = _activeAOEs[0];
                _activeAOEs[0] = new(currentAOE.Shape, currentAOE.Origin, currentAOE.Rotation, currentAOE.Activation, Colors.Danger);
                break;
        }
    }
    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (actor != Module.PrimaryActor)
            return;

        switch ((SID)status.ID)
        {
            case SID.BeeBeGone:
            case SID.BeeBeHere:
                _activeAOEs.Clear();
                break;
        }
    }
}

class QueenHawkStates : StateMachineBuilder
{
    public QueenHawkStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ResonantBuzz>()
            .ActivateOnEnter<ResonantBuzzMarch>()
            .ActivateOnEnter<StraightSpindle>()
            .ActivateOnEnter<FrenziedSting>()
            .ActivateOnEnter<BeeBeAOE>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13361)]
public class QueenHawk(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);