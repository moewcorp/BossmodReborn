namespace BossMod.Shadowbringers.Foray.Duel.Duel3Sartauvoir;

class Meltdown(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCircle circle = new(5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Meltdown)
        {
            if (_aoe == null)
                _aoe = new(circle, caster.Position, default);
            if (++NumCasts == 8)
            {
                _aoe = null;
                NumCasts = 0;
            }
        }
    }
}
