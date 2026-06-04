namespace BossMod.Dawntrail.Ultimate.DMU;

public enum OID : uint {
    _Gen_GravenImage = 0x4C31, // R0.500, x9
    Kefka = 0x4C30, // R6.000, x1
    Helper = 0x233C, // R0.500, x37, Helper type
    _Gen_Actor1e8536 = 0x1E8536, // R0.500-2.000, x1, EventObj type
    _Gen_Actor1ebfb4 = 0x1EBFB4, // R0.500, x1, EventObj type
    _Gen_Exit = 0x1E850B, // R0.500, x1, EventObj type
    StatueBodyOrb = 0x1EBFBB, // R0.500, x2, EventObj type
    _Gen_Actor1ebfbe = 0x1EBFBE, // R0.500, x2, EventObj type
    _Gen_Actor1ebfbd = 0x1EBFBD, // R0.500, x2, EventObj type
    _Gen_Actor1ebfbc = 0x1EBFBC, // R0.500, x2, EventObj type
    _Gen_Actor1ebfbf = 0x1EBFBF, // R0.500, x2, EventObj type
    _Gen_Actor1ec022 = 0x1EC022, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint {
    AutoAttack = 49746, // Kefka->player, no cast, single-target
    RevoltingRuinIII = 50179, // Kefka->self/players, 5.0s cast, range 100 ?-degree cone
    RevoltingRuinIII1 = 50401, // Kefka->self, no cast, range 100 ?-degree cone
    Ability = 50173, // Kefka->location, no cast, single-target

    GravenImage = 48370, // Kefka->self, 3.0s cast, single-target
    PulseWave = 47785, // 4C31->player, no cast, single-target
    BlizzardIIIBlowout = 47774, // Helper->self, 5.0s cast, range 40 ?-degree cone - Empty zone
    BlizzardIIIBlowout1 = 47771, // Helper->self, 5.0s cast, range 40 ?-degree cone - Questionmark zone
    BlizzardIIIBlowout2 = 47768, // Helper->self, 5.0s cast, range 40 ?-degree cone - Questionmark zone
    MysteryMagic = 47764, // Kefka->self, 5.0s cast, single-target
    FlagrantFireIIIStack = 47779, // Helper->players, no cast, range 6 circle - Stack
    FlagrantFireIIISpread = 47778, // Helper->players, no cast, range 5 circle

    WaveCannon = 47784, // 4C31->self, no cast, range 100 width 6 rect
    TowerExplosion = 47786, // Helper->self, 3.0s cast, range 4 circle
    UnmitigatedExplosion = 47787, // Helper->self, no cast, range 100 circle

    DoubleTroubleTrap = 47782, // Kefka->self, 3.0s cast, single-target
    DoubleTroubleTrap1 = 47783, // Helper->player, no cast, range 6 circle

    ThrummingThunderIII = 47775, // Helper->self, 5.0s cast, range 40 width 10 rect
    ThrummingThunderIII1 = 47776, // Helper->self, 5.0s cast, range 40 width 10 rect
    ThrummingThunderIII2 = 47777, // Helper->self, 5.0s cast, range 40 width 10 rect

    LightOfJudgment = 50722, // Kefka->self, 5.0s cast, range 100 circle
    Hyperdrive = 49739, // Kefka->player, no cast, range 5 circle

    _Ability_BlizzardIIIBlowout3 = 47765, // Kefka->self, 5.0s cast, single-target

    Gravitas = 47788, // 4C31->player, no cast, range 5 circle - Purple puddle - Stack
    Vitrophyre = 47792, // _Gen_GravenImage->players, no cast, range 5 circle - Rock - Spread
    _Ability_GravityIII = 47791, // Helper->self, no cast, range 5 circle



    _Ability_GravitationalExplosion = 47789, // Helper->self, no cast, range 100 circle
    _Ability_GravitationalWave = 47793, // 4C31->self, no cast, range 100 ?-degree cone


    _Ability_IntemperateWill = 47794, // _Gen_GravenImage->self, no cast, range 100 ?-degree cone

    _Ability_TeleTrouncing = 47801, // Kefka->self, 5.0s cast, single-target
    _Ability_TeleTrouncing1 = 47802, // Helper->players, no cast, range 2 circle
    _Weaponskill_ = 50516, // Kefka->self, 3.0s cast, single-target
    _Ability_IndulgentWill = 47797, // _Gen_GravenImage->player, no cast, single-target
    _Ability_IdyllicWill = 47798, // _Gen_GravenImage->players, no cast, range 5 circle
    _Weaponskill_1 = 50517, // Kefka->self, no cast, single-target
    _Ability_IndolentWill = 47796, // _Gen_GravenImage->self, no cast, range 100 circle
    _Ability_LightOfJudgment = 47803, // Kefka->self, 5.0s cast, range 100 circle
    _Ability_AveMaria = 47795, // _Gen_GravenImage->self, no cast, range 100 circle
}

public enum SID : uint {
    DoubleTroubleTrap = 5078, // none->player, extra=0x0

    _Gen_DamageDown = 2911, // Helper->player, extra=0x0
    _Gen_MagicVulnerabilityUp = 2941, // Helper/4C31->player, extra=0x0
}

public enum IconID : uint {
    spreadIcon = 127, // player->self // Spread
    stackIcon = 128, // player->self // Stack
    FireRingQuestionMark = 673, // Kefka->self // Questionmark - 2A1
    FireRingBlueOrb = 674, // Kefka->self // Blue orb - 2A2
    BlueRingQuestionMark = 675, // Kefka->self // Questionmark - 2A3
    BlueRingBlueOrb = 676, // Kefka->self // Blue orb - 2A4
    PurpleRingQuestionMark = 677, // Kefka->self // Questionmark
    PurpleRingBlueOrb = 678, // Kefka->self // Blue orb

    _Gen_Icon_tank_lockon02k1 = 218, // player->self
}

public enum TetherID : uint {
    GravenImageTether = 45, // 4C31->player
}
