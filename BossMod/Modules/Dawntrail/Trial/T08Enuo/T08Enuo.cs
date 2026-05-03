namespace BossMod.Dawntrail.Trial.T08Enuo;


public enum OID : uint
{
    Enuo = 0x4DB9,
    YawningVoid = 0x4DBA,
    Helper = 0x233C,
    _Gen_Exit = 0x1E850B, // R0.500, x1, EventObj type
    _Gen_ = 0x4DB8, // R5.000, x2
    _Gen_Actor1e8536 = 0x1E8536, // R2.000, x1, EventObj type
    _Gen_Void = 0x4EB4, // R0.850, x0 (spawn during fight), Helper type
    _Gen_Void1 = 0x4DBB, // R0.850, x0 (spawn during fight), Helper type
    _Gen_UncastShadow = 0x4DBD, // R5.000, x0 (spawn during fight)
    _Gen_LoomingShadow = 0x4DBC, // R12.500, x0 (spawn during fight)
}

public enum AID: uint
{
    _AutoAttack_ = 49936, // Enuo->player, no cast, single-target
    Meteorain = 49971, // Enuo->self, no cast, range 40 circle
    _Ability_ = 49927, // Enuo->location, no cast, single-target
    NaughtGrowsCastVisual = 49931, // Enuo->self, no cast, single-target
    NaughtGrowsAOE = 49933, // 4DBA (Yawning Void)->self, no cast, range 40 circle
    NaughtWakes = 49929, // Enuo->self, no cast, single-target melee hit
    _Weaponskill_ = 49930, // YawningVoid->location, no cast, single-target
    GazeOfTheVoidAnimation = 49950, // Enuo->self, 7.0+1.0s cast, single-target
    GazeOfTheVoid1 = 49952, // Helper->self, 8.0s cast, single-target
    GazeOfTheVoidCones = 49953, // Helper->self, 8.0s cast, range 40 ?-degree cone
    DeepFreezeCast = 49965, // Enuo->self, 5.0+1.0s cast, single-target
    DeepFreeze = 49966, // Helper->players, 6.0s cast, range 40 circle
    NaughtHunts = 49939, // Enuo->self, 6.0+1.0s cast, single-target
    EndlessChaseFirst = 48474, // _Gen_Void->self, 6.0s cast, range 6 circle : chasing aoe
    EndlessChaseRest = 49940, // _Gen_Void->location, no cast, range 6 circle
    _Weaponskill_NaughtHuntsAnother = 49941, // Enuo->self, 5.0+1.0s cast, single-target
    _Spell_ShroudedHoly = 49967, // Enuo->self, 4.0+1.0s cast, single-target
    ShroudedHolyStack = 49968, // Helper->players, 5.0s cast, range 6 circle
    _Spell_Meltdown = 49962, // Enuo->self, 4.0+1.0s cast, single-target
    _Spell_Meltdown1 = 49963, // Helper->location, 5.0s cast, range 5 circle
    MeltdownSpread = 49964, // Helper->players, 5.0s cast, range 5 circle
    _Weaponskill_Vacuum = 49942, // Enuo->self, 2.0+1.0s cast, single-target
    _Weaponskill_SilentTorrent1 = 49944, // _Gen_Void1->location, 3.5s cast, single-target
    _Weaponskill_SilentTorrent2 = 49943, // _Gen_Void1->location, 3.5s cast, single-target
    _Weaponskill_SilentTorrent = 49945, // _Gen_Void1->location, 3.5s cast, single-target
    _Weaponskill_SilentTorrent5 = 49947, // Helper->self, 4.0s cast, range ?-19 donut
    _Weaponskill_SilentTorrent4 = 49948, // Helper->self, 4.0s cast, range ?-19 donut
    _Weaponskill_SilentTorrent3 = 49946, // Helper->self, 4.0s cast, range ?-19 donut
    _Weaponskill_Vacuum1 = 49949, // _Gen_Void1->self, 1.5s cast, range 7 circle
    _Weaponskill_AllForNaught = 49954, // Enuo->self, 5.0s cast, single-target
    _Weaponskill_LoomingEmptiness = 49955, // _Gen_LoomingShadow->self, 6.0s cast, single-target
    _Weaponskill_LoomingEmptiness1 = 49981, // Helper->self, 7.0s cast, range 100 circle
    _Weaponskill_EmptyShadow = 49956, // _Gen_UncastShadow->self, 7.0s cast, single-target
    _AutoAttack_1 = 49957, // _Gen_LoomingShadow->player, no cast, single-target
    _Weaponskill_EmptyShadow1 = 50667, // Helper->self, 8.0s cast, range 10 circle
    _Weaponskill_Nothingness = 49958, // _Gen_UncastShadow->self, 3.0s cast, range 100 width 4 rect
    _Weaponskill_1 = 49938, // _Gen_UncastShadow/_Gen_LoomingShadow->self, no cast, single-target
    _Weaponskill_LightlessWorld = 49959, // Enuo->self, 5.0s cast, single-target
    _Weaponskill_LightlessWorld1 = 49960, // Helper->self, no cast, range 40 circle
    _Weaponskill_LightlessWorld2 = 49961, // Helper->self, no cast, range 40 circle
    _AutoAttack_2 = 50775, // 4DC0->Enuo, no cast, single-target
    _AutoAttack_Attack = 870, // 4DBF->Enuo, no cast, single-target
    _Ability_SpellFlame = 50796, // 4DC0->Enuo, no cast, range 25 width 4 rect
    _Spell_ShineBraver = 50505, // 4DBF->Enuo, no cast, range 5 circle
    _Weaponskill_WarlocksTide = 50776, // 4DC0->Enuo, no cast, single-target
    _Weaponskill_LightSlash = 50777, // 4DBF->Enuo, no cast, single-target
    _Ability_PaladinForce = 50798, // 4DBF->Enuo, no cast, range 5 circle
    _Weaponskill_NaughtGrows = 49932, // Enuo->self, 6.0+1.0s cast, single-target
    _Weaponskill_NaughtGrows1 = 49934, // Helper->self, 7.0s cast, range 12 circle
    _Ability_ShieldSlam = 50797, // 4DBF->Enuo, no cast, single-target
    _Weaponskill_SacredSword = 50778, // 4DBF->Enuo, no cast, single-target
    _Weaponskill_AuthorityOfTheAnointed = 50779, // 4DBF->Enuo, no cast, single-target
    _Weaponskill_Almagest = 49928, // Enuo->self, 5.0s cast, range 40 circle
    _Weaponskill_DimensionZero = 49969, // Enuo->self, 5.0s cast, single-target
    _Weaponskill_DimensionZero1 = 49970, // Enuo->self, no cast, range 60 width 8 rect
    _Weaponskill_GazeOfTheVoid2 = 49951, // Helper->self, 8.0s cast, single-target
}

