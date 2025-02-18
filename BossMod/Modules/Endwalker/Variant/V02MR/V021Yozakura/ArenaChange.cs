namespace BossMod.Endwalker.VariantCriterion.V02MR.V021Yozakura;

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square1 = new([new Square(V021Yozakura.ArenaCenter1, 23f)], [new Square(V021Yozakura.ArenaCenter1, 20f)]);
    private static readonly AOEShapeCustom square2 = new([new Square(V021Yozakura.ArenaCenter3, 23f)], [new Square(V021Yozakura.ArenaCenter3, 20f)]);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GloryNeverlasting && Arena.Bounds == V021Yozakura.StartingBounds)
        {
            void AddAOE(AOEShape shape) => _aoe = new(shape, Arena.Center, default, Module.CastFinishAt(spell, 3.7f));
            if (Arena.Center == V021Yozakura.ArenaCenter1)
                AddAOE(square1);
            else if (Arena.Center == V021Yozakura.ArenaCenter3)
                AddAOE(square2);
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index is 0x35 or 0x36)
        {
            Arena.Bounds = V021Yozakura.DefaultBounds1;
            _aoe = null;
        }
    }
}
