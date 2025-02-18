namespace BossMod.Dawntrail.Alliance.A11Prishe;

class NullifyingDropkick(BossModule module) : Components.CastSharedTankbuster(module, ActionID.MakeSpell(AID.NullifyingDropkick), 6f);
class Holy(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Holy), 6f);
class BanishgaIV(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BanishgaIV));
class Banishga(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Banishga));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13351, SortOrder = 2, PlanLevel = 100)]
public class A11Prishe(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultBounds)
{
    public static readonly WPos ArenaCenter = new(800f, 400f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(35f);
}
