namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class DireStraits(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);

    private static readonly AOEShapeRect _shape = new(40f, 40f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe with { Risky = false };
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DireStraitsVisualFirst or (uint)AID.DireStraitsVisualSecond)
        {
            _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 4.8f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DireStraitsAOEFirst or (uint)AID.DireStraitsAOESecond)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
                _aoes.RemoveAt(0);
        }
    }
}

class NavigatorsTridentAOE(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.NavigatorsTridentAOE), new AOEShapeRect(40f, 5f));

class NavigatorsTridentKnockback(BossModule module) : Components.Knockback(module)
{
    private readonly SerpentsTide? _serpentsTide = module.FindComponent<SerpentsTide>();
    private readonly List<Source> _sources = new(2);

    private static readonly AOEShapeCone _shape = new(30f, 90f.Degrees());

    public override IEnumerable<Source> Sources(int slot, Actor actor) => _sources;
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos) || (_serpentsTide?.AOEs.Any(z => z.Check(pos)) ?? false);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NavigatorsTridentAOE)
        {
            _sources.Clear();
            _sources.Add(new(spell.LocXZ, 20, Module.CastFinishAt(spell), _shape, spell.Rotation + 90f.Degrees(), Kind.DirForward));
            _sources.Add(new(spell.LocXZ, 20, Module.CastFinishAt(spell), _shape, spell.Rotation - 90f.Degrees(), Kind.DirForward));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NavigatorsTridentAOE)
        {
            _sources.Clear();
            ++NumCasts;
        }
    }
}
