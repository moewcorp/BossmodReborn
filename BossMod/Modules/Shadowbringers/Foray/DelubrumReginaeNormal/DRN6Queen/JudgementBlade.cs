namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN6Queen;

class JudgmentBlade(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeRect rect = new(70f, 15f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.JudgmentBladeRAOE or (uint)AID.JudgmentBladeLAOE)
            _aoe = new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.JudgmentBladeRAOE or (uint)AID.JudgmentBladeLAOE)
        {
            _aoe = null;
            ++NumCasts;
        }
    }
}
