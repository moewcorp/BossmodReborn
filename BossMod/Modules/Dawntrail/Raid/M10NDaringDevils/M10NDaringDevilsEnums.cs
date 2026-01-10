namespace BossMod.DawnTrail.Raid.M10NDaringDevils;

public enum OID : uint
{
    RedHot = 0x4B53,
    DeepBlue = 0x4b54,
    Helper = 0x233C,
    TheXtremes = 0x4BDE, // R1.000, x2
    XtremeAether = 0x4B55, // R1.500, x28
    SickSwell = 0x4B56, // R1.000, x1
    _Gen_ = 0x4AE7, // R1.000, x4
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x1, EventObj type
    Exit = 0x1E850B, // R0.500, x1, EventObj type
    Actor1ebf31 = 0x1EBF31, // R0.500, x0 (spawn during fight), EventObj type
    Actor1ebf34 = 0x1EBF34, // R0.500, x0 (spawn during fight), EventObj type
    Actor1ebf32 = 0x1EBF32, // R0.500, x0 (spawn during fight), EventObj type
    Actor1ebf35 = 0x1EBF35, // R0.500, x0 (spawn during fight), EventObj type

}
public enum AID : uint
{
    AutoAttack = 48637, // 4B53->player, no cast, single-target
    HotImpact = 46464, // 4B53->players, 5.0s cast, range 6 circle
    AlleyOopInferno = 46470, // 4B53->self, 4.3+0.7s cast, single-target
    AlleyOopInferno1 = 46471, // 233C->player, 5.0s cast, range 5 circle
    Ability_ = 46456, // 4B53->location, no cast, single-target
    CutbackBlaze = 46478, // 4B53->self, 4.3+0.7s cast, single-target
    CutbackBlaze1 = 46479, // 233C->self, no cast, range 60 ?-degree cone
    DiversDare = 46466, // 4B53->self, 5.0s cast, range 60 circle
    AutoAttack1 = 48638, // 4B54->player, no cast, single-target
    SickSwell = 46480, // 4B54->self, 3.0s cast, single-target
    SickestTakeOff = 46482, // 4B54->self, 4.0s cast, single-target
    SickSwell1 = 46481, // 233C->self, 7.0s cast, range 50 width 50 rect
    SickestTakeOff1 = 46483, // 233C->self, 7.0s cast, range 50 width 15 rect
    Ability_1 = 46457, // 4B54->location, no cast, single-target
    DeepVarial = 47247, // 4B54->location, 5.3+1.0s cast, ???
    DeepVarial1 = 46488, // 233C->self, 6.8s cast, range 60 120.000-degree cone
    DeepImpact = 46465, // 4B54->player, 5.0s cast, range 6 circle
    XtremeSpectacular = 46498, // 4B54->self, 4.0s cast, single-target
    XtremeSpectacular1 = 46497, // 4B53->self, 4.0s cast, single-target
    XtremeSpectacular2 = 46499, // 4BDE->self, 7.4s cast, range 50 width 40 rect
    XtremeSpectacular3 = 46555, // 4BDE->self, no cast, range 60 circle
    XtremeSpectacular4 = 47049, // 4BDE->self, no cast, range 60 circle
    EpicBrotherhood = 46459, // 4B54->4B53, no cast, single-target
    EpicBrotherhood1 = 46458, // 4B53->4B54, no cast, single-target
    HotAerial = 46474, // 4B53->self, 4.9s cast, single-target
    HotAerial1 = 46475, // 4B53->location, no cast, single-target
    HotAerial2 = 46476, // 233C->self, 6.0s cast, range 6 circle
    Ability_2 = 46460, // 4B53->self, no cast, single-target
    SteamBurst = 46507, // 4B55->self, 3.0s cast, range 9 circle
    Pyrotation = 46472, // 4B53->self, 4.3+0.7s cast, single-target
    Pyrotation1 = 46473, // 233C->players, no cast, range 6 circle
    AlleyOopMaelstrom = 46495, // 233C->self, 3.0s cast, range 60 30.000-degree cone
    AlleyOopMaelstrom1 = 46494, // 4B54->self, 3.0s cast, single-target
    AlleyOopMaelstrom2 = 46496, // 233C->self, 3.0s cast, range 60 15.000-degree cone
    DiversDare1 = 46467, // 4B54->self, 5.0s cast, range 60 circle
    Watersnaking = 46463, // 4B54->self, 5.0s cast, range 60 circle
    Firesnaking = 46462, // 4B53->self, 5.0s cast, range 60 circle
    InsaneAir = 47252, // 4B54->self, 5.9+1.5s cast, single-target
    InsaneAir1 = 47251, // 4B53->self, 5.9+1.5s cast, single-target
    PlungingSnap = 46504, // 4B54->self, no cast, single-target
    BlastingSnap = 46503, // 4B53->self, no cast, single-target
    BlastingSnap1 = 46505, // 233C->self, no cast, range 60 ?-degree cone
    PlungingSnap1 = 46506, // 233C->self, no cast, range 60 ?-degree cone
    InsaneAir2 = 47254, // 4B54->self, 3.9+1.5s cast, single-target
    InsaneAir3 = 47253, // 4B53->self, 3.9+1.5s cast, single-target
    Ability_3 = 46461, // 4B54->self, no cast, single-target
}

public enum TetherID : uint
{
    _Gen_Tether_chn_m0982_2c = 372, // DeepBlue->4AE7
    water01f = 380, // player->DeepBlue
    fire001f = 381, // player->RedHot
}

public enum SID : uint
{
    _Gen_ = 2056, // none->DeepBlue, extra=0x435
    VulnerabilityUp = 1789, // Helper->player, extra=0x1
    Watersnaking = 4975, // none->player, extra=0x0
    Firesnaking = 4974, // none->player, extra=0x0
    DirectionalDisregard = 3808, // none->DeepBlue/RedHot, extra=0x0
    Burns1 = 3065, // none->player, extra=0x0
    Burns2 = 3066, // none->player, extra=0x0
    Cover = 2412, // none->DeepBlue, extra=0x64
    Covered = 2413, // DeepBlue->RedHot, extra=0x1

}
