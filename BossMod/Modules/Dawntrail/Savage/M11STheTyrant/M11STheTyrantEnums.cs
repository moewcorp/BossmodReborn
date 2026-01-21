namespace BossMod.Dawntrail.Savage.M11STheTyrant;

public enum OID : uint
{
    Boss = 0x4AEC,
    Helper = 0x233C,
    _Gen_TheTyrant = 0x4AEA, // R3.000-30.000, x?
    _Gen_TheTyrant1 = 0x233C, // R0.500, x?, Helper type
    _Gen_ = 0x4AF2, // R1.000, x?
    Comet = 0x4AED, // R2.160, x? ; These are the rocks that spawn from Meteorain
    _Gen_1 = 0x4AF0, // R1.000, x?
    _Gen_2 = 0x4AF1, // R1.000, x?
    _Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x?, EventObj type
    _Gen_TheTyrant2 = 0x4AEC, // R5.000-10.000, x?
    _Gen_Exit = 0x1E850B, // R0.500, x?, EventObj type
    Maelstrom = 0x4AEF, // R1.000, x? ; These are the whirlwinds that spawn during ultimate trophy weapons
    _Gen_Actor0 = 0x0, // R340282346638528859811704183484516925440.000--340282346638528859811704183484516925440.000, x?, None type
}

