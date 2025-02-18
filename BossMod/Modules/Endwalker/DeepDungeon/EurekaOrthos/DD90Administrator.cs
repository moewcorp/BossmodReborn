namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD90Administrator;

public enum OID : uint
{
    Boss = 0x3D23, // R5.950
    EggInterceptor = 0x3D24, // R2.3
    SquareInterceptor = 0x3D25, // R2.3
    OrbInterceptor = 0x3D26, // R2.3
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 31457, // Boss->player, no cast, single-target

    AetherochemicalLaserCone = 31451, // EggInterceptor->self, 6.0s cast, range 50 120-degree cone
    AetherochemicalLaserDonut = 31453, // OrbInterceptor->self, 6.0s cast, range 8-40 donut
    AetherochemicalLaserLine = 31452, // SquareInterceptor->self, 6.0s cast, range 40 width 5 rect

    AetherochemicalLaserCone2 = 32832, // EggInterceptor->self, 8.0s cast, range 50 120-degree cone
    AetherochemicalLaserLine2 = 32833, // SquareInterceptor->self, 8.0s cast, range 40 width 5 rect

    CrossLaser = 31448, // Boss->self, 6.0s cast, range 60 width 10 cross

    PeripheralLasersModelChange1 = 31458, // Boss->self, no cast, single-target
    PeripheralLasersModelChange2 = 31459, // Boss->self, no cast, single-target
    PeripheralLasers = 31447, // Boss->self, 6.0s cast, range 5-60 donut

    HomingLaserVisual = 31460, // Boss->self, no cast, single-target
    HomingLaser = 31461, // Helper->location, 3.0s cast, range 6 circle

    Laserstream = 31456, // Boss->self, 4.0s cast, range 60 circle, raidwide
    ParallelExecution = 31454, // Boss->self, 3.0s cast, range 60 circle, starts action combo

    SalvoScript = 31455, // Boss->self, 3.0s cast, range 60 circle
    SupportSystems = 31449, // Boss->self, 3.0s cast, single-target

    InterceptionSequence = 31450 // Boss->self, 3.0s cast, range 60 circle, starts action combo
}

public enum IconID : uint
{
    Icon1 = 390,
    Icon2 = 391,
    Icon3 = 392
}

class AetheroChemicalLaserCombo(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCone(50f, 60f.Degrees()), new AOEShapeDonut(8f, 60f), new AOEShapeRect(40f, 2.5f),
    new AOEShapeCross(60f, 5f), new AOEShapeDonut(5f, 60f)];
    private readonly Dictionary<uint, List<AOEInstance>> _icons = new() {
        { (uint)IconID.Icon1, [] },
        { (uint)IconID.Icon2, [] },
        { (uint)IconID.Icon3, [] }
    };
    private AOEInstance _boss;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var icon in _icons)
        {
            if (icon.Value != null && icon.Value.Count > 0)
            {
                foreach (var c in icon.Value)
                    yield return new(c.Shape, c.Origin, c.Rotation, c.Activation, Colors.Danger);
                var nextIcon = _icons.FirstOrDefault(x => x.Key == icon.Key + 1).Value;
                if (nextIcon != null)
                    foreach (var c in nextIcon)
                        yield return new(c.Shape, c.Origin, c.Rotation, c.Activation, Risky: false);
                if (_boss != default)
                    yield return new(_boss.Shape, _boss.Origin, _boss.Rotation, _boss.Activation, Risky: false);
                yield break;
            }
        }
        if (_boss != default)
            yield return new(_boss.Shape, _boss.Origin, _boss.Rotation, _boss.Activation, Colors.Danger);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var shapeIndex = actor.OID switch
        {
            (uint)OID.SquareInterceptor => 2,
            (uint)OID.OrbInterceptor => 1,
            (uint)OID.EggInterceptor => 0,
            _ => default
        };

        var activation = iconID switch
        {
            (uint)IconID.Icon1 => WorldState.FutureTime(7d),
            (uint)IconID.Icon2 => WorldState.FutureTime(10.5d),
            (uint)IconID.Icon3 => WorldState.FutureTime(14d),
            _ => default
        };

        _icons[iconID].Add(new(_shapes[shapeIndex], WPos.ClampToGrid(actor.Position), actor.OID == (uint)OID.OrbInterceptor ? default : actor.Rotation, activation));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        _boss = spell.Action.ID switch
        {
            (uint)AID.PeripheralLasers => new(_shapes[4], spell.LocXZ, default, Module.CastFinishAt(spell)),
            (uint)AID.CrossLaser => new(_shapes[3], spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)),
            _ => _boss
        };
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.AetherochemicalLaserCone or (uint)AID.AetherochemicalLaserLine or (uint)AID.AetherochemicalLaserDonut)
        {
            foreach (var icon in _icons)
                if (icon.Value.Count != 0)
                {
                    icon.Value.RemoveAt(0);
                    break;
                }
        }
        else if (spell.Action.ID is (uint)AID.PeripheralLasers or (uint)AID.CrossLaser)
            _boss = default;
    }
}

class AetherLaserLine(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AetherochemicalLaserLine), new AOEShapeRect(40f, 2.5f), 4)
{
    private readonly AetheroChemicalLaserCombo _aoe = module.FindComponent<AetheroChemicalLaserCombo>()!;
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;

        if (count == 0)
            return [];

        var hasActiveAOEs = false;
        {
            foreach (var aoe in _aoe.ActiveAOEs(slot, actor))
            {
                hasActiveAOEs = true;
                break;
            }
        }
        if (hasActiveAOEs)
            return [];

        var max = count > 4 ? 4 : count;
        var aoes = new AOEInstance[max];
        for (var i = 0; i < max; ++i)
        {
            var caster = Casters[i];
            aoes[i] = caster with { Color = i < 2 && count > i ? Colors.Danger : 0 };
        }
        return aoes;
    }
}

class AetherLaserLine2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AetherochemicalLaserLine2), new AOEShapeRect(40f, 2.5f));
class AetherLaserCone(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AetherochemicalLaserCone2), new AOEShapeCone(50f, 60f.Degrees()));
class HomingLasers(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HomingLaser), 6f);
class Laserstream(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Laserstream));

class DD90AdministratorStates : StateMachineBuilder
{
    public DD90AdministratorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AetheroChemicalLaserCombo>()
            .ActivateOnEnter<AetherLaserLine>()
            .ActivateOnEnter<AetherLaserLine2>()
            .ActivateOnEnter<AetherLaserCone>()
            .ActivateOnEnter<Laserstream>()
            .ActivateOnEnter<HomingLasers>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "legendoficeman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 905, NameID = 12102)]
public class DD90Administrator(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300f, -300f), new ArenaBoundsSquare(20f));
