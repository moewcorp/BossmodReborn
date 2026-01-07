//TODO: Account for Dead Wake's shortening arena.

namespace BossMod.Modules.Dawntrail.Raid.M09NVampFatale;
sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    private static readonly AOEShapeRect rect = new(50f, 5f);
    private static readonly WPos left = new(85f, 80f);
    private static readonly WPos right = new(115f, 80f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SadisticScreech)
        {
            _aoe = [new(rect, left, default, WorldState.FutureTime(7d)), new(rect, right, default, WorldState.FutureTime(7d))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00 && state == 0x00020001)
        {
            Arena.Bounds = new ArenaBoundsRect(10f, 20f);
            _aoe = [];
        }
        if ((index == 0x00 || index == 0x10) && state == 0x00080004)
        {
            Arena.Bounds = new ArenaBoundsSquare(20f);
            _aoe = [];
        }
        if (index == 0x10 && state == 0x00020001)
        {
            Arena.Bounds = new ArenaBoundsCircle(20f);
        }
    }
}
