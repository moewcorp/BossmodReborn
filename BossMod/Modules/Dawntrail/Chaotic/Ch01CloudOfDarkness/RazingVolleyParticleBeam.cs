namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class RazingVolleyParticleBeam(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.RazingVolleyParticleBeam), new AOEShapeRect(45f, 4f))
{
    private DateTime _nextBundle;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
            return [];
        var deadline = Casters[0].Activation.AddSeconds(3d);

        var aoes = new AOEInstance[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var caster = Casters[i];
            if (caster.Activation > deadline)
                break;
            aoes[index++] = caster;
        }
        return aoes[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action == WatchedAction)
            NumCasts = 0;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction && WorldState.CurrentTime > _nextBundle)
        {
            ++NumCasts;
            _nextBundle = WorldState.FutureTime(1d);
        }
    }
}
