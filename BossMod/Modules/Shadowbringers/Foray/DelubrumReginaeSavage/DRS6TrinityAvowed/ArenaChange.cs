namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS6TrinityAvowed;

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GloryOfBozja && Arena.Bounds == TrinityAvowed.StartingArena)
            _aoe = new(TrinityAvowed.ArenaChange, Arena.Center, default, Module.CastFinishAt(spell, 0.7f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001u && index == 0x11u)
        {
            Arena.Bounds = TrinityAvowed.DefaultArena;
            _aoe = null;
        }
    }
}
