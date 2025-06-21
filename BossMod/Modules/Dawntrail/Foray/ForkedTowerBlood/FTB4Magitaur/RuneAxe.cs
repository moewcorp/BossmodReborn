namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB4Magitaur;

sealed class RuneAxeStatus(BossModule module) : BossComponent(module)
{
    private int numStatuses;
    public readonly List<(int Order, Actor Actor)> StatusBig = [];
    public readonly List<(int Order, Actor Actor)> StatuSmall = [];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var order = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds switch
        {
            < 15d => 1,
            _ => 2
        };
        switch (status.ID)
        {
            case (uint)SID.PreyGreaterAxebit:
                StatusBig.Add((order, actor));
                ++numStatuses;
                break;
            case (uint)SID.PreyLesserAxebit:
                StatuSmall.Add((order, actor));
                ++numStatuses;
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (numStatuses > 7)
        {
            hints.Add($"Too many targets, mechanic potentially unsolveable!");
        }
    }
}

sealed class RuneAxeAOEs(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RuneAxe)
        {
            _aoes.Add(new(FTB4Magitaur.CircleMinusSquares, Arena.Center, default, Module.CastFinishAt(spell)));
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID is (uint)SID.PreyGreaterAxebit or (uint)SID.PreyLesserAxebit)
        {
            _aoes.Clear();
        }
    }
}
