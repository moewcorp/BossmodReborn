namespace BossMod.Endwalker.VariantCriterion.V02MR.V022Moko;

class AzureAuspice(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AzureAuspice), new AOEShapeDonut(6, 60));
class KenkiRelease(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.KenkiRelease));
class IronRain(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.IronRain), 10);
class Unsheathing(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Unsheathing), 3);
class VeilSever(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.VeilSever), new AOEShapeRect(40, 2.5f));
class ScarletAuspice(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ScarletAuspice), 6);
class MoonlessNight(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MoonlessNight));
class Clearout(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Clearout), new AOEShapeCone(22, 90.Degrees()));

abstract class Boundless(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(60, 5));
class BoundlessScarlet(BossModule module) : Boundless(module, AID.BoundlessScarlet);
class BoundlessAzure(BossModule module) : Boundless(module, AID.BoundlessAzure);

class Explosion(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Explosion), new AOEShapeRect(60, 15), 2)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
            return [];
        var hasDifferentRotations = count > 1 && Casters[0].Rotation != Casters[1].Rotation;
        var max = count > MaxCasts ? MaxCasts : count;
        List<AOEInstance> result = new(max);
        for (var i = 0; i < max; ++i)
        {
            var caster = Casters[i];
            result.Add(caster with { Color = i == 0 && count > i ? Colors.Danger : 0, Risky = caster.Origin == Casters[0].Origin || hasDifferentRotations });
        }
        return result;
    }
}

class GhastlyGrasp(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GhastlyGrasp), 5);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12357, SortOrder = 2)]
public class V022MokoOtherPaths(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChange.ArenaCenter, ArenaChange.StartingBounds);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", PrimaryActorOID = (uint)OID.BossP2, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12357, SortOrder = 3)]
public class V022MokoPath2(WorldState ws, Actor primary) : V022MokoOtherPaths(ws, primary);
