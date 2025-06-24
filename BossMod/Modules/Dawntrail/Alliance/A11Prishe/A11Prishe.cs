﻿namespace BossMod.Dawntrail.Alliance.A11Prishe;

sealed class NullifyingDropkick(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.NullifyingDropkick, 6f);
sealed class Holy(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Holy, 6f);
sealed class BanishgaIV(BossModule module) : Components.RaidwideCast(module, (uint)AID.BanishgaIV);
sealed class Banishga(BossModule module) : Components.RaidwideCast(module, (uint)AID.Banishga);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13351, SortOrder = 2, PlanLevel = 100)]
public sealed class A11Prishe(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultBounds)
{
    public static readonly WPos ArenaCenter = new(800f, 400f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(35f);
}
