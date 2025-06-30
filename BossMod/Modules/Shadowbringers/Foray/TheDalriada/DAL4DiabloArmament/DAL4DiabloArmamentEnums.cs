namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL4DiabloArmament;

public enum OID : uint
{
    Boss = 0x31B3, // R28.5

    DiabolicBit = 0x31B4, // R1.2
    Aether = 0x31B5, // R1.5
    DiabolicGate1 = 0x1EB1D8, // R0.5
    DiabolicGate2 = 0x1EB1D7, // R0.5
    DiabolicGate3 = 0x1EB1D6, // R0.5
    DiabolicGate4 = 0x1EB1D9, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    BrutalCamisado = 24899, // Boss->player, no cast, single-target
    DeathWall = 24994, // Helper->self, no cast, range 17-60 donut
    TeleportBit = 23725, // DiabolicBit->location, no cast, single-target

    AdvancedDeathIVVisual = 23727, // Boss->self, 4.0s cast, single-target
    AdvancedDeathIV = 23728, // Helper->location, 7.0s cast, range 10 circle

    AdvancedDeathRayVisual = 23748, // Boss->self, 5.0s cast, single-target, tankbuster
    AdvancedDeathRay = 23749, // Helper->player, no cast, range 70 width 8 rect

    AdvancedNox = 23743, // Boss->self, 4.0s cast, single-target
    AdvancedNoxAOEFirst = 23744, // Helper->self, 10.0s cast, range 10 circle
    AdvancedNoxAOERest = 23745, // Helper->self, no cast, range 10 circle

    AethericBoom1 = 23731, // Boss->self, 5.0s cast, single-target
    AethericBoom2 = 23732, // Helper->self, 5.0s cast, ???

    AethericExplosion1 = 23750, // Boss->self, 5.0s cast, single-target
    AethericExplosion2 = 23751, // Helper->self, 5.0s cast, ???

    Aetheroplasm = 23733, // Aether->self, no cast, range 6 circle

    MagitekBit = 23724, // Boss->self, 4.0s cast, single-target
    AssaultCannon = 23726, // DiabolicBit->self, 7.0s cast, range 100 width 6 rect

    DeadlyDealingAOE = 23746, // Boss->location, 7.0s cast, range 6 circle
    DeadlyDealing = 23747, // Helper->self, 7.5s cast, ???, knockback 30, away from source

    DiabolicGate1 = 23711, // Boss->self, 4.0s cast, single-target
    DiabolicGate2 = 25028, // Helper->self, 5.0s cast, ???, raidwide

    AetherochemicalLaserAOE1 = 23716, // Boss->self, no cast, range 60 width 22 rect
    AetherochemicalLaserAOE2 = 23717, // Boss->self, no cast, range 60 width 60 rect

    Explosion1 = 23718, // Helper->self, 10.0s cast, range 60 width 22 rect
    Explosion2 = 23719, // Helper->self, 10.0s cast, range 60 width 22 rect
    Explosion3 = 24721, // Helper->self, 10.0s cast, range 60 width 22 rect

    Explosion4 = 23720, // Helper->self, 8.0s cast, range 60 width 22 rect
    Explosion5 = 23721, // Helper->self, 8.0s cast, range 60 width 22 rect
    Explosion6 = 24722, // Helper->self, 8.0s cast, range 60 width 22 rect

    Explosion7 = 23722, // Helper->self, 6.0s cast, range 60 width 22 rect
    Explosion8 = 23723, // Helper->self, 6.0s cast, range 60 width 22 rect
    Explosion9 = 24723, // Helper->self, 6.0s cast, range 60 width 22 rect

    FusionBurst = 23734, // Aether->self, no cast, range 100 circle

    LightPseudopillar = 23729, // Boss->self, 3.0s cast, single-target
    LightPseudopillarAOE = 23730, // Helper->location, 4.0s cast, range 10 circle

    PillarOfShamashCone1 = 23737, // Helper->self, 8.0s cast, range 70 20-degree cone
    PillarOfShamashCone2 = 23738, // Helper->self, 9.5s cast, range 70 20-degree cone
    PillarOfShamashCone3 = 23739, // Helper->self, 11.0s cast, range 70 20-degree cone
    PillarOfShamashBait = 23740, // Helper->player, no cast, range 70 width 4 rect, bait away
    PillarOfShamashMarker = 23741, // Helper->player, no cast, single-target, line stack
    PillarOfShamashStack = 23742, // Helper->player, no cast, range 70 width 8 rect

    RuinousPseudomen1 = 23712, // Boss->self, 15.0s cast, single-target
    RuinousPseudomen2 = 23713, // Helper->self, 1.0s cast, single-target
    RuinousPseudomen3 = 23714, // Boss->self, no cast, single-target
    RuinousPseudomen4 = 24908, // Helper->self, 1.5s cast, range 100 width 24 rect
    RuinousPseudomen5 = 24911, // Helper->self, 4.5s cast, range 80 width 24 rect
    RuinousPseudomen6 = 24995, // Helper->self, 1.5s cast, range 80 width 24 rect

    UltimatePseudoterror = 23715, // Boss->self, 4.0s cast, range 15-70 donut

    VoidSystemsOverloadVisual1 = 23735, // Boss->self, 5.0s cast, single-target
    VoidSystemsOverloadVisual2 = 25364, // Boss->self, 5.0s cast, single-target
    VoidSystemsOverload = 23736 // Helper->self, 5.0s cast, ???, raidwide
}

public enum SID : uint
{
    AccelerationBomb = 2657 // none->player, extra=0x0
}

public enum IconID : uint
{
    AdvancedDeathRay = 230, // player
    PillarOfShamash = 23 // player
}
