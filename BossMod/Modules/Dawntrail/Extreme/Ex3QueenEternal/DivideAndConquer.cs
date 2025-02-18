namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

class DivideAndConquerBait(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.DivideAndConquerBait))
{
    private static readonly AOEShapeRect _shape = new(60, 2.5f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DivideAndConquer && WorldState.Actors.Find(targetID) is var target && target != null)
            CurrentBaits.Add(new(actor, target, _shape, WorldState.FutureTime(3.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            if (CurrentBaits.Count != 0)
                CurrentBaits.RemoveAt(0);
        }
    }
}

class DivideAndConquerAOE(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.DivideAndConquerBait))
{
    private static readonly AOEShapeRect rect = new(60, 2.5f);
    public readonly List<AOEInstance> AOEs = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            AOEs.Add(new(rect, caster.Position, caster.Rotation, WorldState.FutureTime(11 - AOEs.Count)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DivideAndConquerAOE)
        {
            ++NumCasts;
            AOEs.Clear();
        }
    }
}
