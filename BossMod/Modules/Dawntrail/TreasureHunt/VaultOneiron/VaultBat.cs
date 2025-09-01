namespace BossMod.Dawntrail.TreasureHunt.VaultOneiron.VaultBat;

public enum OID : uint
{
    VaultGemkeeper = 0x489E, // R0.9
    VaultAcrobat = 0x489F, // R1.2
    VaultBat = 0x48A0 // R1.6
}

public enum AID : uint
{
    AutoAttack = 872, // VaultGemkeeper/VaultAcrobat/VaultBat->player, no cast, single-target

    FastBoulder = 43627, // VaultGemkeeper->location, 3.0s cast, range 3 circle
    Catchoo = 43629, // VaultAcrobat->self, 4.0s cast, range 40 width 8 rect
    ColdSpot = 43631, // VaultBat->location, 3.0s cast, range 6 circle
    PhantomLight = 43630, // VaultBat->self/player, 5.0s cast, range 60 width 8 rect, tankbuster
    ChillingDraft = 43632, // VaultBat->self, 4.0s cast, range 40 45-degree cone
    ParanormalActivity = 43633, // VaultBat->self, 5.0s cast, range 100 circle, raidwide
    Acrocatics = 43628 // VaultAcrobat->self, 3.0s cast, range 6 circle
}

sealed class PhantomLight(BossModule module) : Components.BaitAwayCast(module, (uint)AID.PhantomLight, new AOEShapeRect(60f, 4f), tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster, endsOnCastEvent: true);
sealed class FastBoulder(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FastBoulder, 3f);
sealed class ChillingDraft(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ChillingDraft, new AOEShapeCone(40f, 22.5f.Degrees()));
sealed class AcrocaticsColdSpot(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Acrocatics, (uint)AID.ColdSpot], 6f);
sealed class Catchoo(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Catchoo, new AOEShapeRect(40f, 4f));
sealed class ParanormalActivity(BossModule module) : Components.RaidwideCast(module, (uint)AID.ParanormalActivity);

sealed class VaultBatStates : StateMachineBuilder
{
    public VaultBatStates(VaultBat module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PhantomLight>()
            .ActivateOnEnter<FastBoulder>()
            .ActivateOnEnter<ChillingDraft>()
            .ActivateOnEnter<AcrocaticsColdSpot>()
            .ActivateOnEnter<Catchoo>()
            .ActivateOnEnter<ParanormalActivity>()
            .Raw.Update = () => AllDestroyed(VaultBat.Trash) && (module.BossBat?.IsDeadOrDestroyed ?? true);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.VaultGemkeeper, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1060u, NameID = 13992u, Category = BossModuleInfo.Category.TreasureHunt, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 9)]
public sealed class VaultBat(WorldState ws, Actor primary) : SharedBoundsBoss(ws, primary)
{
    public static readonly uint[] Trash = [(uint)OID.VaultGemkeeper, (uint)OID.VaultAcrobat];

    public Actor? BossBat;

    protected override void UpdateModule()
    {
        BossBat ??= GetActor((uint)OID.VaultBat);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(BossBat);
        Arena.Actors(this, Trash);
    }
}
