namespace BossMod.Endwalker.VariantCriterion.C02AMR.C023Moko;

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(C023Moko.ArenaCenter, 25f)], [new Square(C023Moko.ArenaCenter, 20f)]);

    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NKenkiRelease or (uint)AID.SKenkiRelease && Arena.Bounds == C023Moko.StartingBounds)
            _aoe = new(square, Arena.Center, default, Module.CastFinishAt(spell, 2.1f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x2B)
        {
            Arena.Bounds = C023Moko.DefaultBounds;
            _aoe = null;
        }
    }
}
