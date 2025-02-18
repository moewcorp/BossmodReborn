namespace BossMod.Endwalker.VariantCriterion.C03AAI.C031Ketuduke;

class AngrySeasAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(40, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NAngrySeasAOE or AID.SAngrySeasAOE)
            _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
    }
}

// TODO: generalize
class AngrySeasKnockback(BossModule module) : Components.Knockback(module)
{
    private readonly List<Source> _sources = [];
    private static readonly AOEShapeCone _shape = new(30, 90.Degrees());

    public override IEnumerable<Source> Sources(int slot, Actor actor) => _sources;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NAngrySeasAOE or AID.SAngrySeasAOE)
        {
            _sources.Clear();
            var activation = Module.CastFinishAt(spell);
            // charge always happens through center, so create two sources with origin at center looking orthogonally
            _sources.Add(new(Arena.Center, 12, activation, _shape, spell.Rotation + 90.Degrees(), Kind.DirForward));
            _sources.Add(new(Arena.Center, 12, activation, _shape, spell.Rotation - 90.Degrees(), Kind.DirForward));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NAngrySeasAOE or AID.SAngrySeasAOE)
        {
            _sources.Clear();
            ++NumCasts;
        }
    }
}
