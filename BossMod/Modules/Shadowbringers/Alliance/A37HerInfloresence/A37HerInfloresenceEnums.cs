namespace BossMod.Shadowbringers.Alliance.A37HerInfloresence;

public enum OID : uint
{
    Boss = 0x3190, // R5.999, x1
    Energy = 0x3192, // R1.000, x0 (spawn during fight)
    RedGirl = 0x3191, // R3.450, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    BossAutoAttack = 24575, // Boss->player, no cast, single-target

    // Building through floor mechanic
    Pervasion = 23520, // Boss->self, 3.0s cast, single-target
    RecreateStructure = 23521, // Boss->self, 3.0s cast, single-target
    UnevenFooting = 23522, // Helper->self, 1.9s cast, range 80 width 30 rect

    // Train mechanic
    RecreateSignal = 23523, // Boss->self, 3.0s cast, single-target
    MixedSignals = 23524, // Boss->self, 3.0s cast, single-target
    Crash = 23525, // Helper->self, 0.8s cast, range 50 width 10 rect

    // Player baited marching AOEs
    LighterNote1 = 23564, // Boss->self, 3.0s cast, single-target
    LighterNote2 = 23513, // Helper->location, no cast, range 6 circle
    LighterNote3 = 23514, // Helper->location, no cast, range 6 circle

    // Raidwide
    ScreamingScore = 23541, // Boss->self, 5.0s cast, range 71 circle

    // all three tanks get a pink AoE damage marker
    DarkerNote1 = 23516, // Helper->player, 5.0s cast, range 6 circle
    DarkerNote2 = 23562, // Boss->self, 5.0s cast, single-target

    // Boss forms arms into pillars and signals where they will fall, to her flanks or front and rear
    HeavyArms1 = 23535, // Helper->self, 7.0s cast, range 44 width 100 rect
    HeavyArms2 = 23534, // Boss->self, 7.0s cast, single-target
    HeavyArms3 = 23533, // Boss->self, 7.0s cast, range 100 width 12 rect

    // players get a circle surrounding them - half white/half black - likely going to need to be similar to gaze code
    // a large ring will start to drop in the center
    // you need to match your half of color to it to avoid damage
    Distortion1 = 23529, // Boss->self, 3.0s cast, range 60 circle
    Distortion2 = 24664, // Boss->self, 3.0s cast, range 60 circle
    TheFinalSong = 23530, // Boss->self, 3.0s cast, single-target
    WhiteDissonance = 23531, // Helper->self, no cast, range 60 circle
    BlackDissonance = 23532, // Helper->self, no cast, range 60 circle
    PlaceOfPower = 23565, // Helper->location, 3.0s cast, range 6 circle

    PillarImpact1 = 23566, // Boss->self, no cast, single-target
    PillarImpact2 = 23536, // Boss->self, 10.0s cast, single-target
    Shockwave1 = 23538, // Helper->self, 6.5s cast, range 71 circle knockback 35
    Shockwave2 = 23537, // Helper->self, 6.5s cast, range 7 circle
    Towerfall1 = 23539, // Boss->self, 3.0s cast, single-target
    Towerfall2 = 23540, // Helper->self, 3.0s cast, range 70 width 14 rect

    UnknownAbility2 = 23526, // RedGirl->self, no cast, single-target
    UnknownAbility3 = 23527, // RedGirl->self, no cast, single-target
    ScatteredMagic = 23528, // Energy->player, no cast, single-target

    // Beams firing across the arena - need to figure out indicators
    RhythmRings = 23563, // Boss->self, 3.0s cast, single-target
    MagicalInterference = 23509 // Helper->self, no cast, range 50 width 10 rect
}

public enum SID : uint
{
    UnknownStatus = 2056, // none->Boss, extra=0xE1
    Distorted = 2535 // Boss->player, extra=0x0
}

public enum IconID : uint
{
    Icon1 = 1, // player
    Icon139 = 139, // player
}

public enum TetherID : uint
{
    Tether54 = 54, // Helper/Boss->Boss/Helper
}
