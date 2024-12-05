namespace BossMod.Dawntrail.Alliance.A12Fafnir;

class DarkMatterBlast(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DarkMatterBlast));
class HurricaneWingRW(BossModule module) : Components.RaidwideCastDelay(module, ActionID.MakeSpell(AID.HurricaneWingRaidwide), ActionID.MakeSpell(AID.HurricaneWingRaidwideAOE1), 2.7f, "Raidwide x9");
class Venom(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Venom), new AOEShapeCone(30, 60.Degrees()));
class PestilentSphere(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.PestilentSphere));
class WingedTerror(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WingedTerrorAOE), new AOEShapeRect(70, 12.5f));
class AbsoluteTerror(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AbsoluteTerrorAOE), new AOEShapeRect(70, 10));
class BalefulBreath(BossModule module) : Components.LineStack(module, (uint)IconID.LineStack, ActionID.MakeSpell(AID.BalefulBreathAOERest), 8.2f, 70, 3, PartyState.MaxAllianceSize, PartyState.MaxAllianceSize, 3, false);
class SharpSpike(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(4), (uint)IconID.Tankbuster, ActionID.MakeSpell(AID.SharpSpikeAOE), 6.2f, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13662, SortOrder = 4)]
public class A12Fafnir(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsCircle(34.5f))
{
    public static readonly WPos ArenaCenter = new(-500, 600);
    public static readonly ArenaBoundsCircle DefaultBounds = new(30);
    public static readonly ArenaBoundsCircle FireArena = new(16);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly));
    }
}
