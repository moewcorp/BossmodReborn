namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN6Queen;

class HeavensWrathAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavensWrathKnockback, new AOEShapeRect(60f, 50f));

// TODO: generalize
class HeavensWrathKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> _sources = new(2);
    private static readonly AOEShapeCone _shape = new(30f, 90f.Degrees());

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(_sources);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HeavensWrathKnockback)
        {
            _sources.Clear();
            var act = Module.CastFinishAt(spell);
            _sources.Add(new(caster.Position, 20f, act, _shape, spell.Rotation + 90f.Degrees(), Kind.DirForward));
            _sources.Add(new(caster.Position, 20f, act, _shape, spell.Rotation - 90f.Degrees(), Kind.DirForward));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HeavensWrathKnockback)
        {
            _sources.Clear();
            ++NumCasts;
        }
    }
}
