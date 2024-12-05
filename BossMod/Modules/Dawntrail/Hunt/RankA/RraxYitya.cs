namespace BossMod.Dawntrail.Hunt.RankA.RraxYitya;

public enum OID : uint
{
    Boss = 0x4232 // R5.0
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    RightWingblade = 37164, // Boss->self, 3.0s cast, range 25 90 degree cone
    LeftWingblade = 37165, // Boss->self, 3.0s cast, range 25 90 degree cone
    LaughingLeap = 37372, // Boss->self, 4.0s cast, range 15 width 5 rect
    TriplicateReflex = 37170, // Boss->self, 5.0s cast, single-target
    HiddenRightWingblade = 37171, // Boss->self, no cast, range 25 90 degree cone
    HiddenLeftWingblade = 37172, // Boss->self, no cast, range 25 90 degree cone
    RightWingblade2 = 37166, // Boss->self, 3.0s cast, range 25 90 degree cone
    LeftWingblade2 = 37167, // Boss->self, 3.0s cast, range 25 90 degree cone
}

class RightWingblade(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RightWingblade), new AOEShapeCone(25, 90.Degrees()));
class LeftWingblade(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LeftWingblade), new AOEShapeCone(25, 90.Degrees()));
class RightWingblade2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RightWingblade2), new AOEShapeCone(25, 90.Degrees()));
class LeftWingblade2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LeftWingblade2), new AOEShapeCone(25, 90.Degrees()));
class LaughingLeap(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LaughingLeap), new AOEShapeRect(15, 2.5f));
class TriplicateReflex(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.TriplicateReflex), "Last 3 wingblades repeat in rapid succession, stay close to the middle!");

class HiddenWingblades(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _caster;
    private readonly Queue<(AID, Angle)> _wingbladeCasts = new();
    private readonly List<AOEInstance> _activeAOEs = [];
    private static readonly AOEShapeCone _shape = new(25, 90.Degrees());
    private int _castCount;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _activeAOEs.Where(aoe => WorldState.CurrentTime < aoe.Activation.AddSeconds(10));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        _caster ??= caster;

        if (spell.Action.ID is (uint)AID.RightWingblade or (uint)AID.LeftWingblade or (uint)AID.RightWingblade2 or (uint)AID.LeftWingblade2)
        {
            var rotation = spell.Action.ID is (uint)AID.RightWingblade or (uint)AID.RightWingblade2 ? -90.Degrees() : 90.Degrees();
            _wingbladeCasts.Enqueue(((AID)spell.Action.ID, rotation));
            if (_wingbladeCasts.Count > 3)
                _wingbladeCasts.Dequeue();
        }

        if (spell.Action.ID == (uint)AID.TriplicateReflex)
        {
            // Failsafe: Don't display mechanics if we don't have 3 wingblades stored.
            if (_wingbladeCasts.Count != 3)
                return;

            _activeAOEs.Clear();
            var activationTime = WorldState.FutureTime(10);
            _activeAOEs.Add(new(_shape, _caster.Position, _caster.Rotation + _wingbladeCasts.Dequeue().Item2, activationTime, Colors.Danger));
            _activeAOEs.Add(new(_shape, _caster.Position, _caster.Rotation + _wingbladeCasts.Dequeue().Item2, activationTime, Risky: false));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HiddenRightWingblade or AID.HiddenLeftWingblade && _activeAOEs.Count > 0)
        {
            _activeAOEs.RemoveAt(0);
            _castCount++;
            if (_castCount == 1 && _caster != null)
            {
                DateTime activationTime = WorldState.FutureTime(10);
                _activeAOEs.Add(new(_shape, _caster.Position, _caster.Rotation + _wingbladeCasts.Dequeue().Item2, activationTime, Risky: false));
                _activeAOEs[0] = new(_shape, _activeAOEs[0].Origin, _activeAOEs[0].Rotation, _activeAOEs[0].Activation, Colors.Danger, true);
            }
            else if (_castCount == 2)
                _activeAOEs[0] = new(_shape, _activeAOEs[0].Origin, _activeAOEs[0].Rotation, _activeAOEs[0].Activation, Colors.Danger, true);
            else if (_castCount == 3)
            {
                _activeAOEs.Clear();
                _wingbladeCasts.Clear();
                _castCount = 0;
            }
        }
    }
}

class RraxYityaStates : StateMachineBuilder
{
    public RraxYityaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RightWingblade>()
            .ActivateOnEnter<LeftWingblade>()
            .ActivateOnEnter<RightWingblade2>()
            .ActivateOnEnter<LeftWingblade2>()
            .ActivateOnEnter<LaughingLeap>()
            .ActivateOnEnter<TriplicateReflex>()
            .ActivateOnEnter<HiddenWingblades>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 12753)]
public class RraxYitya(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
