namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(25f, 35f);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Cloudsplitter)
            _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell, 1.5f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x00)
        {
            Arena.Bounds = A13ArkAngels.DefaultBounds;
            _aoe = null;
        }
    }
}
