namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V11Geryon;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square1 = new([new Square(V11Geryon.ArenaCenter1, 25f)], [new Square(V11Geryon.ArenaCenter1, 20f)]);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ColossalStrike && Arena.Bounds.Radius == 24.5f && Arena.Center == V11Geryon.ArenaCenter1)
        {
            _aoe = new(square1, Arena.Center, default, WorldState.FutureTime(4d));
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x13 && state == 0x00080004u)
        {
            Arena.Bounds = new ArenaBoundsSquare(20f);
            _aoe = null;
        }
    }
}
