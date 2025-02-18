namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

class CrushingHoof(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.CrushingHoofAOE))
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCircle circle = new(25);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CrushingHoof)
            _aoe = new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 1));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            _aoe = null;
    }
}
