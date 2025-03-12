namespace BossMod.Dawntrail.Hunt.RankA.CatsEye;

public enum OID : uint
{
    Boss = 0x4396 // R6.02
}

public enum AID : uint
{
    AutoAttack = 38517, // Boss->player, no cast, single-target
    CatsEye1 = 38510, // Boss->location, 7.0s cast, range 40 circle, non-inverted.
    KillerCuriosity = 38514, // Boss->self, 4.0s cast, single-target
    CatsEye2 = 38511, // Boss->location, 7.0s cast, range 40 circle, inverted gaze on end.
    Unknown = 38516, // Boss->self, no cast, single-target (lose Wandering Eyes)
    GravitationalWave = 39887, // Boss->self, 5.0s cast, range 40 circle
    BloodshotGaze1 = 38515, // Boss->players, 5.0s cast, range 8 circle, non-inverted.
    BloodshotGaze2 = 39668, // Boss->players, 5.0s cast, range 8 circle, inverted.
}

class CatsEye1Gaze(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.CatsEye1));
class CatsEye2Gaze(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.CatsEye2), true);
class GravitationalWave(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.GravitationalWave), "Raidwide!");

class BloodshotGaze1(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.BloodshotGaze1))
{
    private readonly BloodshotStack1 _stack = module.FindComponent<BloodshotStack1>()!;

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        Components.GenericStackSpread.Stack? stack = _stack.Stacks.Count != 0 ? _stack.Stacks[0] : null;
        if (stack != null)
        {
            var v = stack.Value;
            return new Eye[1] { new(v.Target.Position, v.Activation) };
        }
        return [];
    }
}

class BloodshotGaze2(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.BloodshotGaze2), true)
{
    private readonly BloodshotStack2 _stack = module.FindComponent<BloodshotStack2>()!;

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        Components.GenericStackSpread.Stack? stack = _stack.Stacks.Count != 0 ? _stack.Stacks[0] : null;
        if (stack != null)
        {
            var v = stack.Value;
            return new Eye[1] { new(v.Target.Position, v.Activation) };
        }
        return [];
    }
}

class BloodshotStack1(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.BloodshotGaze1), 8f);
class BloodshotStack2(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.BloodshotGaze2), 8f);

class CatsEyeStates : StateMachineBuilder
{
    public CatsEyeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CatsEye1Gaze>()
            .ActivateOnEnter<CatsEye2Gaze>()
            .ActivateOnEnter<GravitationalWave>()
            .ActivateOnEnter<BloodshotStack1>()
            .ActivateOnEnter<BloodshotStack2>()
            .ActivateOnEnter<BloodshotGaze1>()
            .ActivateOnEnter<BloodshotGaze2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13436)]
public class CatsEye(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
