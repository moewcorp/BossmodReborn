namespace BossMod.Endwalker.VariantCriterion.V01SS.V014ZelessGah;

class Burn(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Burn), 12);
class PureFire2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.PureFire2), 6);

class CastShadowFirst(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CastShadowFirst), new AOEShapeCone(50, 45.Degrees()));
class CastShadowNext(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CastShadowNext), new AOEShapeCone(50, 45.Degrees()));

class FiresteelFracture(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FiresteelFracture), new AOEShapeCone(50, 45.Degrees()));

class InfernGaleKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Unknown2), 20, shape: new AOEShapeCircle(80));

class ShowOfStrength(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ShowOfStrength));

class CastShadow(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(50f, 15f.Degrees());
    private readonly List<AOEInstance> _aoes = new(8);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 6 ? 6 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.CastShadowFirst or (uint)AID.CastShadowNext)
        {
            _aoes.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            if (_aoes.Count == 12)
                _aoes.SortBy(x => x.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.CastShadowFirst or (uint)AID.CastShadowNext)
            _aoes.RemoveAt(0);
    }
}

class BlazingBenifice(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BlazingBenifice), new AOEShapeRect(100, 5));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.Boss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 868, NameID = 11394)]
public class V014ZelessGah(WorldState ws, Actor primary) : BossModule(ws, primary, new(289, -105), new ArenaBoundsRect(15, 20));
