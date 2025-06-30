namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL4DiabloArmament;

sealed class AdvancedDeathIV(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AdvancedDeathIV, 10f);

class AdvancedNox(BossModule module) : Components.StandardChasingAOEs(module, 10f, (uint)AID.AdvancedNoxAOEFirst, (uint)AID.AdvancedNoxAOERest, 5.5f, 1.6f, 5, true);

sealed class AccelerationBomb(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.AccelerationBomb && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.AccelerationBomb && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            PlayerStates[slot] = default;
        }
    }
}

sealed class AssaultCannon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AssaultCannon, new AOEShapeRect(100f, 3f));
sealed class DeadlyDealingAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeadlyDealingAOE, 6f);
sealed class AdvancedDeathRay(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(70f, 4f), (uint)IconID.AdvancedDeathRay, (uint)AID.AdvancedDeathRay, 5.9f, tankbuster: true);
sealed class PillarOfShamashBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(70f, 2f), (uint)IconID.PillarOfShamash, (uint)AID.PillarOfShamashBait);
sealed class PillarOfShamashStack(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.PillarOfShamashMarker, (uint)AID.PillarOfShamashStack, 6f, 70f, 4f, 24, 24, markerIsFinalTarget: true);
sealed class Explosion(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module, [(uint)AID.Explosion1, (uint)AID.Explosion2,
(uint)AID.Explosion3, (uint)AID.Explosion4, (uint)AID.Explosion5, (uint)AID.Explosion6, (uint)AID.Explosion7, (uint)AID.Explosion8, (uint)AID.Explosion9],
new AOEShapeRect(60f, 11f), 4d);

sealed class LightPseudopillarAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightPseudopillarAOE, 10f);

sealed class PillarOfShamash(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module, [(uint)AID.PillarOfShamashCone1, (uint)AID.PillarOfShamashCone2,
(uint)AID.PillarOfShamashCone3], new AOEShapeCone(70f, 10f.Degrees()), expectedNumCasters: 9);
sealed class UltimatePseudoterror(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UltimatePseudoterror, new AOEShapeDonut(15f, 70f));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.TheDalriada, GroupID = 778, NameID = 10007, SortOrder = 5)]
public sealed class DAL4DiabloArmament(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultArena)
{
    public static readonly WPos ArenaCenter = new(-720f, -760f);
    public static readonly ArenaBoundsComplex DefaultArena = new([new Polygon(ArenaCenter, 29.5f, 48)]);
    public static readonly ArenaBoundsCircle SmallArena = new(17); // this is a pulsing donut aoe
}
