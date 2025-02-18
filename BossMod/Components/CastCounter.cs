namespace BossMod.Components;

// generic component that counts specified casts
public class CastCounter(BossModule module, ActionID aid) : BossComponent(module)
{
    public readonly ActionID WatchedAction = aid;
    public int NumCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            ++NumCasts;
    }
}

public class CastCounterMulti(BossModule module, HashSet<ActionID> aids) : BossComponent(module)
{
    public readonly HashSet<ActionID> WatchedActions = aids;
    public int NumCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (WatchedActions.Contains(spell.Action))
            ++NumCasts;
    }
}
