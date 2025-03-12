namespace BossMod.Dawntrail.Hunt.RankA.Keheniheyamewi;

public enum OID : uint
{
    Boss = 0x43DC // R8.5
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Scatterscourge1 = 39807, // Boss->self, 4.0s cast, range 10-40 donut
    SlipperyScatterscourge = 38648, // Boss->self, 5.0s cast, range 20 width 10 rect
    WildCharge = 39559, // Boss->self, no cast, range 20 width 10 rect
    Scatterscourge2 = 38650, // Boss->self, 1.5s cast, range 10-40 donut
    PoisonGas = 38652, // Boss->self, 5.0s cast, range 60 circle
    BodyPress1 = 40063, // Boss->self, 4.0s cast, range 15 circle
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

abstract class BodyPress(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 15f);
class BodyPress1(BossModule module) : BodyPress(module, AID.BodyPress1);
class BodyPress2(BossModule module) : BodyPress(module, AID.BodyPress2);

class Scatterscourge(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Scatterscourge1), new AOEShapeDonut(10f, 40f));

class SlipperyScatterscourge(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _caster;
    private readonly List<AOEInstance> _activeAOEs = [];
    private static readonly AOEShapeRect _shapeRect = new(20f, 5f);
    private static readonly AOEShapeDonut _shapeDonut = new(10f, 40f);
    private static readonly AOEShapeCircle _shapeCircle = new(10f);
    private bool _finishedCast;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_caster == null)
            return [];

        WPos rectEndPos = default;
        if (!_finishedCast)
            rectEndPos = GetRectEndPosition(_caster.Position, _caster.Rotation, _shapeRect.LengthFront);

        var count = _activeAOEs.Count;
        if (count == 0)
            return [];

        var aoes = new AOEInstance[count];

        for (var i = 0; i < count; ++i)
        {
            var aoe = _activeAOEs[i];

            if (aoe.Shape == _shapeRect)
            {
                if (!_finishedCast)
                    aoes[i] = new(_shapeRect, _caster.Position, _caster.Rotation, aoe.Activation, aoe.Color, aoe.Risky);
            }
            else if (aoe.Shape == _shapeDonut || aoe.Shape == _shapeCircle)
            {
                if (!_finishedCast)
                    aoes[i] = new(aoe.Shape, rectEndPos, aoe.Rotation, aoe.Activation, aoe.Color, aoe.Risky);
                else
                    aoes[i] = aoe;
            }
            else
                aoes[i] = aoe;
        }

        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.SlipperyScatterscourge)
            return;

        var activation = WorldState.FutureTime(10d);
        _caster = caster;
        _finishedCast = false;

        _activeAOEs.Add(new(_shapeRect, spell.LocXZ, caster.Rotation, activation, Colors.Danger));
        _activeAOEs.Add(new(_shapeDonut, spell.LocXZ, default, activation));
        _activeAOEs.Add(new(_shapeCircle, spell.LocXZ, default, activation, Colors.SafeFromAOE, false));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_caster != null && spell.Action.ID == (uint)AID.SlipperyScatterscourge)
        {
            var finalPos = GetRectEndPosition(_caster.Position, _caster.Rotation, _shapeRect.LengthFront);
            var futureActivation = WorldState.FutureTime(10d);

            for (int i = 0; i < _activeAOEs.Count; ++i)
            {
                var aoe = _activeAOEs[i];
                if (aoe.Shape == _shapeDonut)
                {
                    _activeAOEs[i] = new(_shapeDonut, finalPos, aoe.Rotation, futureActivation, Colors.Danger);
                }
                else if (aoe.Shape == _shapeCircle)
                {
                    _activeAOEs[i] = new(_shapeCircle, finalPos, aoe.Rotation, futureActivation, Colors.SafeFromAOE, false);
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
            _activeAOEs.RemoveAll(aoe => aoe.Shape == _shapeRect);
    }

    private static WPos GetRectEndPosition(WPos origin, Angle rotation, float lengthFront)
    {
        var direction = rotation.ToDirection();
        var offsetX = direction.X * lengthFront;
        var offsetZ = direction.Z * lengthFront;
        return new(origin.X + offsetX, origin.Z + offsetZ);
    }
}

class PoisonGas(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PoisonGas), "Raidwide & Forced March (13s)");

class PoisonGasMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace, 5)
{
    private readonly SlipperyScatterscourge _aoe = module.FindComponent<SlipperyScatterscourge>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Color != Colors.SafeFromAOE && aoe.Check(pos))
                return true;
        }
        return false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var last = ForcedMovements(actor).LastOrDefault();
        if (last.from != last.to && DestinationUnsafe(slot, actor, last.to))
            hints.Add("Aim for green safe spot!");
    }
}

class MalignantMucus(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.MalignantMucus));
class PoisonMucus(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.PoisonMucus), 6f);

class KeheniheyamewiStates : StateMachineBuilder
{
    public KeheniheyamewiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BodyPress1>()
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
