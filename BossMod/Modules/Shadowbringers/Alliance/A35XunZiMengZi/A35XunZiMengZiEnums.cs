namespace BossMod.Shadowbringers.Alliance.A35XunZiMengZi;

public enum OID : uint
{
    MengZi = 0x3196, // R15.000, x1
    XunZi = 0x3195, // R15.000, x1
    Energy = 0x3197, // R1.000, x24
    SerialJointedModel = 0x3199, // R2.400, x4
    SmallFlyer = 0x3198, // R1.320, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    DeployArmamentsVisual1 = 23552, // XunZi/MengZi->self, 6.0s cast, range 50 width 18 rect
    DeployArmamentsVisual2 = 23553, // MengZi->self, 7.0s cast, range 50 width 18 rect
    DeployArmamentsVisual3 = 23555, // XunZi/MengZi->self, 6.0s cast, range 50 width 18 rect
    DeployArmamentsVisual4 = 23556, // MengZi/XunZi->self, 7.0s cast, range 50 width 18 rect
    DeployArmaments1 = 23554, // Helper->self, 6.7s cast, range 50 width 18 rect
    DeployArmaments2 = 23557, // Helper->self, 7.0s cast, range 50 width 18 rect
    DeployArmaments3 = 24696, // Helper->self, 7.7s cast, range 50 width 18 rect
    DeployArmaments4 = 24697, // Helper->self, 8.0s cast, range 50 width 18 rect

    HighPoweredLaser = 23561, // SerialJointedModel->self, no cast, range 70 width 4 rect
    UniversalAssault = 23558, // XunZi/MengZi->self, 5.0s cast, range 50 width 50 rect
    LowPoweredOffensive = 23559, // SmallFlyer->self, 2.0s cast, single-target

    EnergyBomb = 23560 // Energy->player, no cast, single-target
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Helper/SerialJointedModel->player, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7
    UnknownStatus = 2056, // none->SerialJointedModel, extra=0x87
}

public enum IconID : uint
{
    Icon164 = 164, // player
}
