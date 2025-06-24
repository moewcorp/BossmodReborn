namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS5Phantom;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.ArenaFeatures)
        {
            if (state == 0x00010002u)
                _aoe = new(Phantom.ArenaChange, Arena.Center, default, WorldState.FutureTime(4f));
            else if (state == 0x00080010u)
            {
                _aoe = null;
                Arena.Bounds = Phantom.DefaultArena;
                Arena.Center = Phantom.DefaultCenter;
            }
        }
    }
}
