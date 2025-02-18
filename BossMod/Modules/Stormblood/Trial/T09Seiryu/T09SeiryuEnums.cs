﻿namespace BossMod.Stormblood.Trial.T09Seiryu;

public enum OID : uint
{
    Boss = 0x25F4, // R4.4
    Helper = 0x233C,
    AkaNoShiki = 0x2786, // R2.6
    AoNoShiki = 0x2787, // R3.0
    IwaNoShiki = 0x2788, // R4.0
    BlueOrochi = 0x2672, // R1.0
    TenNoShiki = 0x25F8, // R2.7
    NumaNoShiki = 0x25F6, // R2.4
    DoroNoShiki = 0x25F7, // R1.44
    BlueOrochi1 = 0x25F5, // R1.0
    BlueOrochi2 = 0x2658, // R1.0
    BlueOrochi3 = 0x2659, // R1.0
    YamaNoShiki = 0x25F9, // R12.0
    Towers = 0x1EAA3C // R0.5
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // NumaNoShiki/DoroNoShiki->player, no cast, single-target

    HundredTonzeSwing = 15390, // IwaNoShiki->self, 4.0s cast, range 16 circle

    BlueBoltMarker = 14319, // Helper->player, no cast, single-target, applies linestack marker
    BlueBolt = 15388, // AoNoShiki->self, no cast, range 80+R width 5 rect, line stack
    RedRush = 15389, // AkaNoShiki->self, no cast, range 80+R width 5 rect, knockback bait, 15 awayfromsource

    CoursingRiver1 = 14325, // BlueOrochi1->self, 5.0s cast, single-target
    CoursingRiverAOE = 14349, // Helper->self, 7.5s cast, range 21 circle, knockback dirforward 25
    CoursingRiver = 14350, // Helper->self, 7.5s cast, range 90+R width 90 rect, visual?, doesn't seem to do anything
    CoursingRiver4 = 14626, // BlueOrochi2->self, 5.0s cast, single-target
    CoursingRiver5 = 14627, // BlueOrochi3->self, 5.0s cast, single-target

    DragonsWake1 = 14282, // Boss->self, no cast, single-target
    DragonsWake2 = 14336, // Helper->self, 24.0s cast, range 80 circle

    Explosion1 = 14347, // NumaNoShiki->self, no cast, range 80 circle
    Explosion2 = 14348, // DoroNoShiki->self, no cast, range 80 circle

    FifthElement = 14334, // Boss->self, 4.0s cast, range 100 circle

    ForbiddenArtsMarker = 14277, // Helper->player, no cast, single-target, applies linestack marker
    ForbiddenArtsFirst = 15490, // Boss->self, no cast, range 80+R width 8 rect
    ForbiddenArtsSecond = 15474, // Boss->self, no cast, range 80+R width 8 rect

    FortuneBladeSigil = 14342, // Helper->self, 6.5s cast, range 50+R width 4 rect

    GreatTyphoon28 = 14352, // Helper->self, 3.0s cast, range ?-28 donut, outside of arena
    GreatTyphoon34 = 14353, // Helper->self, 3.0s cast, range ?-34 donut, outside of arena
    GreatTyphoon40 = 14354, // Helper->self, 3.0s cast, range ?-40 donut, outside of arena
    InfirmSoul = 14333, // Boss->player, 5.0s cast, range 4 circle, tankbuster

    KanaboVisual1 = 14316, // IwaNoShiki->location, no cast, ???
    KanaboVisual2 = 15392, // IwaNoShiki->self, 6.0s cast, single-target
    Kanabo = 15391, // IwaNoShiki->self, 3.0s cast, range 40+R 60-degree cone

    KujiKiri = 14305, // Boss->self, 4.0s cast, single-target

    OnmyoSigil1 = 14338, // Boss->self, no cast, single-target
    OnmyoSigil2 = 14855, // Helper->self, 3.0s cast, range 12 circle

    SerpentAscending = 14300, // Boss->self, no cast, single-target
    SerpentDescending = 14340, // Helper->player, 6.0s cast, range 5 circle

    SerpentAscendingTowers = 15397, // Boss->self, 4.0s cast, single-target
    SerpentsFang = 14988, // Helper->self, no cast, range 3 circle
    SerpentsJaws = 14989, // Helper->self, no cast, range 80 circle, tower fail

    SerpentEyeSigil1 = 14339, // Boss->self, no cast, single-target
    SerpentEyeSigil2 = 14856, // Helper->self, 3.0s cast, range 7-30 donut

    StrengthOfSpirit = 14281, // Boss->self, 5.0s cast, range 80 circle, knockback 24, awayfromsource, Transitions fight to Phase 2

    SummonShiki1 = 14286, // Boss->self, 3.0s cast, single-target
    SummonShiki2 = 14287, // Boss->self, 3.0s cast, single-target
    SummonShiki3 = 14288, // Boss->self, 5.0s cast, single-target

    Teleport1 = 14276, // Boss->location, no cast, ???
    Teleport2 = 14293, // Boss->location, no cast, ???

    YamaKagura = 14355, // TenNoShiki->self, 5.0s cast, range 60+R width 6 rect

    HandprintVisual1 = 14309, // YamaNoShiki->self, no cast, single-target
    HandprintVisual2 = 14310, // YamaNoShiki->self, no cast, single-target
    Handprint1 = 14343, // Helper->self, 4.5s cast, range 20 180-degree cone, handprint 3 and 4 happen at exactly the same time and spot
    Handprint2 = 14344, // Helper->self, 4.5s cast, range 40 180-degree cone

    ForceOfNature1 = 14346, // Helper->self, 5.0s cast, range 21 circle, knockback 10 AwayFromOrigin
    ForceOfNature2 = 14345, // Helper->self, 5.0s cast, range 5 circle
    ForceOfNature3 = 14313, // YamaNoShiki->self, no cast, single-target

    SummonShiki = 14285 // Boss->self, 3.0s cast, single-target
}

public enum IconID : uint
{
    Spreadmarker = 169 // player
}

public enum TetherID : uint
{
    BaitAway = 17 // AkaNoShiki/AoNoShiki/IwaNoShiki->player
}
