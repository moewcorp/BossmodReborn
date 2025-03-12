namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN2Dahu;

class Shockwave(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone _shape = new(15f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.LeftSidedShockwaveFirst or (uint)AID.RightSidedShockwaveFirst)
        {
            _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation + 180f.Degrees(), Module.CastFinishAt(spell, 2.6f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.LeftSidedShockwaveFirst or (uint)AID.RightSidedShockwaveFirst or (uint)AID.LeftSidedShockwaveSecond or (uint)AID.RightSidedShockwaveSecond)
            _aoes.RemoveAt(0);
    }
}
