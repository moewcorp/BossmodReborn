﻿namespace BossMod.Endwalker.Variant.V02MR.V021Yozakura;

public enum OID : uint
{
    Boss = 0x3EDB, // R3.45
    ShishuYamabiko = 0x3F00, // R0.8
    MirroredYozakura = 0x3EDC, // R3.45
    MudBubble = 0x3EDD, // R4.0
    Kuromaru = 0x3EDF, // R0.4
    Shiromaru = 0x3EE0, // R0.4
    Shibamaru = 0x3EDE, // R0.4
    AccursedSeedling = 0x3EE1, // R0.75
    AutumnalTempest = 0x3EE3, // R0.8
    Wind = 0x1EB88C, // R0.5
    Thunder = 0x1EB88E, // R0.5
    Water = 0x1EB88D, // R0.5
    Fire = 0x1EB88B, // R0.5
    Helper = 0x233C,
    Helper2 = 0x3F53
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Teleport = 34322, // Boss->location, no cast, single-target

    GloryNeverlasting = 33705, // Boss->player, 5.0s cast, single-target, tankbuster

    ArtOfTheFireblossom = 33640, // Boss->self, 3.0s cast, range 9 circle
    ArtOfTheWindblossom = 33641, // Boss->self, 5.0s cast, range 5-60 donut

    KugeRantsui = 33706, // Boss->self, 5.0s cast, range 60 circle, raidwide
    OkaRanman = 33646, // Boss->self, 5.0s cast, range 60 circle, raidwide

    SealOfRiotousBloom = 33652, // Boss->self, 5.0s cast, single-target
    SealOfTheWindblossomVisual = 33659, // Boss->location, no cast, single-target
    SealOfTheFireblossomVisual = 33658, // Boss->location, no cast, single-target
    SealOfTheRainblossomVisual = 33661, // Boss->location, no cast, single-target
    SealOfTheLevinblossomVisual = 33660, // Boss->location, no cast, single-target
    SealOfTheWindblossom = 33654, // Helper->self, 2.0s cast, range 5-60 donut
    SealOfTheFireblossom = 33653, // Helper->self, 2.0s cast, range 9 circle
    SealOfTheRainblossom = 33655, // Helper->self, 2.0s cast, range 70 45-degree cone, four cone AoEs from Yozakura's hitbox in the intercardinal directions
    SealOfTheLevinblossom = 33656, // Helper->self, 1.8s cast, range 70 45-degree cone, four cone AoEs from Yozakura's hitbox in the cardinal directions

    SealOfTheFleeting = 33657, // Boss->self, 3.0s cast, single-target, Yozakura tethers to the petal piles

    SeasonsOfTheFleetingVisual1 = 33665, // Boss->self, 10.0s cast, single-target, telegraph four sequential AoEs
    SeasonsOfTheFleetingVisual2 = 33666, // Boss->self, no cast, single-target
    FireAndWaterTelegraph = 33667, // Helper->self, 2.0s cast, range 46 width 5 rect
    EarthAndLightningTelegraph = 33668, // Helper->self, 2.0s cast, range 60 45-degree cone

    SeasonOfFire = 33669, // Helper->self, 0.8s cast, range 46 width 5 rect
    SeasonOfWater = 33670, // Helper->self, 0.8s cast, range 46 width 5 rect
    SeasonOfLightning = 33671, // Helper->self, 0.8s cast, range 70 45-degree cone
    SeasonOfEarth = 33672, // Helper->self, 0.8s cast, range 70 45-degree cone

    //Left Windy
    WindblossomWhirlVisual = 33679, // Boss->self, 3.0s cast, single-target
    WindblossomWhirl1 = 33680, // Helper->self, 5.0s cast, range 5-60 donut
    WindblossomWhirl2 = 34544, // Helper->self, 3.0s cast, range 5-60 donut
    LevinblossomStrikeVisual = 33681, // Boss->self, 2.3s cast, single-target
    LevinblossomStrike = 33682, // Helper->location, 3.0s cast, range 3 circle
    DriftingPetals = 33683, // Boss->self, 5.0s cast, range 60 circle, knockback

    //Left Rainy
    Bunshin = 33662, // Boss->self, 5.0s cast, single-target
    Shadowflight = 33663, // Boss->self, 3.0s cast, single-target
    ShadowflightAOE = 33664, // MirroredYozakura->self, 2.5s cast, range 10 width 6 rect
    MudrainVisual = 33673, // Boss->self, 3.0s cast, single-target
    Mudrain = 33674, // Helper->location, 3.8s cast, range 5 circle
    IcebloomVisual = 33675, // Boss->self, 3.0s cast, single-target
    Icebloom = 33676, // Helper->location, 3.0s cast, range 6 circle
    MudPie = 33677, // Boss->self, 3.0s cast, single-target
    MudPieAOE = 33678, // MudBubble->self, 4.0s cast, range 60 width 6 rect

    //Middle Rope Pulled
    ArtOfTheFluff1 = 33693, // Shibamaru/Kuromaru->self, 6.5s cast, range 60 circle, gaze
    ArtOfTheFluff2 = 33694, // Shiromaru->self, 6.5s cast, range 60 circle, gaze

    FireblossomFlareVisual = 33695, // Boss->self, 3.0s cast, single-target
    FireblossomFlare = 33696, // Helper->location, 3.0s cast, range 6 circle

    DondenGaeshi = 33692, // Boss->self, 3.0s cast, single-target, indicates platforms
    SilentWhistle = 33691, // Boss->self, 3.0s cast, single-target, dog summons

    //Middle Rope Unpulled
    LevinblossomLance1 = 33687, // Boss->self, 5.0s cast, single-target
    LevinblossomLance2 = 33688, // Boss->self, 5.0s cast, single-target
    LevinblossomLanceFirst = 33689, // Helper->self, 5.8s cast, range 60 width 7 rect
    LevinblossomLanceRest = 33690, // Helper->self, no cast, range 60 width 7 rect

    TatamiTrap = 33684, // Boss->self, 3.0s cast, single-target
    TatamiGaeshi = 33685, // Boss->self, 3.0s cast, single-target
    TatamiGaeshiAOE = 33686, // Helper->self, 3.8s cast, range 40 width 10 rect

    //Right No Dogu
    RockMebuki = 33697, // Boss->self, 3.0s cast, single-target
    RockRootArrangementVisual = 33700, // Boss->self, 5.0s cast, single-target
    RockRootArrangementFirst = 33701, // Helper->location, 3.0s cast, range 4 circle
    RockRootArrangementRest = 33702, // Helper->location, no cast, range 4 circle

    //Right Dogu
    Witherwind = 33703 // Boss->self, 3.0s cast, single-target
}

public enum SID : uint
{
    SeasonsOfTheFleeting = 3623, // none->Boss, extra=0x0
    Unknown = 2056, // none->3EE1, extra=0x243
}

public enum IconID : uint
{
    Tankbuster = 218, // player
    Icon374 = 374, // Shibamaru/Shiromaru/Kuromaru
    RotateCW = 167, // Boss
    RotateCCW = 168, // Boss
    RootArrangement = 197, // player
}

public enum TetherID : uint
{
    Thunder = 6, // Helper2->Boss
    Wind = 4, // Helper2->Boss
    Water = 3, // Helper2->Boss
    Fire = 5, // Helper2->Boss
    Tether79 = 79, // Helper2->Boss
}
