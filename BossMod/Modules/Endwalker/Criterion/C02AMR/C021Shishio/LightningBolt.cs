namespace BossMod.Endwalker.VariantCriterion.C02AMR.C021Shishio;

abstract class LightningBolt(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 6f);
class NLightningBolt(BossModule module) : LightningBolt(module, AID.NLightningBoltAOE);
class SLightningBolt(BossModule module) : LightningBolt(module, AID.SLightningBoltAOE);

class CloudToCloud(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape1 = new(100f, 1f);
    private static readonly AOEShapeRect _shape2 = new(100f, 3f);
    private static readonly AOEShapeRect _shape3 = new(100f, 6f);

    public bool Active => _aoes.Count > 0;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var deadline = _aoes[0].Activation.AddSeconds(1.4d);
        List<AOEInstance> aoes = new(count);
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (aoe.Activation <= deadline)
                aoes.Add(aoe);
            else
                break;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = ShapeForAction(spell.Action);
        if (shape != null)
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (ShapeForAction(spell.Action) != null)
        {
            ++NumCasts;
            var numRemoved = _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1f));
            if (numRemoved != 1)
                ReportError($"Unexpected number of matching aoes: {numRemoved}");
        }
    }

    private static AOEShapeRect? ShapeForAction(ActionID action) => action.ID switch
    {
        (uint)AID.NCloudToCloud1 or (uint)AID.SCloudToCloud1 => _shape1,
        (uint)AID.NCloudToCloud2 or (uint)AID.SCloudToCloud2 => _shape2,
        (uint)AID.NCloudToCloud3 or (uint)AID.SCloudToCloud3 => _shape3,
        _ => null
    };
}
