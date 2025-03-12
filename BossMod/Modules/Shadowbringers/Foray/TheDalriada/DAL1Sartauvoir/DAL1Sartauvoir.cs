namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Sartauvoir;

class PyrokinesisAOE(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PyrokinesisAOE));

class Flamedive(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Flamedive), new AOEShapeRect(55f, 2.5f));
class BurningBlade(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.BurningBlade));

class MannatheihwonFlame2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MannatheihwonFlame2));
class MannatheihwonFlame3(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MannatheihwonFlame3), new AOEShapeRect(50f, 4f));
class MannatheihwonFlame4(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MannatheihwonFlame4), 10f);

abstract class Brand(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(40f, 90f.Degrees()));
class LeftBrand(BossModule module) : Brand(module, AID.LeftBrand);
class RightBrand(BossModule module) : Brand(module, AID.RightBrand);

class Pyrocrisis(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Pyrocrisis), 6f);
class Pyrodoxy(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.Pyrodoxy), 6f, 8);

class ThermalGustAOE(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ThermalGustAOE), new AOEShapeRect(44f, 5f));
class GrandCrossflameAOE(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GrandCrossflameAOE), new AOEShapeRect(40f, 9f));

class TimeEruption(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(20f, 10f);
    private readonly List<AOEInstance> _aoes = new(4);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ReverseTimeEruptionAOEFirst or (uint)AID.ReverseTimeEruptionAOESecond or (uint)AID.TimeEruptionAOEFirst or (uint)AID.TimeEruptionAOESecond)
        {
            _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            if (_aoes.Count == 4)
                _aoes.SortBy(x => x.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.ReverseTimeEruptionAOEFirst or (uint)AID.ReverseTimeEruptionAOESecond or (uint)AID.TimeEruptionAOEFirst or (uint)AID.TimeEruptionAOESecond)
            _aoes.RemoveAt(0);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 778, NameID = 32, SortOrder = 2)] //BossNameID = 9384
public class DAL1Sartauvoir(WorldState ws, Actor primary) : BossModule(ws, primary, new(631f, 157f), new ArenaBoundsSquare(20f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.BossP2));
        Arena.Actor(PrimaryActor);
    }
}
