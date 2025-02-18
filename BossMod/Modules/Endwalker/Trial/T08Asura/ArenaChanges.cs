namespace BossMod.Endwalker.Trial.T08Asura;

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(19, 20);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LowerRealm && Arena.Bounds == T08Asura.StartingArena)
            _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell, 0.8f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x01)
        {
            Arena.Bounds = T08Asura.DefaultArena;
            _aoe = null;
        }
    }
}
