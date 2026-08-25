namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME2SwordDancer;

public enum OID : uint
{
    SwordDancer = 0x4D7E,
    Helper = 0x233C,
    _Gen_ControlSigil = 0x1EBFDA, // R0.500, x1, EventObj type
    _Gen_ControlSigil1 = 0x1EBFD8, // R0.500, x1, EventObj type
    _Gen_Actor1e8fb8 = 0x1E8FB8, // R2.000, x2, EventObj type
    _Gen_Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    _Gen_DancingSword = 0x4D84, // R2.000, x16
    _Gen_DancingSword1 = 0x4D82, // R1.000, x5
    _Gen_DancingSword2 = 0x4D83, // R2.000, x2
    _Gen_DancingSword3 = 0x4D81, // R2.000, x3
    _Gen_DancingSword4 = 0x4D7F, // R2.000, x4
    _Gen_SwordDancer = 0x4D85, // R1.000, x1
    _Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
    _Gen_TeleportationSigil = 0x1EBFF0, // R0.500, x1, EventObj type
    _Gen_Actor1ec032 = 0x1EC032, // R0.500, x0 (spawn during fight), EventObj type
    _Gen_ = 0x4D86, // R1.000, x0 (spawn during fight)
    _Gen_Actor1ec033 = 0x1EC033, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    _AutoAttack_Attack = 50783, // SwordDancer->player, no cast, single-target
    _Weaponskill_SwordStorm = 49675, // SwordDancer->self, 5.0s cast, range 0 ???
    _Ability_SwordStorm = 49686, // Helper->self, no cast, range 0 ???
    _Ability_ = 49558, // SwordDancer->location, no cast, single-target
    _Ability_1 = 49618, // 4D85->self, no cast, range ?-30 donut
    _Weaponskill_ThrowingSwords = 49619, // SwordDancer->self, 2.0+1.0s cast, single-target
    _Ability_Rush = 49621, // 4D7F->location, 3.0s cast, width 7 rect charge
    _Ability_Rush1 = 50527, // 4D7F->location, 3.0s cast, width 7 rect charge
    _Weaponskill_ThrowingSwords1 = 49620, // SwordDancer->self, no cast, single-target
    _Ability_Rush2 = 50528, // 4D7F->location, 3.0s cast, width 7 rect charge
    _Ability_Turn = 49635, // Helper->self, 3.5s cast, range ?-14 donut
    _Ability_Turn1 = 49628, // 4D7F->location, 3.5s cast, ???
    _Ability_Turn2 = 49637, // Helper->self, 3.5s cast, range ?-24 donut
    _Ability_Turn3 = 49623, // 4D7F->location, 3.5s cast, ???
    _Ability_Rush3 = 49622, // 4D7F->location, 3.6s cast, width 7 rect charge
    _Weaponskill_ = 49676, // SwordDancer->location, no cast, single-target
    _Ability_Turn4 = 49634, // 4D7F->location, 3.5s cast, ???
    _Ability_Turnabout = 50064, // Helper->self, 3.5s cast, range ?-24 donut
    _Weaponskill_MartialMystique = 49644, // SwordDancer->self, 4.0+1.5s cast, single-target
    _Ability_Rush4 = 49674, // 4D84->self, 6.0s cast, range 30 width 6 rect
    _Ability_MartialMystique = 49645, // Helper->self, 5.5s cast, range 48 width 96 rect
    _Weaponskill_MartialMystique1 = 49642, // SwordDancer->self, 4.0+1.5s cast, single-target
    _Weaponskill_MartialMystique2 = 49641, // SwordDancer->self, 4.0+1.5s cast, single-target
    _Weaponskill_CycloswordsUnsheathed = 49646, // SwordDancer->self, 3.0s cast, single-target
    _Weaponskill_Cycloswords = 49647, // SwordDancer->self, 3.0+1.0s cast, single-target
    _Ability_Spin = 49649, // 4D81->self, 1.0s cast, range ?-60 donut
    _Ability_2 = 50435, // 4D81->self, no cast, single-target
    _Ability_Spin1 = 49652, // 4D81->self, 1.0s cast, range 15 circle
    _Weaponskill_SwordDance = 49667, // SwordDancer->self, 4.4+0.6s cast, single-target
    _Ability_SwordDance = 49668, // Helper->self, 5.0s cast, range 0 ???
    _Ability_SwordDance1 = 49669, // Helper->self, no cast, range 0 ???
    _Ability_SwordDance2 = 49670, // Helper->self, no cast, range 0 ???
    _Ability_SwordDance3 = 49671, // Helper->self, no cast, range 0 ???
    _Ability_SwordDance4 = 49672, // Helper->self, 1.5s cast, range 60 width 20 rect
    _Weaponskill_LeapingLift = 49654, // SwordDancer->self, 3.0s cast, single-target
    _Ability_Pierce = 49655, // 4D82->self, 3.6s cast, range 5 circle
    _Ability_LeapingLift = 49656, // SwordDancer->location, no cast, ???
    _Ability_LeapingLift1 = 49657, // SwordDancer->location, no cast, single-target
    _Ability_LeapingLift2 = 49659, // SwordDancer->location, no cast, ???
    _Weaponskill_Swordpointe = 49687, // SwordDancer->self, 2.0+1.0s cast, single-target
    _Ability_Steelsbreath = 50360, // Helper->self, 1.5s cast, range 0 ???
    _Ability_Steelsbreath1 = 49660, // 4D82->self, 1.5s cast, range 60 ???
    _Ability_Steelsforge = 49661, // Helper->self, 0.5s cast, range 13 circle
    _Ability_Spin2 = 49651, // 4D81->self, 1.0s cast, range 10 circle
    _Ability_Spin3 = 49653, // 4D81->self, 1.0s cast, range 20 circle
    _Ability_3 = 50431, // 4D81->self, no cast, single-target
    _Ability_4 = 50433, // 4D81->self, no cast, single-target
    _Ability_Spin4 = 49648, // 4D81->self, 1.0s cast, range ?-60 donut
    _Ability_Spin5 = 49650, // 4D81->self, 1.0s cast, range ?-60 donut
    _Ability_Turn5 = 49636, // Helper->self, 3.5s cast, range ?-19 donut
    _Ability_Turn6 = 49627, // 4D7F->location, 3.5s cast, ???
    _Ability_Turn7 = 49633, // 4D7F->location, 3.5s cast, ???
    _Ability_Turnabout1 = 50063, // Helper->self, 3.5s cast, range ?-19 donut
    _Ability_5 = 50432, // _Gen_DancingSword3->self, no cast, single-target
    _Ability_6 = 50436, // _Gen_DancingSword3->self, no cast, single-target
    _Ability_Turn8 = 49624, // _Gen_DancingSword4->location, 3.5s cast, ???
    _Ability_Turn9 = 49639, // Helper->self, 3.5s cast, range ?-19 donut
    _Ability_Turn10 = 49630, // _Gen_DancingSword4->location, 3.5s cast, ???
    _Weaponskill_MartialMystique3 = 49643, // SwordDancer->self, 4.0+1.5s cast, single-target
}

public enum SID : uint
{
    _Gen_QuickerStep = 4799, // none->player, extra=0x0
    _Gen_ThriceComeRuin = 3478, // 4D7F/4D84/4D81/Helper->player, extra=0x1/0x2
    _Gen_SeraphicVeil = 1917, // player->player, extra=0x0
    _Gen_EverlastingFlight = 1868, // player->player, extra=0x0
    _Gen_SeraphicIllumination = 1875, // player->player, extra=0x0
    _Gen_Unk = 3558, // none->4D81, extra=0x46E/0x46D/0x46F
    _Gen_Unknown = 2056, // none->SwordDancer/4D82, extra=0x47A/0x47B/0x495
    _Gen_Doom = 2519, // 4D7F->player, extra=0x0

}

public enum TetherID : uint
{
    _Gen_Tether_chn_sworddancer_l01t1 = 424, // 4D7F->SwordDancer
    _Gen_Tether_chn_sworddancer_r01t1 = 423, // 4D7F->SwordDancer
}
