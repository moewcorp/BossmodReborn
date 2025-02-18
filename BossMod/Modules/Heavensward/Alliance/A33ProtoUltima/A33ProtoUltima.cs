namespace BossMod.Heavensward.Alliance.A33ProtoUltima;

class AetherialPool(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.AetherialPool), 40, kind: Kind.TowardsOrigin, stopAtWall: true);
class AetherochemicalFlare(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AetherochemicalFlare));

abstract class AetherochemicalLaser(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(71, 4));
class AetherochemicalLaser1(BossModule module) : AetherochemicalLaser(module, AID.AetherochemicalLaser1);
class AetherochemicalLaser2(BossModule module) : AetherochemicalLaser(module, AID.AetherochemicalLaser2);
class AetherochemicalLaser3(BossModule module) : AetherochemicalLaser(module, AID.AetherochemicalLaser3);

class CitadelBuster2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CitadelBuster2), new AOEShapeRect(65.5f, 5));
class FlareStar(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FlareStar), 31);

class Rotoswipe(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Rotoswipe), new AOEShapeCone(11, 60.Degrees()));

class WreckingBall(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WreckingBall), 8);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 220, NameID = 3780)]
public class A33ProtoUltima(WorldState ws, Actor primary) : BossModule(ws, primary, new(-350, -50), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.AllaganDreadnaught));
        Arena.Actors(Enemies(OID.AetherCollector));
    }
}
