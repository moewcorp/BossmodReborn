namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

sealed class DrumOfVollokPlatforms(BossModule module) : BossComponent(module)
{
    public bool Active;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x0B)
        {
            return;
        }
        var a45 = 45f.Degrees();
        switch (state)
        {
            case 0x00800040u:
                var arenaNW = new ArenaBoundsCustom([new Square(new(121.2132f, 78.7868f), 10f, a45), new Square(Arena.Center, 10f, a45)], ScaleFactor: 1.24f);
                Arena.Bounds = arenaNW;
                Arena.Center = arenaNW.Center;
                Active = true;
                break;
            case 0x02000100u:
                var arenaNE = new ArenaBoundsCustom([new Square(new(78.7868f, 78.7868f), 10f, a45), new Square(Arena.Center, 10f, a45)], ScaleFactor: 1.24f);
                Arena.Bounds = arenaNE;
                Arena.Center = arenaNE.Center;
                Active = true;
                break;
        }
    }
}

sealed class DrumOfVollok(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.DrumOfVollokAOE, 4f, 2, 2);

sealed class DrumOfVollokKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private readonly DrumOfVollok? _main = module.FindComponent<DrumOfVollok>();

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_main == null)
        {
            return [];
        }
        var count = _main.Stacks.Count;
        for (var i = 0; i < count; ++i)
        {
            if (_main.Stacks[i].Target == actor)
            {
                return [];
            }
        }
        var sources = new List<Knockback>();
        for (var i = 0; i < count; ++i)
        {
            var s = _main.Stacks[i];
            if (actor.Position.InCircle(s.Target.Position, s.Radius))
            {
                sources.Add(new(s.Target.Position, 25f, s.Activation, ignoreImmunes: true));
            }
        }
        return CollectionsMarshal.AsSpan(sources);
    }
}
