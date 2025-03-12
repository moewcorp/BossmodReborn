namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

class HeavensWrathAOE(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HeavensWrathVisual), new AOEShapeRect(60f, 50f));

// TODO: generalize
class HeavensWrathKnockback(BossModule module) : Components.Knockback(module)
{
    private readonly List<Source> _sources = new(2);
    private static readonly AOEShapeCone _shape = new(30f, 90f.Degrees());

    public override ReadOnlySpan<Source> ActiveSources(int slot, Actor actor) => CollectionsMarshal.AsSpan(_sources);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HeavensWrathKnockback)
        {
            _sources.Clear();
            var act = Module.CastFinishAt(spell);
            _sources.Add(new(caster.Position, 15f, act, _shape, spell.Rotation + 90f.Degrees(), Kind.DirForward));
            _sources.Add(new(caster.Position, 15f, act, _shape, spell.Rotation - 90f.Degrees(), Kind.DirForward));
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
