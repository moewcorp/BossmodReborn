namespace BossMod.Heavensward.Alliance.A14Echidna;

class SickleStrike(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.SickleStrike));

abstract class SickleSlash(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(18.5f, 30));
class SickleSlash1(BossModule module) : SickleSlash(module, AID.SickleSlash1);
class SickleSlash2(BossModule module) : SickleSlash(module, AID.SickleSlash2);

class AbyssalReaper(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AbyssalReaper), 14);
class AbyssalReaperKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.AbyssalReaper), 5, stopAtWall: true);
class Petrifaction1(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.Petrifaction1));
class Petrifaction2(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.Petrifaction2));
class Gehenna(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Gehenna));
class BloodyHarvest(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BloodyHarvest), 12);
class Deathstrike(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Deathstrike), new AOEShapeRect(62, 3));
class FlameWreath(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FlameWreath), 18);
class SerpentineStrike(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SerpentineStrike), 20);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 120, NameID = 4631)]
public class A14Echidna(WorldState ws, Actor primary) : BossModule(ws, primary, new(288, -126), new ArenaBoundsCircle(29.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.Dexter));
        Arena.Actors(Enemies(OID.Sinister));
        Arena.Actors(Enemies(OID.Bloodguard));
    }
}