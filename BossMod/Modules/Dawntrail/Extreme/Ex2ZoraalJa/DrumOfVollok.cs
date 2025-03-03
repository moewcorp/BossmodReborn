namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

class DrumOfVollokPlatforms(BossModule module) : BossComponent(module)
{
    public bool Active;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index != 11)
            return;

        switch (state)
        {
            case 0x00800040:
                Arena.Bounds = Ex2ZoraalJa.NWPlatformBounds;
                Arena.Center += 15f * 135f.Degrees().ToDirection();
                Active = true;
                break;
            case 0x02000100:
                Arena.Bounds = Ex2ZoraalJa.NEPlatformBounds;
                Arena.Center += 15f * (-135f).Degrees().ToDirection();
                Active = true;
                break;
        }
    }
}

class DrumOfVollok(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DrumOfVollokAOE), 4f, 2, 2);

class DrumOfVollokKnockback(BossModule module) : Components.Knockback(module, ignoreImmunes: true)
{
    private readonly DrumOfVollok? _main = module.FindComponent<DrumOfVollok>();

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_main == null)
            return [];
        var count = _main.Stacks.Count;
        for (var i = 0; i < count; ++i)
        {
            if (_main.Stacks[i].Target == actor)
                return [];
        }
        var sources = new List<Source>();
        for (var i = 0; i < count; ++i)
        {
            var s = _main.Stacks[i];
            if (actor.Position.InCircle(s.Target.Position, s.Radius))
                sources.Add(new(s.Target.Position, 25f, s.Activation));
        }
        return sources;
    }
}
