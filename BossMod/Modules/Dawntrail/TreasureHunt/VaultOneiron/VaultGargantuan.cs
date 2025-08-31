namespace BossMod.Dawntrail.TreasureHunt.VaultOneiron.VaultGargantuan;

public enum OID : uint
{
    VaultCrab = 0x4898, // R1.4
    VaultScorpion = 0x4899, // R2.4
    VaultGargantuan = 0x489A // R2.28
}

public enum AID : uint
{
    AutoAttack = 872, // VaultCrab/VaultScorpion/VaultGargantuan->player, no cast, single-target

    SmallClaw = 43611, // VaultCrab->player, no cast, single-target
    MoltenSilk = 43613, // VaultScorpion->self, 3.0s cast, range 9 270-degree cone
    FlyingPress = 43614, // VaultScorpion->location, 3.0s cast, range 6 circle
    Scoop = 43618, // VaultGargantuan->self, 4.0s cast, range 13 120-degree cone
    Brushfire = 43616, // VaultGargantuan->self, 5.0s cast, range 60 circle
    Inhale = 43617, // VaultGargantuan->self, 0.5s cast, range 20 120-degree cone, pull 25 between hitboxes
    Spin = 43619, // VaultGargantuan->self, 3.5s cast, range 11 circle
    Mash = 43615 // VaultGargantuan->player, 5.0s cast, single-target, tankbuster
}

sealed class MoltenSilk(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MoltenSilk, new AOEShapeCone(9f, 135f.Degrees()));
sealed class FlyingPress(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlyingPress, 6f);
sealed class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));
sealed class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
sealed class Mash(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.Mash);
sealed class Brushfire(BossModule module) : Components.RaidwideCast(module, (uint)AID.Brushfire);

sealed class VaultGargantuanStates : StateMachineBuilder
{
    public VaultGargantuanStates(VaultGargantuan module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MoltenSilk>()
            .ActivateOnEnter<FlyingPress>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Scoop>()
            .ActivateOnEnter<Brushfire>()
            .Raw.Update = () => AllDestroyed(VaultGargantuan.Trash) && (module.BossGargantuan?.IsDeadOrDestroyed ?? true);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.VaultCrab, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1060u, NameID = 13986u, Category = BossModuleInfo.Category.TreasureHunt, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 1)]
public sealed class VaultGargantuan(WorldState ws, Actor primary) : SharedBoundsBoss(ws, primary)
{
    public static readonly uint[] Trash = [(uint)OID.VaultCrab, (uint)OID.VaultScorpion];

    public Actor? BossGargantuan;

    protected override void UpdateModule()
    {
        BossGargantuan ??= GetActor((uint)OID.VaultGargantuan);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(BossGargantuan);
        Arena.Actors(this, Trash);
    }
}
