namespace BossMod.RealmReborn.Dungeon.D17PharosSirius.D171SymondTheUnsinkable;


public enum OID : uint
{
    SymondTheUnsinkable = 0x8FB,
    Helper = 0x233C,
    ZombieWarHound = 0x8FC, // R1.650, x?
    CorruptedCrystal = 0x1B2, //1B2/Corrupted Crystal/
    _Gen_Actor1e8f29 = 0x1E8F29, // R2.000, x?, EventObj type : single voidzone growth? : actually no idea
    _Gen_Actor1e9058 = 0x1E9058, // R0.500, x?, EventObj type : two voidzone growth first?
    _Gen_Actor1e9059 = 0x1E9059, // R0.500, x?, EventObj type : two voidzone growth second?
    _Gen_Actor1e905a = 0x1E905A, // R0.500, x?, EventObj type : two voidzone growth third?
    _Gen_Exit = 0x1E850B, // R0.500, x?, EventObj type

}

public enum AID : uint
{
    AutoAttack = 870, // SymondTheUnsinkable/ZombieWarHound->player, no cast, single-target
    PiercingThrust = 1527, // SymondTheUnsinkable->player, no cast, single-target
    CorruptingShot = 1498, // SymondTheUnsinkable->self, 2.2s cast, range 30+R width 3 rect
    CorruptingSpit = 1499, // ZombieWarHound->location, 1.0s cast, range 6 circle
    CrystallineShot = 1501, // SymondTheUnsinkable->self, 2.5s cast, single-target
    CrystallineShower = 1542, // CorruptedCrystal->location, 3.5s cast, range 9 circle
    GigaSlash = 1502, // SymondTheUnsinkable->self, 4.0s cast, range 20+R circle
    CorruptingBurst = 1500, // CorruptedCrystal->player, no cast, range 6 circle
}

public enum SID : uint
{
    CorruptedCrystal = 374, // ZombieWarHound->player, extra=0x1/0x2/0x3
    VulnerabilityUp = 202, // CorruptedCrystal->player, extra=0x1/0x2
}





sealed class CorruptingShot(BossModule module)
    : Components.SimpleAOEs(module, (uint)AID.CorruptingShot, new AOEShapeRect(50f, 1.5f));

sealed class WarHoundAdds(BossModule module) : Components.Adds(module, (uint)OID.ZombieWarHound, 1);
sealed class CorruptingSpit(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CorruptingSpit, 6);
//    CrystallineShower = 1542, // CorruptedCrystal->location, 3.5s cast, range 9 circle
// I think this grows? if not find the ones that do grow to big giant spots.
sealed class CrystallineShower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrystallineShower, 9);

//    GigaSlash = 1502, // SymondTheUnsinkable->self, 4.0s cast, range 20+R circle : knockback here distance 10? away from origin
sealed class GigaSlash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GigaSlash, 20);

/*
 * If you reach 3 stacks of Corrupting Crystal, run away from everyone so the explosion doesn't hit them. Since you
 * are already at 3 stacks, you can run into the Crystalline Shot puddles if necessary.
 *     CorruptingBurst = 1500, // CorruptedCrystal->player, no cast, range 6 circle

 */


[SkipLocalsInit]
sealed class SymondTheUnsinkableStates : StateMachineBuilder
{
    public SymondTheUnsinkableStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CorruptingShot>()
            .ActivateOnEnter<WarHoundAdds>()
            .ActivateOnEnter<CorruptingSpit>()
            .ActivateOnEnter<CrystallineShower>()
            .ActivateOnEnter<GigaSlash>()

            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(SymondTheUnsinkableStates),
    ConfigType = null, // replace null with typeof(SymondTheUnsinkableConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = null, // replace null with typeof(AID) if applicable
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.SymondTheUnsinkable,
    Contributors = "wen",
    Expansion = BossModuleInfo.Expansion.RealmReborn,
    Category = BossModuleInfo.Category.Dungeon,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 17u,
    NameID = 2259u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
// technically arena center is (42, 30, -56) if you want to visit in hyperborea.
public sealed class SymondTheUnsinkable(WorldState ws, Actor primary) : BossModule(ws, primary, new(42f, -56f), new ArenaBoundsCircle(20f));
