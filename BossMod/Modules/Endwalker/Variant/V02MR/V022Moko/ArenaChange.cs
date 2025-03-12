namespace BossMod.Endwalker.VariantCriterion.V02MR.V022Moko;

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly WPos ArenaCenter = new(-700, 540);
    public static readonly ArenaBoundsSquare StartingBounds = new(24.6f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20);
    private static readonly AOEShapeCustom square = new([new Square(ArenaCenter, 25)], [new Square(ArenaCenter, 20)]);

    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.KenkiRelease && Arena.Bounds == StartingBounds)
            _aoe = new(square, Arena.Center, default, Module.CastFinishAt(spell, 2.1f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x5B)
        {
            Arena.Bounds = DefaultBounds;
            _aoe = null;
        }
    }
}
