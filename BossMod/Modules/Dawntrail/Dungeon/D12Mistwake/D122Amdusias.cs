// namespace BossMod.Dawntrail.Dungeon.D12Mistwake.D122Amdusias;

// public enum OID : uint
// {
//     Amdusias = 0x4A77, // R3.96
//     PoisonCloud = 0x4A78, // R1.2
//     Helper = 0x233C
// }

// public enum AID : uint
// {
//     AutoAttack = 46839, // Amdusias->player, no cast, single-target
//     Teleport = 45357, // Amdusias->location, no cast, single-target

//     StaticCharge1 = 45333, // Amdusias->self, no cast, single-target
//     StaticCharge2 = 45335, // Amdusias->self, no cast, single-target
//     ThunderclapConcertoVisual1 = 45336, // Amdusias->self, 5.0+0,5s cast, single-target
//     ThunderclapConcertoVisual2 = 45341, // Amdusias->self, 5.0+0,5s cast, single-target
//     ThunderclapConcerto1 = 45337, // Helper->self, 5.5s cast, range 40 ?-degree cone
//     ThunderclapConcerto2 = 45342, // Helper->self, 5.5s cast, range 40 ?-degree cone

//     BioIIVisual = 45344, // Amdusias->self, 5.0s cast, single-target
//     BioII = 45345, // Helper->location, 5.0s cast, range 20 circle

//     GallopingThunderVisual = 45346, // Amdusias->location, 10.0s cast, single-target
//     GallopingThunderTelegraph = 45348, // Helper->location, 1.5s cast, width 5 rect charge
//     GallopingThunder = 45347, // Amdusias->location, no cast, width 5 rect charge
//     Burst = 45349, // PoisonCloud->self, 2.5s cast, range 9 circle
//     ThunderIVVisual = 45350, // Amdusias->self, 4.4+0,6s cast, single-target
//     ThunderIV = 45351, // Helper->self, 5.0s cast, range 70 circle
//     ShockboltVisual = 45355, // Amdusias->self, 4.4+0,6s cast, single-target
//     Shockbolt = 45356, // Helper->player, 5.0s cast, single-target, tankbuster
//     Thunder = 45343, // Helper->player, 5.0s cast, range 5 circle
//     ThunderIIIVisual = 45352, // Amdusias->self, 4.4+0,6s cast, single-target, stack x3
//     ThunderIIIFirst = 45353, // Helper->players, 5.0s cast, range 6 circle
//     ThunderIIIRepeat = 45354 // Helper->players, no cast, range 6 circle
// }

// [SkipLocalsInit]
// sealed class D122AmdusiasStates : StateMachineBuilder
// {
//     public D122AmdusiasStates(BossModule module) : base(module)
//     {
//         TrivialPhase();
//     }
// }

// [ModuleInfo(BossModuleInfo.Maturity.WIP,
// StatesType = typeof(D122AmdusiasStates),
// ConfigType = null,
// ObjectIDType = typeof(OID),
// ActionIDType = typeof(AID),
// StatusIDType = null,
// TetherIDType = null,
// IconIDType = null,
// PrimaryActorOID = (uint)OID.Amdusias,
// Contributors = "",
// Expansion = BossModuleInfo.Expansion.Dawntrail,
// Category = BossModuleInfo.Category.Dungeon,
// GroupType = BossModuleInfo.GroupType.CFC,
// GroupID = 1064u,
// NameID = 14271u,
// SortOrder = 2,
// PlanLevel = 0)]
// [SkipLocalsInit]
// public sealed class D122Amdusias : BossModule
// {
//     public D122Amdusias(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

//     private D122Amdusias(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

//     private static (WPos center, ArenaBoundsCustom arena) BuildArena()
//     {
//         var arena = new ArenaBoundsCustom([new Polygon(new(281f, -285f), 19.5f, 64)], [new Rectangle(new(281f, -305f), 8f, 1.25f), new Rectangle(new(281f, -265f), 8f, 1.25f)]);
//         return (arena.Center, arena);
//     }
// }
