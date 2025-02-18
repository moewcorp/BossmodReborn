namespace BossMod.Shadowbringers.Alliance.A34RedGirlP1;

public enum OID : uint
{
    Boss = 0x32BB, // R7.500, x1
    WhiteLance = 0x32E3, // R1.000, x0 (spawn during fight)
    BlackLance = 0x32E4, // R1.000, x0 (spawn during fight)
    RedGirl1 = 0x32BC, // R2.250, x0 (spawn during fight)
    WallbossRedGirl = 0x32BD, // R12.250, x0 (spawn during fight)
    RedGirl3 = 0x32BE, // R12.250, x3
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 24597, // Helper->player, no cast, single-target
    Teleport = 24605, // Helper->location, no cast, single-target

    Cruelty1 = 24594, // Boss->self, 5.0s cast, single-target
    Cruelty2 = 24596, // Helper->location, no cast, range 75 circle

    Shockwave = 24590, // Boss->self, 2.0s cast, single-target
    ShockWhite1 = 24591, // Helper->players, no cast, range 5 circle
    ShockWhite2 = 24592, // Helper->location, 4.0s cast, range 5 circle
    ShockBlack1 = 24593, // Helper->players, no cast, range 5 circle
    ShockBlack2 = 24972, // Helper->location, 4.0s cast, range 5 circle

    GenerateBarrier1 = 24580, // Boss->self, 4.0s cast, single-target
    GenerateBarrier2 = 24584, // Helper->self, 4.0s cast, range 18 width 3 rect
    GenerateBarrier3 = 24585, // Helper->self, 4.0s cast, range 24 width 3 rect
    GenerateBarrier4 = 25362, // Helper->self, no cast, range 18 width 3 rect
    GenerateBarrier5 = 25363, // Helper->self, no cast, range 24 width 3 rect

    PointWhite1 = 24607, // WhiteLance->self, no cast, range 50 width 6 rect
    PointWhite2 = 24609, // WhiteLance->self, no cast, range 24 width 6 rect
    PointBlack1 = 24608, // BlackLance->self, no cast, range 50 width 6 rect
    PointBlack2 = 24610, // BlackLance->self, no cast, range 24 width 6 rect

    ManipulateEnergy1 = 24600, // Boss->self, 4.0s cast, single-target
    ManipulateEnergy2 = 24602, // Helper->players, no cast, range 3 circle

    DiffuseEnergy1 = 24611, // RedGirl1->self, 5.0s cast, range 12 120-degree cone
    DiffuseEnergy2 = 24662, // RedGirl1->self, no cast, range 12 ?-degree cone

    SublimeTranscendence1 = 25098, // Boss->self, 5.0s cast, single-target
    SublimeTranscendence2 = 25099, // Helper->location, no cast, range 75 circle

    Vortex = 24599, // Helper->location, no cast, ???
    RecreateMeteor = 24903, // Boss->self, 2.0s cast, single-target
    WipeWhite = 24588, // Helper->self, 13.0s cast, range 75 circle
    Replicate = 24586, // Boss->self, 3.0s cast, single-target
}

public enum SID : uint
{
    ProgramFFFFFFF = 2632, // none->player, extra=0x1AB
    Program000000 = 2633, // none->player, extra=0x1AC
}

public enum IconID : uint
{
    Icon262 = 262, // player
    Icon263 = 263, // player
    Icon264 = 264, // player
    Tankbuster = 218, // player
    Icon167 = 167, // RedGirl1
    Icon168 = 168, // RedGirl1
}
