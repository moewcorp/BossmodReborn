namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(25, 35);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Cloudsplitter)
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
