namespace BossMod.Heavensward.Alliance.A36DiabolosHollow;

class Shadethrust(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Shadethrust), new AOEShapeRect(43, 2.5f));
class HollowCamisado(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.HollowCamisado));
class HollowNightmare(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.HollowNightmare));
class HollowOmen1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HollowOmen1));
class HollowOmen2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HollowOmen2));
class Blindside(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.Blindside), 6, 8);
class EarthShaker2(BossModule module) : Components.SimpleProtean(module, ActionID.MakeSpell(AID.EarthShaker2), new AOEShapeCone(60, 15.Degrees()));
class HollowNight(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.HollowNight), 8, 8);
class HollowNightGaze(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.HollowNight));
class ParticleBeam2(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.ParticleBeam2), 5);
class ParticleBeam4(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.ParticleBeam4), 5);

class Nox(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(10), ActionID.MakeSpell(AID.NoxAOEFirst), ActionID.MakeSpell(AID.NoxAOERest), 5.5f, 1.6f, 5, true)
{
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 220, NameID = 5526)]
public class A36DiabolosHollow(WorldState ws, Actor primary) : BossModule(ws, primary, new(-350, -445), new ArenaBoundsCircle(35))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.Deathgate));
        Arena.Actors(Enemies(OID.DiabolicGate));
        Arena.Actors(Enemies(OID.Shadowsphere));
        Arena.Actors(Enemies(OID.NightHound));
    }
}
