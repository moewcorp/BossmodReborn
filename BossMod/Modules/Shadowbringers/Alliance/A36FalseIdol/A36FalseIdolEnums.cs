namespace BossMod.Shadowbringers.Alliance.A36FalseIdol;

public enum OID : uint
{
    Boss = 0x318D, // R26.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 24572, // Boss->player, no cast, single-target

    Eminence = 24021, // Boss->location, 5.0s cast, range 60 circle
    RhythmRings = 23508, // Boss->self, 3.0s cast, single-target
    MagicalInterference = 23509, // Helper->self, no cast, range 50 width 10 rect
    MadeMagic1 = 23510, // Boss->self, 7.0s cast, range 50 width 30 rect
    MadeMagic2 = 23511, // Boss->self, 7.0s cast, range 50 width 30 rect
    LighterNote1 = 23512, // Boss->self, 3.0s cast, single-target
    LighterNote2 = 23513, // Helper->location, no cast, range 6 circle
    LighterNote3 = 23514, // Helper->location, no cast, range 6 circle
    DarkerNote1 = 23515, // Boss->self, 5.0s cast, single-target
    DarkerNote2 = 23516, // Helper->players, 5.0s cast, range 6 circle
    ScreamingScore = 23517, // Boss->self, 5.0s cast, range 60 circle
    SeedOfMagic = 23518, // Boss->self, 3.0s cast, single-target
    ScatteredMagic = 23519, // Helper->location, 3.0s cast, range 4 circle
}

public enum IconID : uint
{
    Icon1 = 1, // player
    Icon139 = 139, // player
}