public enum AID : uint
{
    AutoAttack = 46085, // 4AEC->player, no cast, single-target
    CrownOfArcadia = 46086, // 4AEC->self, 5.0s cast, range 60 circle
    RawSteelTrophy = 46115, // 4AEC->self, 2.0+4.0s cast, single-target ; summons weapon, this was a scythe
    RawSteel = 46093, // 4AEC->self, no cast, single-target
    RawSteel1 = 46094, // 4AEC->self, no cast, single-target
    RawSteel2 = 46095, // 233C->self, no cast, range 60 90-degree cone ; Tanks spread
    HeavyHitter = 46096, // 233C->self, no cast, range 60 ?-degree cone ; Party stack cone for RawSteelTrophy
    _Ability_ = 46084, // 4AEC->location, no cast, single-target
    TrophyWeapons = 46102, // 4AEC->self, 3.0s cast, single-target ; summons 3 weapons on floor
    AssaultEvolved = 46103, // 4AEC->self, 6.0s cast, single-target ; Animation showing jump to first weapon
    AssaultEvolved_ScytheDash = 46404, // 4AEC->location, no cast, single-target ; Jump to scythe
    AssaultEvolved_SweepingVictory = 46108, // 233C->self, no cast, range 60 ?-degree cone ; fire 8 spread cones at party
    AssaultEvolved_ScytheDonut = 46105, // 233C->self, 2.0s cast, range 5-60 donut ; Donut AOE after jumping to scythe
    AssaultEvolved_AxeDash = 46403, // 4AEC->location, no cast, single-target ; Jump to axe
    AssaultEvolved_HeavyWeight = 46107, // 233C->players, no cast, range 6 circle ; Axe party stack
    AssaultEvolved_AxeAOE = 46104, // 233C->self, 2.0s cast, range 8 circle ; Axe out AOE
    AssaultEvolved_SwordDash = 46405, // 4AEC->location, no cast, single-target ; Jump to sword
    AssaultEvolved_SharpTaste = 46109, // 233C->self, no cast, range 60 width 6 rect ; healer cone stacks on sword
    AssaultEvolved_SwordCross = 46106, // 233C->self, 2.0s cast, range 40 width 10 cross ; sword cross AOE
    VoidStardust = 46098, // 4AEC->self, 4.0+1.0s cast, single-target
    Cometite = 46099, // 233C->location, 3.0s cast, range 6 circle ; these are the multi puddle meteors
    CrushingComet = 46101, // 233C->players, 5.0s cast, range 6 circle ; this is the party stack from icon 161
    Comet = 46100, // 233C->players, 5.0s cast, range 6 circle ; spread AOE variation icon 139
    DanceOfDominationTrophy = 47035, // 4AEC->self, 2.0+4.0s cast, single-target
    DanceOfDomination = 46110, // 4AEC->self, no cast, single-target
    DanceOfDomination1 = 46111, // 233C->self, 0.5s cast, range 60 circle
    DanceOfDomination2 = 46113, // 233C->self, no cast, range 60 circle
    DanceOfDomination3 = 47082, // 233C->self, no cast, range 60 circle
    EyeOfTheHurricane = 46116, // 233C->players, 5.0s cast, range 6 circle ; This is the 4 party stacks after dance of dominion
    Explosion1 = 46112, // 233C->self, 5.0s cast, range 60 width 10 rect
    Explosion2 = 47036, // 233C->self, 5.5s cast, range 60 width 10 rect
    RawSteelTrophy1 = 46114, // 4AEC->self, 2.0+4.0s cast, single-target ; This is the axe trophy
    RawSteel3 = 46090, // 4AEC->self, no cast, single-target
    RawSteel4 = 46091, // 4AEC->players, no cast, range 6 circle
    Impact = 46092, // 233C->player, no cast, range 6 circle
    Charybdistopia = 46117, // 4AEC->self, 5.0s cast, range 60 circle ; Drop to 1hp
    UltimateTrophyWeapons = 47085, // 4AEC->self, 3.0s cast, single-target ; Good place to transition state
    AssaultApex = 47086, // 4AEC->self, 5.0s cast, single-target
    PowerfulGust = 46119, // 233C->self, no cast, range 60 ?-degree cone ; Maelstrom baited cones
    ImmortalReign = 46120, // 4AEC->self, 3.0+1.0s cast, single-target
    OneAndOnly = 46121, // 4AEC->self, 6.0+2.0s cast, single-target
    OneAndOnly1 = 46122, // 233C->self, 9.0s cast, range 60 circle
    GreatWallOfFire = 46123, // 4AEC->self, 5.0s cast, single-target
    GreatWallOfFire1 = 46124, // 4AEC->self, no cast, range 60 width 6 rect
    GreatWallOfFire2 = 46125, // 233C->self, no cast, single-target
    GreatWallOfFireExplosion = 46126, // 233C->self, 3.0s cast, range 60 width 6 rect ; tankbuster aftershock
    _Spell_OrbitalOmen = 46130, // 4AEC->self, 5.0s cast, single-target
    _Weaponskill_FireAndFury = 46127, // 4AEC->self, 4.0+1.0s cast, single-target
    OrbitalOmen_Lines = 46131, // 233C->self, 6.0s cast, range 60 width 10 rect
    FireAndFuryFront = 46129, // 233C->self, 5.0s cast, range 60 90-degree cone
    FireAndFuryBack = 46128, // 233C->self, 5.0s cast, range 60 90-degree cone
    Meteorain = 46132, // 4AEC->self, 5.0s cast, single-target
    FearsomeFireball = 46137, // 4AEC->self, 5.0s cast, single-target ; icon 525 line stack
    FearsomeFireball1 = 46138, // 4AEC->self, no cast, range 60 width 6 rect ; icon 525 line stack
    CosmicKiss = 46133, // 4AED->self, no cast, range 4 circle ; icon 244 this is the comet impact which leaves a rock
    ForegoneFatality = 46134, // 4AEA->player/4AED, no cast, single-target ; tank tether damage
    Comet_Explosion = 46136, // 4AED->self, 3.0s cast, range 8 circle ; comets explode when they die
    _Ability_UnmitigatedExplosion = 46135, // 4AED->self, no cast, range 60 circle
    TripleTyrannhilation1 = 46139, // 4AEC->self, no cast, single-target
    _Spell_Charybdis = 46118, // 233C->player, no cast, single-target
    TripleTyrannhilation2 = 46140, // 4AEC->self, 7.0+1.0s cast, single-target
    _Weaponskill_Shockwave = 46141, // 233C->self, no cast, range 60 circle
    _Ability_1 = 46142, // 4AED->self, no cast, single-target
    _Weaponskill_ = 46176, // 4AEC->self, no cast, single-target
    Flatliner = 46143, // 4AEC->self, 4.0+2.0s cast, single-target
    Flatliner1 = 47760, // 233C->self, 6.0s cast, range 60 circle
    _Spell_MajesticMeteor = 46144, // 4AEC->self, 5.0s cast, single-target
    _Spell_Explosion = 46148, // 233C->self, 10.0s cast, range 4 circle
    _Spell_UnmitigatedExplosion = 46149, // 233C->self, no cast, range 60 circle
    _Spell_MajesticMeteor1 = 46145, // 233C->location, 3.0s cast, range 6 circle
    FireBreath = 46150, // 4AEC->self, 8.0+1.0s cast, single-target
    FireBreath1 = 46151, // 233C->self, no cast, range 60 width 6 rect
    _Spell_MajesticMeteorain = 46146, // 233C->self, no cast, range 60 width 10 rect
    _Spell_MajesticMeteowrath = 46147, // 233C->self, no cast, range 60 width 10 rect
}

public enum IconID : uint
{
    CrushingCometIcon = 161, // player->self ; This is the icon for Crushing Comet stack spell 46101, also used for Eye of the Hurricane 46116
    CometIcon = 139, // player->self ; This is the icon for Comet spell 46100
    CosmicKissIcon = 244, // player->self ; This is the icon for Cosmic Kiss spell 46133 which drops comets
    FearsomeFireballIcon = 525, // Boss->player ; line stack icon for Fearsome Fireball 
}

public enum TetherID : uint
{
    CometTether = 356, // _Gen_TheTyrant->_Gen_Comet/player ; This is the tether that tanks take off comets
    _Gen_Tether_chn_arrow01f = 57, // _Gen_TheTyrant->player ; First tether in split arena
    _Gen_Tether_chn_tergetfix2k1 = 249, // _Gen_TheTyrant->player ; Second tether in split arena
}
