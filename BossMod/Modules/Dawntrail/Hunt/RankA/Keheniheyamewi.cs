namespace BossMod.Dawntrail.Hunt.RankA.Keheniheyamewi;

public enum OID : uint
{
    Boss = 0x43DC // R8.5
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Scatterscourge1 = 39807, // Boss->self, 4.0s cast, range 10-40 donut
    BodyPress = 40063, // Boss->self, 4.0s cast, range 15 circle
    SlipperyScatterscourge = 38648, // Boss->self, 5.0s cast, range 20 width 10 rect
    WildCharge = 39559, // Boss->self, no cast, range 20 width 10 rect
    Scatterscourge2 = 38650, // Boss->self, 1.5s cast, range 10-40 donut
    PoisonGas = 38652, // Boss->self, 5.0s cast, range 60 circle
    BodyPress2 = 38651, // Boss->self, 4.0s cast, range 15 circle
    MalignantMucus = 38653, // Boss->self, 5.0s cast, single-target
    PoisonMucus = 38654 // Boss->location, 1.0s cast, range 6 circle
}

public enum SID : uint
{
    RightFace = 2164,
    LeftFace = 2163,
    ForwardMarch = 2161,
    AboutFace = 2162
}

class BodyPress(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BodyPress), new AOEShapeCircle(15));
class BodyPress2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BodyPress2), new AOEShapeCircle(15));
class Scatterscourge(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Scatterscourge1), new AOEShapeDonut(10, 40));

class SlipperyScatterscourge(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _caster;
    private readonly List<AOEInstance> _activeAOEs = [];
    private static readonly AOEShapeRect _shapeRect = new(20, 5);
    private static readonly AOEShapeDonut _shapeDonut = new(10, 40);
    private static readonly AOEShapeCircle _shapeCircle = new(10);
    private bool _finishedCast;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_caster == null || _finishedCast)
            yield break;

        var rectEndPosition = GetRectEndPosition(_caster.Position, _caster.Rotation, _shapeRect.LengthFront);

        foreach (var aoe in _activeAOEs)
        {
            if (aoe.Shape == _shapeRect)
            {
                yield return new(_shapeRect, _caster.Position, _caster.Rotation, aoe.Activation, aoe.Color, aoe.Risky);
            }
            else if (aoe.Shape == _shapeDonut || aoe.Shape == _shapeCircle)
            {
                yield return new(aoe.Shape, rectEndPosition, aoe.Rotation, aoe.Activation, aoe.Color, aoe.Risky);
            }
            else
            {
                yield return aoe;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.SlipperyScatterscourge)
            return;
        var activation = WorldState.FutureTime(10);
        _caster = caster;
        _finishedCast = false;
        _activeAOEs.Add(new(_shapeRect, _caster.Position, _caster.Rotation, activation, Colors.Danger));

        var rectEndPosition = GetRectEndPosition(_caster.Position, _caster.Rotation, _shapeRect.LengthFront);

        _activeAOEs.Add(new(_shapeDonut, rectEndPosition, default, activation));
        _activeAOEs.Add(new(_shapeCircle, rectEndPosition, default, activation, Colors.SafeFromAOE, false));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SlipperyScatterscourge)
        {
            var activation = WorldState.FutureTime(10);
            var index = _activeAOEs.FindIndex(aoe => aoe.Shape == _shapeDonut);
            if (index != -1)
            {
                _activeAOEs[index] = new(_shapeDonut, _activeAOEs[index].Origin, _activeAOEs[index].Rotation, activation, Colors.Danger);
                var circleIndex = _activeAOEs.FindIndex(aoe => aoe.Shape == _shapeCircle);
                if (circleIndex != -1)
                {
                    _activeAOEs[circleIndex] = new(_shapeCircle, _activeAOEs[circleIndex].Origin, _activeAOEs[circleIndex].Rotation, activation, Colors.SafeFromAOE, false);
                }
            }
            _finishedCast = true;
        }
        else if (spell.Action.ID == (uint)AID.Scatterscourge2)
        {
            _activeAOEs.RemoveAll(aoe => aoe.Shape == _shapeDonut || aoe.Shape == _shapeCircle);
            _caster = null;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WildCharge && _caster != null)
        {
            _activeAOEs.RemoveAll(aoe => aoe.Shape == _shapeRect);
        }
    }

    private static WPos GetRectEndPosition(WPos origin, Angle rotation, float lengthFront)
    {
        var direction = rotation.ToDirection();
        var offsetX = direction.X * lengthFront;
        var offsetZ = direction.Z * lengthFront;
        return new(origin.X + offsetX, origin.Z + offsetZ);
    }
}

class PoisonGas(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PoisonGas), "Applies Forced March!");

class PoisonGasMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace, 5)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        return Module.FindComponent<SlipperyScatterscourge>()?.ActiveAOEs(slot, actor).Any(a => a.Color != Colors.SafeFromAOE && a.Shape.Check(pos, a.Origin, a.Rotation)) ?? false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var last = ForcedMovements(actor).LastOrDefault();
        if (last.from != last.to && DestinationUnsafe(slot, actor, last.to))
            hints.Add("Aim for green safe spot!");
    }
}

class MalignantMucus(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.MalignantMucus));
class PoisonMucus(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.PoisonMucus), 6);

class KeheniheyamewiStates : StateMachineBuilder
{
    public KeheniheyamewiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BodyPress>()
            .ActivateOnEnter<BodyPress2>()
            .ActivateOnEnter<Scatterscourge>()
            .ActivateOnEnter<SlipperyScatterscourge>()
            .ActivateOnEnter<PoisonGas>()
            .ActivateOnEnter<PoisonGasMarch>()
            .ActivateOnEnter<MalignantMucus>()
            .ActivateOnEnter<PoisonMucus>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13401)]
public class Keheniheyamewi(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);

