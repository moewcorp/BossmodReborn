namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class Break(BossModule module) : Components.GenericGaze(module)
{
    public readonly List<Eye> Eyes = new(3);

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor) => Eyes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.BreakBoss or (uint)AID.BreakEye)
            Eyes.Add(new(spell.LocXZ, Module.CastFinishAt(spell, 0.9f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BreakBossAOE or (uint)AID.BreakEyeAOE)
        {
            var count = Eyes.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var eye = Eyes[i];
                if (eye.Position == pos)
                {
                    Eyes.Remove(eye);
                    break;
                }
            }
        }
    }
}
