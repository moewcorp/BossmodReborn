namespace BossMod.Endwalker.VariantCriterion.V02MR.V024Shishio;

public enum OID : uint
{
    Boss = 0x3F40, // R3.45
    Iwakura = 0x1EB8BD, // R0.5
    Raiun = 0x3F41, // R0.8
    Rairin = 0x3F42, // R1.0
    VenomousThrallSnakes = 0x3F45, // R1.44
    FeralThrallTigers = 0x3F44, // R3.25
    CleverThrallBaboons = 0x3F43, // R1.8
    DevilishThrall = 0x3F46, // R2.0
    HauntingThrall = 0x3F47, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 34526, // Boss->player, no cast, single-target
    Teleport = 33783, // Boss->location, no cast, single-target

    StormcloudSummons = 33751, // Boss->self, 3.0s cast, single-target

    SmokeaterFirst = 33752, // Boss->self, 2.5s cast, single-target
    SmokeaterRest = 33753, // Boss->self, no cast, single-target
    SmokeaterAbsorb = 33754, // Raiun->Boss, no cast, single-target

    LeapingLevin1 = 33758, // Raiun->self, 1.0s cast, range 8 circle
    LeapingLevin2 = 33759, // Raiun->self, 1.5s cast, range 12 circle
    LeapingLevin3 = 33760, // Raiun->self, 2.5s cast, range 23 circle

    NoblePursuit = 33766, // Boss->location, 8.0s cast, width 12 rect charge
    Levinburst = 33767, // Rairin->self, no cast, range 10 width 40 rect

    Enkyo = 33781, // Boss->self, 5.0s cast, range 60 circle, raidwide
    SplittingCry = 33782, // Boss->self/player, 5.0s cast, range 60 width 14 rect

    OnceOnRokujoVisual = 33755, // Boss->self, 7.5s cast, single-target
    OnceOnRokujo = 33757, // Helper->self, 8.0s cast, range 60 width 14 rect

    TwiceOnRokujoVisual1 = 34723, // Boss->self, 7.5s cast, single-target
    TwiceOnRokujoVisual2 = 34724, // Boss->self, no cast, single-target
    TwiceOnRokujo = 34725, // Helper->self, 8.0s cast, range 60 width 14 rect

    ThriceOnRokujoVisual1 = 34726, // Boss->self, 7.5s cast, single-target
    ThriceOnRokujoVisual2 = 34731, // Boss->self, no cast, single-target
    ThriceOnRokujo = 34732, // Helper->self, 8.0s cast, range 60 width 14 rect

    ThunderOnefoldVisual = 33761, // Boss->self, 3.0s cast, single-target
    ThunderOnefold = 33762, // Helper->location, 4.0s cast, range 6 circle

    ThunderTwofoldVisual = 34733, // Boss->self, 3.0s cast, single-target
    ThunderTwofold = 34809, // Helper->location, 4.0s cast, range 6 circle

    ThunderThreefoldVisual = 34810, // Boss->self, 3.0s cast, single-target
    ThunderThreefold = 34811, // Helper->location, 4.0s cast, range 6 circle

    CloudToCloud1 = 33763, // Raiun->self, 2.5s cast, range 100 width 2 rect
    CloudToCloud2 = 33764, // Raiun->self, 3.0s cast, range 100 width 6 rect
    CloudToCloud3 = 33765, // Raiun->self, 4.0s cast, range 100 width 12 rect

    //Route 8
    HauntingCry = 33768, // Boss->self, 3.0s cast, single-target, summons adds
    ThunderVortex = 33780, // Boss->self, 5.0s cast, range 6-30 donut
    UnsagelySpin = 33773, // VenomousThrallSnakes->self, 4.0s cast, range 6 circle
    Rush = 33774, // FeralThrallTigers->location, 6.0s cast, width 8 rect charge
    Vasoconstrictor = 33775, // VenomousThrall->location, 3.0s cast, range 5 circle

    //Route 9
    FocusedTremor = 33769, // Helper->self, 5.0s cast, range 60 circle
    YokiVisual = 33770, // Boss->self, 3.0s cast, single-target
    Yoki = 33771, // Helper->self, 4.0s cast, range 6 circle
    YokiUzu = 33772, // Boss->self, 8.0s cast, range 23 circle

    //Route 10
    RightSwipe = 33776, // DevilishThrall->self, 8.0s cast, range 40 180-degree cone
    LeftSwipe = 33777, // DevilishThrall->self, 8.0s cast, range 40 180-degree cone

    //Route 11
    ReishoVisual = 34603, // HauntingThrall->self, 4.0s cast, single-target, each ghost tethers a player, if there are not enough players, ghost moves seemingly randomly
    ReishoFirst = 34604, // Helper->self, 4.0s cast, range 6 circle
    ReishoRest = 33778 // Helper->self, no cast, range 6 circle
}

public enum SID : uint
{
    SixFulmsUnder = 567, // none->player, extra=0xC27/0xC28/0xC29/0xC2A/0xC2B/0xC2C/0xC2D/0xC2E/0xC2F/0xC30/0xC31/0xC32
}

public enum IconID : uint
{
    Tankbuster = 471, // player
}
