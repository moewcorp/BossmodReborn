namespace BossMod.Heavensward.Alliance.A34Scathach;

class ThirtyCries(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.ThirtyCries), new AOEShapeCircle(12));
class ThirtyThorns4(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ThirtyThorns4), 8);
class ThirtySouls(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ThirtySouls));
class ThirtyArrows2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ThirtyArrows2), new AOEShapeRect(35.5f, 4));
class ThirtyArrows1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ThirtyArrows1), 8);
class TheDragonsVoice(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TheDragonsVoice), 30);

class Shadespin2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Shadespin2), new AOEShapeCone(30, 45.Degrees()));

class Shadesmite1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Shadesmite1), 15);
class Shadesmite2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Shadesmite2), 3);
class Shadesmite3(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Shadesmite3), 3);

class Pitfall(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Pitfall));
class FullSwing(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FullSwing));

class Nox(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(10), ActionID.MakeSpell(AID.NoxAOEFirst), ActionID.MakeSpell(AID.NoxAOERest), 5.5f, 1.6f, 5, true);

abstract class MarrowDrain(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(10.44f, 60.Degrees()));
class MarrowDrain1(BossModule module) : MarrowDrain(module, AID.MarrowDrain1);
class MarrowDrain2(BossModule module) : MarrowDrain(module, AID.MarrowDrain2);

class BigHug(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BigHug), new AOEShapeRect(5.25f, 1.5f));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 220, NameID = 5515)]
public class A34Scathach(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -50), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.Connla));
        Arena.Actors(Enemies(OID.Connla2));
        Arena.Actors(Enemies(OID.ShadowLimb));
        Arena.Actors(Enemies(OID.ShadowcourtJester));
        Arena.Actors(Enemies(OID.ChimeraPoppet));
        Arena.Actors(Enemies(OID.ShadowcourtHound));
    }
}
