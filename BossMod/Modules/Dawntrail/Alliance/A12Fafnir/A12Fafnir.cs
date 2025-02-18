namespace BossMod.Dawntrail.Alliance.A12Fafnir;

class DarkMatterBlast(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DarkMatterBlast))
{
    public override bool KeepOnPhaseChange => true;
}

class HurricaneWingRW(BossModule module) : Components.RaidwideCastDelay(module, ActionID.MakeSpell(AID.HurricaneWingRaidwide), ActionID.MakeSpell(AID.HurricaneWingRaidwideAOE1), 2.7f, "Raidwide x9")
{
    public override bool KeepOnPhaseChange => true;
}

class PestilentSphere(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.PestilentSphere))
{
    public override bool KeepOnPhaseChange => true;
}

class ShudderingEarth(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.ShudderingEarth));

class Darter(BossModule module) : Components.Adds(module, (uint)OID.Darter, 1)
{
    public override bool KeepOnPhaseChange => true;
}
class Venom(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Venom), new AOEShapeCone(30f, 60f.Degrees()))
{
    public override bool KeepOnPhaseChange => true;
}

class AbsoluteTerror(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AbsoluteTerrorAOE), new AOEShapeRect(70f, 10f))
{
    public override bool KeepOnPhaseChange => true;
}

class WingedTerror(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WingedTerrorAOE), new AOEShapeRect(70f, 12.5f))
{
    public override bool KeepOnPhaseChange => true;
}

class BalefulBreath(BossModule module) : Components.LineStack(module, (uint)IconID.BalefulBreath, ActionID.MakeSpell(AID.BalefulBreathAOERest), 8.2f, 70f, 3f, PartyState.MaxAllianceSize, PartyState.MaxAllianceSize, 3, false)
{
    public override bool KeepOnPhaseChange => true;
}

class SharpSpike(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(4), (uint)IconID.SharpSpike, ActionID.MakeSpell(AID.SharpSpikeAOE), 6.2f, true)
{
    public override bool KeepOnPhaseChange => true;
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13662, SortOrder = 4, PlanLevel = 100)]
public class A12Fafnir(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsCircle(34.5f))
{
    public static readonly WPos ArenaCenter = new(-500f, 600f);
    public static readonly ArenaBoundsCircle DefaultBounds = new(30f);
    public static readonly ArenaBoundsCircle FireArena = new(16f);
}
