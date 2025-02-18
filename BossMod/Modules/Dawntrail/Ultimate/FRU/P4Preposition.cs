namespace BossMod.Dawntrail.Ultimate.FRU;

// boss can spawn either N or S from center
class P4Preposition(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _boss = module.Enemies(OID.UsurperOfFrostP4);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < _boss.Count; ++i)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(_boss[i].Position, 8), DateTime.MaxValue);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        for (var i = 0; i < _boss.Count; ++i)
            Arena.AddCircle(_boss[i].Position, 1, Colors.Safe);
    }
}

// utility to draw hitbox around crystal, so that it's easier not to clip
class P4FragmentOfFate(BossModule module) : BossComponent(module)
{
    private readonly IReadOnlyList<Actor> _fragment = module.Enemies(OID.FragmentOfFate);

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (playerSlot >= PartyState.MaxPartySize)
        {
            customColor = Colors.Object;
            return PlayerPriority.Danger;
        }
        return PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var f in _fragment)
            Arena.AddCircle(f.Position, f.HitboxRadius, Colors.Object);
    }
}
