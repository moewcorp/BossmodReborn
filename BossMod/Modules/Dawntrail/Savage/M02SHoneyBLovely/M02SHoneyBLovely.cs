namespace BossMod.Dawntrail.Savage.M02SHoneyBLovely;

class StingingSlash(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(50, 45.Degrees()), (uint)IconID.StingingSlash, ActionID.MakeSpell(AID.StingingSlashAOE));
class KillerSting(BossModule module) : Components.IconSharedTankbuster(module, (uint)IconID.KillerSting, ActionID.MakeSpell(AID.KillerStingAOE), 6);
class BlindingLoveBait(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BlindingLoveBaitAOE), new AOEShapeRect(50, 4))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = ActiveCasters.Select((c, index) =>
            new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo),
            index < 2 ? Colors.Danger : Colors.AOE));

        return aoes;
    }
}

class BlindingLoveCharge(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(45, 5));
class BlindingLoveCharge1(BossModule module) : BlindingLoveCharge(module, AID.BlindingLoveCharge1AOE);
class BlindingLoveCharge2(BossModule module) : BlindingLoveCharge(module, AID.BlindingLoveCharge2AOE);

class PoisonStingBait(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.PoisonStingAOE), new AOEShapeCircle(6), true);
class PoisonStingVoidzone(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID.PoisonStingVoidzone).Where(z => z.EventState != 7));
class BeeSting(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.BeeStingAOE), 6, 4, 4);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 988, NameID = 12685, PlanLevel = 100)]
public class M02SHoneyBLovely(WorldState ws, Actor primary) : Raid.M02NHoneyBLovely.M02NHoneyBLovely(ws, primary);