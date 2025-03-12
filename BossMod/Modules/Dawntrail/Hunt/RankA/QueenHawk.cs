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
    private readonly BeeBeAOE _aoe = module.FindComponent<BeeBeAOE>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
                return true;
        }
        return false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var last = ForcedMovements(actor).LastOrDefault();
        if (last.from != last.to && DestinationUnsafe(slot, actor, last.to))
            hints.Add("Aim outside of the AOE!");
    }
}

class StraightSpindle(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.StraightSpindle), new AOEShapeRect(50, 2.5f));
class FrenziedSting(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.FrenziedSting));

class BeeBeAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle _shapeCircle = new(12);
    private static readonly AOEShapeDonut _shapeDonut = new(10, 40);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var activation = WorldState.FutureTime(15d);
        switch (spell.Action.ID)
        {
            case (uint)AID.BeeBeGone:
                _aoes.Add(new(_shapeCircle, spell.LocXZ, default, activation));
                break;
            case (uint)AID.BeeBeHere:
                _aoes.Add(new(_shapeDonut, spell.LocXZ, default, activation));
                break;
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BeeBeGone:
            case (uint)AID.BeeBeHere:
                var currentAOE = _aoes[0];
                _aoes[0] = new(currentAOE.Shape, currentAOE.Origin, currentAOE.Rotation, currentAOE.Activation, Colors.Danger);
                break;
        }
    }
    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.BeeBeGone:
            case (uint)SID.BeeBeHere:
                _aoes.Clear();
                break;
        }
    }
}

class QueenHawkStates : StateMachineBuilder
{
    public QueenHawkStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BeeBeAOE>()
            .ActivateOnEnter<ResonantBuzz>()
            .ActivateOnEnter<ResonantBuzzMarch>()
            .ActivateOnEnter<StraightSpindle>()
            .ActivateOnEnter<FrenziedSting>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13361)]
public class QueenHawk(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
