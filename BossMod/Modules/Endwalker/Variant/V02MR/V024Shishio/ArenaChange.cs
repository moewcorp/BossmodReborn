namespace BossMod.Endwalker.VariantCriterion.V02MR.V024Shishio;

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(20f, 28f);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StormcloudSummons)
            _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell, 0.7f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x64)
        {
            if (state == 0x00020001)
            {
                Arena.Bounds = V024Shishio.CircleBounds;
                Arena.Center = V024Shishio.CircleBounds.Center;
                _aoe = null;
            }
            else if (state == 0x00080004)
            {
                Arena.Bounds = V024Shishio.NormalBounds;
                Arena.Center = V024Shishio.ArenaCenter;
            }
        }
    }
}
