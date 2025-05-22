namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Gauntlet;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 778, NameID = 32, SortOrder = 1)] // NameID 9834
public class DAL1Gauntlet(WorldState ws, Actor primary) : BossModule(ws, primary, new(222f, -689f), DefaultArena)
{
    public static readonly ArenaBoundsSquare StartingArena = new(29.5f);
    public static readonly ArenaBoundsSquare DefaultArena = new(24f);
}
