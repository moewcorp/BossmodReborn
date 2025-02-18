namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker;

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(25, 30);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.VerdantTempest && Arena.Bounds == TrinitySeeker.StartingArena)
            _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell, 3.8f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x1B)
        {
            Arena.Bounds = TrinitySeeker.DefaultArena;
            Arena.Center = TrinitySeeker.DefaultArena.Center;
            _aoe = null;
        }
    }
}
