namespace BossMod.Dawntrail.Hunt.RankA.RraxYitya;

public enum OID : uint
{
    Boss = 0x4232 // R5.0
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    RightWingblade1 = 37164, // Boss->self, 3.0s cast, range 25 90 degree cone
    LeftWingblade1 = 37165, // Boss->self, 3.0s cast, range 25 90 degree cone
    LaughingLeap = 37372, // Boss->self, 4.0s cast, range 15 width 5 rect
    TriplicateReflex = 37170, // Boss->self, 5.0s cast, single-target
    HiddenRightWingblade = 37171, // Boss->self, no cast, range 25 90 degree cone
    HiddenLeftWingblade = 37172, // Boss->self, no cast, range 25 90 degree cone
    RightWingblade2 = 37166, // Boss->self, 3.0s cast, range 25 90 degree cone
    LeftWingblade2 = 37167, // Boss->self, 3.0s cast, range 25 90 degree cone
}

abstract class Wingblade(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), HiddenWingblades.Cone);
class RightWingblade1(BossModule module) : Wingblade(module, AID.RightWingblade1);
class LeftWingblade1(BossModule module) : Wingblade(module, AID.LeftWingblade1);
class RightWingblade2(BossModule module) : Wingblade(module, AID.RightWingblade2);
class LeftWingblade2(BossModule module) : Wingblade(module, AID.LeftWingblade2);

class LaughingLeap(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LaughingLeap), new AOEShapeRect(15, 2.5f));
class TriplicateReflex(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.TriplicateReflex), "Last 3 wingblades repeat in rapid succession, stay close to the middle!");

class HiddenWingblades(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _caster;
    private readonly Queue<(uint, Angle)> _wingbladeCasts = new();
    private readonly List<AOEInstance> _aoes = [];
    public static readonly AOEShapeCone Cone = new(25f, 90f.Degrees());
    private int _castCount;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var index = 0;
        while (index < count && WorldState.CurrentTime < _aoes[index].Activation.AddSeconds(10d))
            ++index;

        return CollectionsMarshal.AsSpan(_aoes)[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        _caster ??= caster;

        if (spell.Action.ID is (uint)AID.RightWingblade1 or (uint)AID.LeftWingblade1 or (uint)AID.RightWingblade2 or (uint)AID.LeftWingblade2)
        {
            var rotation = spell.Action.ID is (uint)AID.RightWingblade1 or (uint)AID.RightWingblade2 ? -90f.Degrees() : 90f.Degrees();
            _wingbladeCasts.Enqueue((spell.Action.ID, rotation));
            if (_wingbladeCasts.Count > 3)
                _wingbladeCasts.Dequeue();
        }

        if (spell.Action.ID == (uint)AID.TriplicateReflex)
        {
            // Failsafe: Don't display mechanics if we don't have 3 wingblades stored.
            if (_wingbladeCasts.Count != 3)
                return;

            _aoes.Clear();
            var activationTime = WorldState.FutureTime(10);
            _aoes.Add(new(Cone, spell.LocXZ, spell.Rotation + _wingbladeCasts.Dequeue().Item2, activationTime, Colors.Danger));
            _aoes.Add(new(Cone, spell.LocXZ, spell.Rotation + _wingbladeCasts.Dequeue().Item2, activationTime, Risky: false));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.HiddenRightWingblade or (uint)AID.HiddenLeftWingblade)
        {
            _aoes.RemoveAt(0);
            _castCount++;
            if (_castCount == 1 && _caster != null)
            {
                var activationTime = WorldState.FutureTime(10d);
                _aoes.Add(new(Cone, _caster.Position, _caster.Rotation + _wingbladeCasts.Dequeue().Item2, activationTime, Risky: false));
                _aoes[0] = new(Cone, _aoes[0].Origin, _aoes[0].Rotation, _aoes[0].Activation, Colors.Danger, true);
            }
            else if (_castCount == 2)
                _aoes[0] = new(Cone, _aoes[0].Origin, _aoes[0].Rotation, _aoes[0].Activation, Colors.Danger, true);
            else if (_castCount == 3)
            {
                _aoes.Clear();
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
            .ActivateOnEnter<RightWingblade1>()
            .ActivateOnEnter<LeftWingblade1>()
            .ActivateOnEnter<RightWingblade2>()
            .ActivateOnEnter<LeftWingblade2>()
            .ActivateOnEnter<LaughingLeap>()
            .ActivateOnEnter<TriplicateReflex>()
            .ActivateOnEnter<HiddenWingblades>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 12753)]
public class RraxYitya(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
