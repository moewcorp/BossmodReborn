namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

class NorthernCross(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance? _aoe;
    private static readonly AOEShapeRect _shape = new(25f, 30f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index != 0x02)
            return;
        var offset = state switch
        {
            0x00200010 => -90f.Degrees(),
            0x00020001 => 90f.Degrees(),
            _ => default
        };
        if (offset != default)
            _aoe = new(_shape, Arena.Center, -126.875f.Degrees() + offset, WorldState.FutureTime(9.2d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NorthernCross1 or (uint)AID.NorthernCross2)
            _aoe = null;
    }
}
