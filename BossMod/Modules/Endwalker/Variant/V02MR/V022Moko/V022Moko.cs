namespace BossMod.Endwalker.VariantCriterion.V02MR.V022Moko;

sealed class AzureAuspice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AzureAuspice, new AOEShapeDonut(6f, 60f));
sealed class KenkiReleaseMoonlessNight(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.KenkiRelease, (uint)AID.MoonlessNight]);
sealed class IronRain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IronRain, 10f);
sealed class Unsheathing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Unsheathing, 3f);
sealed class VeilSever(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VeilSever, new AOEShapeRect(40f, 2.5f));
sealed class ScarletAuspice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ScarletAuspice, 6f);
sealed class Clearout(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Clearout, new AOEShapeCone(22f, 90f.Degrees()));
sealed class BoundlessScarletAzure(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BoundlessScarlet, (uint)AID.BoundlessAzure], new AOEShapeRect(60f, 5f));

sealed class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, new AOEShapeRect(60f, 15f), 2)
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(Casters);
        ref readonly var aoe0 = ref aoes[0];
        ref readonly var aoe1 = ref aoes[1];
        var hasDifferentRotations = count > 1 && aoe0.Rotation != aoe1.Rotation;
        var max = count > MaxCasts ? MaxCasts : count;

        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            aoe.Color = i == 0 && count > i ? Colors.Danger : default;
            aoe.Risky = i == 0 || hasDifferentRotations;
        }
        return aoes[..max];
    }
}

sealed class GhastlyGrasp(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GhastlyGrasp, 5);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12357, SortOrder = 2)]
public sealed class V022MokoOtherPaths(WorldState ws, Actor primary) : V022Moko(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", PrimaryActorOID = (uint)OID.BossP2, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12357, SortOrder = 3)]
public sealed class V022MokoPath2(WorldState ws, Actor primary) : V022Moko(ws, primary);

public abstract class V022Moko(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChange.ArenaCenter, ArenaChange.StartingBounds);
