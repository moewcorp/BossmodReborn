namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Sartauvoir;

class PyrokinesisAOE(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PyrokinesisAOE));

class Flamedive(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Flamedive), new AOEShapeRect(55, 2.5f));
class BurningBlade(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.BurningBlade));

class MannatheihwonFlame2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MannatheihwonFlame2));
class MannatheihwonFlame3(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MannatheihwonFlame3), new AOEShapeRect(50, 4));
class MannatheihwonFlame4(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MannatheihwonFlame4), 10);

abstract class Brand(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(40, 90.Degrees()));
class LeftBrand(BossModule module) : Brand(module, AID.LeftBrand);
class RightBrand(BossModule module) : Brand(module, AID.RightBrand);

class Pyrocrisis(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Pyrocrisis), 6);
class Pyrodoxy(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.Pyrodoxy), 6, 8);

class ThermalGustAOE(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ThermalGustAOE), new AOEShapeRect(44, 5));
class GrandCrossflameAOE(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GrandCrossflameAOE), new AOEShapeRect(40, 9));

class ReverseTimeEruption(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.ReverseTimeEruptionVisual))
{
    private readonly List<Actor> _castersEruptionAOEFirst = [];
    private readonly List<Actor> _castersEruptionAOESecond = [];

    private static readonly AOEShape _shapeEruptionAOE = new AOEShapeRect(10, 10, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _castersEruptionAOEFirst.Count > 0
            ? _castersEruptionAOEFirst.Select(c => new AOEInstance(_shapeEruptionAOE, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo)))
            : _castersEruptionAOESecond.Select(c => new AOEInstance(_shapeEruptionAOE, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action)?.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action)?.Remove(caster);
    }

    private List<Actor>? CastersForSpell(ActionID spell) => (AID)spell.ID switch
    {
        AID.ReverseTimeEruptionAOEFirst => _castersEruptionAOEFirst,
        AID.ReverseTimeEruptionAOESecond => _castersEruptionAOESecond,
        _ => null
    };
}

class TimeEruption(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.TimeEruptionVisual))
{
    private readonly List<Actor> _castersEruptionAOEFirst = [];
    private readonly List<Actor> _castersEruptionAOESecond = [];

    private static readonly AOEShape _shapeEruptionAOE = new AOEShapeRect(10, 10, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_castersEruptionAOEFirst.Count > 0)
            return _castersEruptionAOEFirst.Select(c => new AOEInstance(_shapeEruptionAOE, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo)));
        else
            return _castersEruptionAOESecond.Select(c => new AOEInstance(_shapeEruptionAOE, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action)?.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action)?.Remove(caster);
    }

    private List<Actor>? CastersForSpell(ActionID spell) => (AID)spell.ID switch
    {
        AID.TimeEruptionAOEFirst => _castersEruptionAOEFirst,
        AID.TimeEruptionAOESecond => _castersEruptionAOESecond,
        _ => null
    };
}
[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 778, NameID = 32, SortOrder = 2)] //BossNameID = 9384
public class DAL1Sartauvoir : BossModule
{
    public readonly List<Actor> Boss;
    public readonly List<Actor> BossP2;

    public DAL1Sartauvoir(WorldState ws, Actor primary) : base(ws, primary, new(631, 157), new ArenaBoundsSquare(20))
    {
        Boss = Enemies(OID.Boss);
        BossP2 = Enemies(OID.BossP2);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Boss);
        Arena.Actors(BossP2);
        Arena.Actor(PrimaryActor);
    }
}
