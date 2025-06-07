namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.DemonTablet;

class LacunateStream(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeRect rect = new(31f, 15f, 3f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LacunateStreamFirst)
            _aoe = new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LacunateStreamRepeat)
        {
            if (++NumCasts == 5)
            {
                _aoe = null;
                NumCasts = 0;
            }
        }
    }
}
