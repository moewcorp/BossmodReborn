namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS2StygimolochWarrior;

class UnrelentingCharge(BossModule module) : Components.Knockback(module)
{
    private Actor? _source;
    private DateTime _activation;

    public override ReadOnlySpan<Source> ActiveSources(int slot, Actor actor)
    {
        if (_source != null)
            return new Source[1] { new(_source.Position, 10, _activation) };
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.UnrelentingCharge)
        {
            _source = caster;
            _activation = Module.CastFinishAt(spell, 0.3f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.UnrelentingChargeAOE)
        {
            ++NumCasts;
            _activation = WorldState.FutureTime(1.6d);
        }
    }
}
