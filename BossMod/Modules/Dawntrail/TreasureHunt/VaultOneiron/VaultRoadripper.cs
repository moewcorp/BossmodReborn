namespace BossMod.Dawntrail.TreasureHunt.VaultOneiron.VaultRoadripper;

public enum OID : uint
{
    VaultBot = 0x48A1, // R2.0
    VaultAerostat = 0x48A2, // R2.76
    VaultRoadripper = 0x48A3 // R2.56
}

public enum AID : uint
{
    AutoAttack1 = 43634, // VaultBot->player, no cast, single-target
    AutoAttack2 = 872, // VaultAerostat/VaultRoadripper->player, no cast, single-target

    HomingShot = 43635, // VaultBot->location, 3.0s cast, range 6 circle
    ThrownFlames = 43636, // VaultAerostat->self, 3.0s cast, range 8 circle
    IncendiaryCircle = 43637, // VaultAerostat->self, 3.0s cast, range 3-12 donut
    RunAmok = 43640, // VaultRoadripper->player, 4.0s cast, width 8 rect charge
    WheelingShot = 43641, // VaultRoadripper->self, 5.0s cast, range 40 180-degree cone
    Electroflame = 43639, // VaultRoadripper->self, 5.0s cast, range 40 circle
    Wheel = 43638, // VaultRoadripper->player, 5.0s cast, single-target
}

sealed class HomingShot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HomingShot, 6f);
sealed class ThrownFlames(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThrownFlames, 8f);
sealed class IncendiaryCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IncendiaryCircle, new AOEShapeDonut(3f, 12f));
sealed class RunAmok(BossModule module) : Components.BaitAwayChargeCast(module, (uint)AID.RunAmok, 4f);
sealed class Wheel(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.Wheel);
sealed class WheelingShot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WheelingShot, new AOEShapeCone(40f, 90f.Degrees()));
sealed class Electroflame(BossModule module) : Components.RaidwideCast(module, (uint)AID.Electroflame);

sealed class VaultRoadripperStates : StateMachineBuilder
{
    public VaultRoadripperStates(VaultRoadripper module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HomingShot>()
            .ActivateOnEnter<ThrownFlames>()
            .ActivateOnEnter<IncendiaryCircle>()
            .ActivateOnEnter<RunAmok>()
            .ActivateOnEnter<Wheel>()
            .ActivateOnEnter<WheelingShot>()
            .ActivateOnEnter<Electroflame>()
            .Raw.Update = () => AllDestroyed(VaultRoadripper.Trash) && (module.BossRoadripper?.IsDeadOrDestroyed ?? true);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.VaultBot, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1060u, NameID = 13995u, Category = BossModuleInfo.Category.TreasureHunt, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 3)]
public sealed class VaultRoadripper(WorldState ws, Actor primary) : SharedBoundsBoss(ws, primary)
{
    public static readonly uint[] Trash = [(uint)OID.VaultBot, (uint)OID.VaultAerostat];

    public Actor? BossRoadripper;

    protected override void UpdateModule()
    {
        BossRoadripper ??= GetActor((uint)OID.VaultRoadripper);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(BossRoadripper);
        Arena.Actors(this, Trash);
    }
}
