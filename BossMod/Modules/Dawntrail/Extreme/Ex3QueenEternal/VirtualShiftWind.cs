namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

class Aeroquell(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.Aeroquell), 5f, 4);
class AeroquellTwister(BossModule module) : Components.PersistentVoidzone(module, 5f, GetTwister)
{
    private static List<Actor> GetTwister(BossModule module) => module.Enemies((uint)OID.Twister);
}
class MissingLink(BossModule module) : Components.Chains(module, (uint)TetherID.MissingLink, default, 25f);

class WindOfChange(BossModule module) : Components.Knockback(module, ActionID.MakeSpell(AID.WindOfChange), true)
{
    private readonly Angle[] _directions = new Angle[PartyState.MaxPartySize];
    private DateTime _activation;

    public override ReadOnlySpan<Source> ActiveSources(int slot, Actor actor)
    {
        if (_directions[slot] != default)
            return new Source[1] { new(actor.Position, 20f, _activation, null, _directions[slot], Kind.DirForward) };
        return [];
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var dir = status.ID switch
        {
            (uint)SID.WestWindOfChange => 90f.Degrees(),
            (uint)SID.EastWindOfChange => -90f.Degrees(),
            _ => default
        };
        if (dir != default && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _directions[slot] = dir;
            _activation = status.ExpireAt;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            if (Raid.FindSlot(spell.MainTargetID) is var slot && slot >= 0)
                _directions[slot] = default;
        }
    }
}
