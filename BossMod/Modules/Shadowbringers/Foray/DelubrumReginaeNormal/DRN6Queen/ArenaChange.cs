namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN6Queen;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EmpyreanIniquity && Arena.Bounds == Queen.StartingArena)
        {
            _aoe = new(Queen.ArenaChange, Arena.Center, default, Module.CastFinishAt(spell, 4.8d));
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x19)
        {
            if (state is 0x00020001u or 0x00400001u)
            {
                Arena.Bounds = Queen.DefaultArena;
                _aoe = null;
            }
            else if (state == 0x00200010u)
            {
                Arena.Bounds = Queen.SquareArena;
                _aoe = null;
            }
        }
    }
}
