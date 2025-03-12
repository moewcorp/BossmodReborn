namespace BossMod.Endwalker.Unreal.Un3Sophia;

class ArmsOfWisdom(BossModule module) : Components.Knockback(module, ActionID.MakeSpell(AID.ArmsOfWisdom))
{
    private Actor? _caster;

    public override ReadOnlySpan<Source> ActiveSources(int slot, Actor actor)
    {
        if (_caster?.CastInfo?.TargetID == actor.InstanceID)
            return new Source[1] { new(_caster.Position, 5, Module.CastFinishAt(_caster.CastInfo)) };
        return [];
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_caster != null && _caster.CastInfo?.TargetID == _caster.TargetID)
        {
            // tank swap hints
            if (_caster.TargetID == actor.InstanceID)
                hints.Add("Pass aggro to co-tank!");
            else if (actor.Role == Role.Tank)
                hints.Add("Taunt!");
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _caster = caster;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _caster = null;
    }
}