public enum SID : uint
{
    _Gen_VulnerabilityUp = 1789, // YawningVoid/_Gen_Void1/_Gen_UncastShadow/Helper->player, extra=0x1/0x2
    _Gen_DirectionalDisregard = 3808, // none->Enuo, extra=0x0
    _Gen_2056 = 2056, // none->_Gen_UncastShadow/_Gen_LoomingShadow, extra=0x46B
    _Gen_Unbecoming = 4882, // none->player, extra=0x0
    _Gen_LightVision = 5343, // none->player, extra=0x28
    _Gen_InEvent = 1268, // none->player, extra=0x0
    _Gen_Preoccupied = 1619, // none->player, extra=0x0
    _Gen_2552 = 2552, // none->Enuo/4DBF, extra=0x487/0x488/0x489
    _Gen_VulnerabilityDown = 5467, // none->player/4DC0/4DBF, extra=0x0
    _Gen_2160 = 2160, // none->4DC0/4DBF, extra=0xC98/0x3152

}


public enum IconID : uint
{
    _Gen_Icon_m0742trg_b1t1 = 327, // player->self triangle bait away candidate deep freeze
    EndlessChaseIcon = 172, // player->self : endless chase
    ShroudedHolyStackIcon = 161, // player->self
    MeltdownSpreadIcon = 558, // player->self
    _Gen_Icon_share_laser_5s_small_c0a1 = 719, // Enuo->player
}




public enum TetherID : uint
{
    _Gen_Tether_chn_z5fd07_0a1 = 391, // _Gen_->Enuo
    _Gen_Tether_chn_z5fd14_0a1 = 404, // _Gen_Void->player
    _Gen_Tether_chn_z5fd15_0a1 = 405, // player->player
    _Gen_Tether_chn_tergetfix1f = 284, // _Gen_LoomingShadow->player
    _Gen_Tether_chn_z5fd18_0a1 = 425, // 4DC0->4DBF
    _Gen_Tether_chn_z5fd08_0a1 = 392, // _Gen_->Enuo
}


class Meteorain(BossModule module) : Components.RaidwideCast(module, (uint)AID.Meteorain);

sealed class NaughtGrowsAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NaughtGrowsAOE, new AOEShapeCircle(40f));

//Set to show 6 cones at a time.  Seems like the AOE might fade a little to soon?
sealed class GazeOfTheVoidCones(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.GazeOfTheVoidCones, (uint)AID.GazeOfTheVoid1], new AOEShapeCone(20f, 22.5f.Degrees()), 6, 11);

sealed class DeepFreeze(BossModule module) : Components.BaitAwayCast(module, (uint)AID.DeepFreeze,new AOEShapeCircle(6), true);

//Naughthunts - chasing AOE with red 3 prong icon/
// Develops a new tether with directional arrows and latches onto a new target midway.
//TODO needs logic for switching tethers and clearing chasing aoes from radar if it finishes early.Radius of circle should be larger than 4. try 6.
class EndlessChase(BossModule module) : Components.StandardChasingAOEs(module, 6f, (uint)AID.EndlessChaseFirst,
    (uint)AID.EndlessChaseRest, 6f, 1.5d, 22, true, (uint)IconID.EndlessChaseIcon);

//49941 Naught Haunts Another

//enshrouded holy - stack marker
sealed class ShroudedHoly(BossModule module) : Components.StackWithIcon(module, (uint)IconID.ShroudedHolyStackIcon, (uint)AID.ShroudedHolyStack, 6, 0);


// meltdown spread marker.  TODO Should show initial aoe and then aoe after moving
sealed class MeltdownSpread(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.MeltdownSpreadIcon, (uint)AID.MeltdownSpread, 4, 5);



[SkipLocalsInit]
sealed class EnuoStates : StateMachineBuilder
{
    public EnuoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Meteorain>()
            .ActivateOnEnter<NaughtGrowsAOE>()
            .ActivateOnEnter<GazeOfTheVoidCones>()
            .ActivateOnEnter<DeepFreeze>()
            .ActivateOnEnter<EndlessChase>()
            .ActivateOnEnter<ShroudedHoly>()


            .ActivateOnEnter<MeltdownSpread>()

            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(EnuoStates),
    ConfigType = null, // replace null with typeof(EnuoConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = typeof(TetherID), // replace null with typeof(TetherID) if applicable
    IconIDType = typeof(IconID), // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Enuo,
    Contributors = "Wen",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Trial,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1115u,
    NameID = 14749u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class Enuo(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f));




