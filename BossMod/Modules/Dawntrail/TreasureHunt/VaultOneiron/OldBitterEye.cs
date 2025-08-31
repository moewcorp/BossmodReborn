namespace BossMod.Dawntrail.TreasureHunt.VaultOneiron.OldBitterEye;

public enum OID : uint
{
    OldBitterEye = 0x48AA, // R4.4
    VaultEyeclops = 0x48AB, // R2.4

    VaultOnion = 0x48B9, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    VaultEggplant = 0x48BA, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    VaultGarlic = 0x48BB, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    VaultTomato = 0x48BC, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    VaultQueen = 0x48BD, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // OldBitterEye/VaultEyeclops->player, no cast, single-target

    TenStoneSwing = 43675, // OldBitterEye->self, 4.0s cast, range 11 circle
    HundredStoneSwipe = 43678, // OldBitterEye->self, 4.0s cast, range 40 60-degree cone
    LumberingLeap = 43683, // OldBitterEye->location, 4.0s cast, range 10 circle
    EyeOfTheThunderstorm = 43680, // OldBitterEye->self, 4.0s cast, range 5-40 donut
    HundredStoneSmash = 43682, // OldBitterEye->self/player, 5.0s cast, range 65 width 8 rect, tankbuster
    HyperchargedGlower = 43681, // OldBitterEye->self, 3.0s cast, range 20 width 7 rect
    ThousandStoneSwing = 43677, // OldBitterEye->self, 8.0s cast, range 30 circle, interruptible raidwide
    Glower = 43685, // VaultEyeclops->self, 3.0s cast, range 19 width 7 rect
    PredatorialInstinct = 43684, // OldBitterEye->self, 4.0s cast, range 45 circle, pull 50, between hitboxes
    HundredStoneSwing = 43676, // OldBitterEye->self, 1.5s cast, range 13 circle

    TearyTwirl = 32301, // VaultOnion->self, 3.5s cast, range 7 circle
    PluckAndPrune = 32302, // VaultEggplant->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // VaultGarlic->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // VaultTomato->self, 3.5s cast, range 7 circle
    Pollen = 32305, // VaultQueen->self, 3.5s cast, range 7 circle
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus adds disappear
}

sealed class TenStoneSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TenStoneSwing, 11f);
sealed class LumberingLeap(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LumberingLeap, 10f);
sealed class HundredStoneSmash(BossModule module) : Components.BaitAwayCast(module, (uint)AID.HundredStoneSmash, new AOEShapeRect(65f, 4f), tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class HundredStoneSwipe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HundredStoneSwipe, new AOEShapeCone(40f, 30f.Degrees()));
sealed class HyperchargedGlower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HyperchargedGlower, new AOEShapeRect(20f, 3.5f));
sealed class Glower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Glower, new AOEShapeRect(19f, 3.5f));
sealed class ThousandStoneSwingHint(BossModule module) : Components.CastInterruptHint(module, (uint)AID.ThousandStoneSwing, showNameInHint: true);
sealed class ThousandStoneSwing(BossModule module) : Components.RaidwideCast(module, (uint)AID.ThousandStoneSwing);

sealed class PredatorialInstinct(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.PredatorialInstinct, 50f, kind: Kind.TowardsOrigin, minDistance: 4.9f);

sealed class HundredStoneSwingEyeOfTheThunderstorm(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeCircle circle = new(13f);
    private static readonly AOEShapeDonut donut = new(5f, 40f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.PredatorialInstinct)
        {
            _aoe = [new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 4.6d))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.HundredStoneSwing:
            case (uint)AID.EyeOfTheThunderstorm:
                _aoe = [];
                break;
            case (uint)AID.LumberingLeap:
                if ((++NumCasts & 1) == 0)
                {
                    return; // boss only does donut on uneven number of leaps
                }
                _aoe = [new(donut, spell.LocXZ, default, Module.CastFinishAt(spell, 7.1d))];
                break;
        }
    }
}

sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);

sealed class OldBitterEyeStates : StateMachineBuilder
{
    public OldBitterEyeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TenStoneSwing>()
            .ActivateOnEnter<LumberingLeap>()
            .ActivateOnEnter<HundredStoneSwipe>()
            .ActivateOnEnter<HundredStoneSmash>()
            .ActivateOnEnter<HyperchargedGlower>()
            .ActivateOnEnter<Glower>()
            .ActivateOnEnter<ThousandStoneSwingHint>()
            .ActivateOnEnter<ThousandStoneSwing>()
            .ActivateOnEnter<PredatorialInstinct>()
            .ActivateOnEnter<HundredStoneSwingEyeOfTheThunderstorm>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => AllDeadOrDestroyed(OldBitterEye.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.OldBitterEye, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1060u, NameID = 14001u, Category = BossModuleInfo.Category.TreasureHunt, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 6)]
public sealed class OldBitterEye : SharedBoundsBoss
{
    public OldBitterEye(WorldState ws, Actor primary) : base(ws, primary)
    {
        eyeclops = Enemies((uint)OID.VaultEyeclops);
    }

    private static readonly uint[] bonusAdds = [(uint)OID.VaultOnion, (uint)OID.VaultTomato, (uint)OID.VaultOnion, (uint)OID.VaultEggplant, (uint)OID.VaultQueen];
    public static readonly uint[] All = [(uint)OID.OldBitterEye, (uint)OID.VaultEyeclops, .. bonusAdds];
    private readonly List<Actor> eyeclops;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(eyeclops);
        Arena.Actors(this, bonusAdds, Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.VaultOnion => 6,
                (uint)OID.VaultEggplant => 5,
                (uint)OID.VaultGarlic => 4,
                (uint)OID.VaultTomato => 3,
                (uint)OID.VaultQueen => 2,
                (uint)OID.VaultEyeclops => 1,
                _ => 0
            };
        }
    }
}
