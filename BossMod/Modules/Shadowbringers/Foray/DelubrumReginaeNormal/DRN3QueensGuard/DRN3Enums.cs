namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN3QueensGuard;

public enum OID : uint
{
    Warrior = 0x30BC, // R2.8
    Knight = 0x30BA, // R2.8
    Soldier = 0x30BF, // R4.0
    Gunner = 0x30C1, // R4.0
    SoldierAvatar = 0x30C0, // R4.0
    AetherialBurst = 0x30BE, // R1.2
    AetherialBolt = 0x30BD, // R0.6
    AetherialWard = 0x30BB, // R3.0
    AutomaticTurret = 0x30C2, // R4.0
    ArenaFeatures = 0x1EA1A1, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackOthers = 6497, // Soldier/Knight/Warrior->player, no cast, single-target
    AutoAttackGunner = 22615, // Gunner->player, no cast, single-target

    DoubleGambit = 22533, // Soldier->self, 3.0s cast, single-target
    SecretsRevealed = 23406, // Soldier->self, 5.0s cast, single-target, visual (tether 2 of 4 avatars that will be activated)
    SecretsRevealedExtra = 23408, // SoldierAvatar->self, no cast, single-target, visual (untether after secrets revealed?)
    ReversalOfForces = 22527, // Warrior->self, 4.0s cast, single-target, visual (remove tethers and apply statuses)
    ReversalOfForcesExtra = 23187, // Helper->self, 4.0s cast, single-target, visual (??? cast together with ReversalOfForces)
    PawnOffFake = 22535, // SoldierAvatar->self, 5.0s cast, range 20 circle fake aoe
    PawnOffReal = 22534, // SoldierAvatar->self, 5.0s cast, range 20 circle aoe
    Bombslinger = 23293, // Warrior->self, 3.0s cast, single-target
    AvatarJump = 22584, // SoldierAvatar->location, no cast, single-target

    AboveBoardExtra = 23409, // Helper->self, 6.0s cast, range 60 circle
    AboveBoard = 22524, // Warrior->self, 6.0s cast, range 60 circle

    LotsCastBigShort = 23403, // AetherialBurst->location, no cast, range 10 circle
    LotsCastSmallShort = 22526, // AetherialBolt->location, no cast, range 10 circle
    LotsCastBigLong = 22525, // AetherialBurst->location, no cast, range 10 circle
    LotsCastSmallLong = 23402, // AetherialBolt->location, no cast, range 10 circle

    RapidSeverKnight = 22523, // Knight->player, 5.0s cast, single-target, tankbuster
    RapidSeverWarrior = 22529, // Warrior->player, 5.0s cast, single-target, tankbuster
    RapidSeverSoldier = 22537, // Soldier->player, 5.0s cast, single-target, tankbuster
    ShotInTheDark = 22545, // Gunner->player, 5.0s cast, single-target, tankbuster

    ShieldOmen = 22513, // Knight->self, 3.0s cast, single-target
    SwordOmen = 22512, // Knight->self, 3.0s cast, single-target

    OptimalPlay = 22516, // Helper->self, 5.0s cast, single-target
    OptimalPlayShield = 22515, // Knight->self, 6.0s cast, range 5-60 donut
    OptimalPlaySword = 22514, // Knight->self, 6.0s cast, range 10 circle

    AutomaticTurret = 22539, // Gunner->self, 3.0s cast, single-target
    Reading = 22540, // Gunner->self, 5.0s cast, single-target
    TurretsTourFirst = 22541, // Helper->location, 5.0s cast, width 6 rect charge
    TurretsTourRest1 = 22542, // GunTurret->location, no cast, width 6 rect charge
    TurretsTourRest2 = 22543, // GunTurret->self, no cast, range 50 width 6 rect

    StrongpointDefense = 22517, // Knight->self, 5.0s cast, single-target
    CoatOfArmsLR = 22519, // AetherialWard->self, 4.0s cast, single-target
    CoatOfArmsFB = 22518, // AetherialWard->self, 4.0s cast, single-target
    Counterplay = 22520, // Helper->player, no cast, single-target

    BloodAndBoneKnight = 22522, // Knight->self, 5.0s cast, range 60 circle
    BloodAndBoneSoldier = 22536, // Soldier->self, 5.0s cast, range 60 circle
    BloodAndBoneWarrior = 22528, // Warrior->self, 5.0s cast, range 60 circle
    QueensShotUnseen = 22544, // Gunner->self, 5.0s cast, range 60 circle

    BloodAndBoneWarriorEnrage = 22530, // Warrior->self, 60.0s cast, range 60 circle
    BloodAndBoneKnightEnrage = 22521, // Knight->self, 60.0s cast, range 60 circle
    BloodAndBoneSoldierEnrage = 22538, // Soldier->self, 60.0s cast, range 60 circle
    QueensShotEnrage = 22546 // Gunner->self, 60.0s cast, range 60 circle
}
