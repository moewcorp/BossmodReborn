namespace BossMod.Components;

public abstract class CleansableDebuff(BossModule module, uint statusID, string noun = "Doom", string adjective = "doomed") : BossComponent(module)
{
    private readonly List<Actor> _affected = [];
    private static readonly ActionID esuna = ActionID.MakeSpell(ClassShared.AID.Esuna);
    private static readonly ActionID wardensPaean = ActionID.MakeSpell(BRD.AID.WardensPaean);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == statusID)
        {
            var count = _affected.Count;
            for (var i = 0; i < count; ++i) // some status effects can be applied multiple times, filter to avoid duplicate hints
            {
                if (_affected[i] == actor)
                {
                    return; // already exists in list
                }
            }
            _affected.Add(actor);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == statusID)
        {
            if (actor.FindStatus(statusID) == null) // verify that all instances of the status effect are gone
                _affected.Remove(actor);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = _affected.Count;
        if (count != 0)
        {
            var contains = false;
            for (var i = 0; i < count; ++i)
            {
                if (_affected[i] == actor)
                {
                    contains = true;
                    break;
                }
            }
            if (contains)
                if (!(actor.Role == Role.Healer || actor.Class == Class.BRD))
                    hints.Add($"You were {adjective}! Get cleansed fast.");
                else
                    hints.Add($"Cleanse yourself! ({noun}).");
            else if (actor.Role == Role.Healer || actor.Class == Class.BRD)
                for (var i = 0; i < count; ++i)
                    hints.Add($"Cleanse {_affected[i].Name}! ({noun})");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _affected.Count;
        if (count != 0)
            for (var i = 0; i < count; ++i)
            {
                var c = _affected[i];
                ActionID action = default;
                if (actor.Role == Role.Healer)
                    action = esuna;
                else if (actor.Class == Class.BRD)
                    action = wardensPaean;
                if (action != default)
                    hints.ActionsToExecute.Push(action, c, ActionQueue.Priority.High, castTime: action == esuna ? 1f : default);
            }
    }
}
