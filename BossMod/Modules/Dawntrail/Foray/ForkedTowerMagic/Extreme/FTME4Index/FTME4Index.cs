namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME4Index;

sealed class ArenaChange(BossModule module) : BossComponent(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00)
        {
            switch (state)
            {
                case 0x00020001u:
                    var arenaFull = Normal.FTMN4Index.FTMN4Index.BuildFullArena();
                    Arena.Bounds = arenaFull.arena;
                    Arena.Center = arenaFull.center;
                    break;
                case 0x00080004u:
                    var arenaInitial = Normal.FTMN4Index.FTMN4Index.BuildInitialArena();
                    Arena.Bounds = arenaInitial.arena;
                    Arena.Center = arenaInitial.center;
                    break;
            }
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Dummy, PrimaryActorOID = (uint)OID.Index, Contributors = "", Category = BossModuleInfo.Category.Foray, GroupType = BossModuleInfo.GroupType.TheForkedTowerMagicExtreme,
    GroupID = 1114u, NameID = 14717u, SortOrder = 4, PlanLevel = 100)]
[SkipLocalsInit]
public sealed class FTME4Index : BossModule
{
    public FTME4Index(WorldState ws, Actor primary) : this(ws, primary, Normal.FTMN4Index.FTMN4Index.BuildInitialArena()) { }

    private FTME4Index(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 28f);
}
