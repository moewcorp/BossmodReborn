namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

class Guillotine(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeCone _shape = new(40f, 120f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Guillotine)
        {
            _aoe = new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 0.6f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.GuillotineAOE or (uint)AID.GuillotineAOELast)
        {
            ++NumCasts;
            if (spell.Action.ID == (uint)AID.GuillotineAOELast)
                _aoe = null;
        }
    }
}
