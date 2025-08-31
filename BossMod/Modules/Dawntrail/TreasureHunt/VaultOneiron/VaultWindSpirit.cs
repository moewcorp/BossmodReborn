namespace BossMod.Dawntrail.TreasureHunt.VaultOneiron.VaultWindSpirit;

public enum OID : uint
{
    VaultAgavoides = 0x489B, // R1.5
    VaultTree = 0x489C, // R1.6
    VaultWindSpirit = 0x489D // R2.4
}

public enum AID : uint
{
    AutoAttack = 872, // VaultAgavoides/VaultTree/VaultWindSpirit->player, no cast, single-target

    Leafcutter = 43620, // VaultAgavoides->self, 3.6s cast, range 14 width 4 rect
    Entangle = 43621, // VaultTree->location, 3.0s cast, range 4 circle
    Stump = 43622, // VaultTree->self, 3.0s cast, range 6 circle
    Gust = 43624, // VaultWindSpirit->location, 3.0s cast, range 6 circle
    WhirlingGaol = 43626, // VaultWindSpirit->self, 5.0s cast, range 40 circle, raidwide
    Whipwind = 43625, // VaultWindSpirit->self, 6.0s cast, range 60 width 40 rect
    ChangelessWinds = 43623 // VaultWindSpirit->self/player, 5.0s cast, range 40 width 8 rect, tankbuster
}

sealed class Leafcutter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Leafcutter, new AOEShapeRect(14f, 2f));
sealed class Entangle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Entangle, 4f);
sealed class StumpGust(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Stump, (uint)AID.Gust], 6f);
sealed class Whipwind(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Whipwind, new AOEShapeRect(60f, 20f));
sealed class ChangelessWinds(BossModule module) : Components.BaitAwayCast(module, (uint)AID.ChangelessWinds, new AOEShapeRect(40f, 4f), tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class WhirlingGaol(BossModule module) : Components.RaidwideCast(module, (uint)AID.WhirlingGaol);

sealed class VaultWindSpiritStates : StateMachineBuilder
{
    public VaultWindSpiritStates(VaultWindSpirit module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Leafcutter>()
            .ActivateOnEnter<Entangle>()
            .ActivateOnEnter<StumpGust>()
            .ActivateOnEnter<Whipwind>()
            .ActivateOnEnter<ChangelessWinds>()
            .ActivateOnEnter<WhirlingGaol>()
            .Raw.Update = () => AllDestroyed(VaultWindSpirit.Trash) && (module.BossWindSpirit?.IsDeadOrDestroyed ?? true);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.VaultAgavoides, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1060u, NameID = 13989u, Category = BossModuleInfo.Category.TreasureHunt, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 7)]
public sealed class VaultWindSpirit(WorldState ws, Actor primary) : SharedBoundsBoss(ws, primary)
{
    public static readonly uint[] Trash = [(uint)OID.VaultAgavoides, (uint)OID.VaultTree];

    public Actor? BossWindSpirit;

    protected override void UpdateModule()
    {
        BossWindSpirit ??= GetActor((uint)OID.VaultWindSpirit);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(BossWindSpirit);
        Arena.Actors(this, Trash);
    }
}

