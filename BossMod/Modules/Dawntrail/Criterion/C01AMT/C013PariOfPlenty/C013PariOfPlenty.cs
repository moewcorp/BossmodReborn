namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

// TODO
//  - FireFlight:
//        - find out who gets the stack
//        - Add Enums value for left and right instead of using side number int
//  - WheelOfFableFlight - find out who gets the stack

/*
 Add Prey icon - during chains & baits - now we have the Status ID for it (Prey = 2939)	
Look into _Gen_ = 2056
Figure out what these two items are:
    _Gen_Icon_m0376trg_fire3_a0p = 97, // player->self
    _Gen_Icon_m0489trg_c0c = 138, // player->self
Confirm this is the tankBuster TankBuster = 342, // player->self

Clean up Enums OID
 
 */

//  - Fix hints for witch hunt - currently says "Stack" "Bait" etc at the same time - most likely just disable for now until priority is added
//  - Fix timeline
//  - Clean up enums + add missing ones
//  - Improve angle working out for FireFlightFourLongNight?
//  - Improve angle working for Fireflight first mechanic?
//  - Improve visual for SpurningFlames? - Really not needed, but could add indictors on the direction to go instead of showing the AOEs (assumes BD)

// - Clean up towers + add to timeline + add cleave from enemies
// - Add Pari's Curse mechanic with no prio at the moment - just remove it for now + draw tiles

class HeatBurst(BossModule module) : Components.RaidwideCast(module, (uint)AID.HeatBurst);

class FireOfVictory(BossModule module) : Components.BaitAwayCast(module, (uint)AID.FireOfVictory, 4f, true, true, true, AIHints.PredictedDamageType.Tankbuster);

class RedCrystals(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Ability_BurningGleam, new AOEShapeCross(40, 5), 3);

class RedCrystals2 : Components.SimpleAOEs {
    public RedCrystals2(BossModule module) : base(module, (uint)AID._Ability_BurningGleam, new AOEShapeCross(40, 5), 2) {
        Color = Colors.Danger;
    }
}

class KindleFlameStackIcon(BossModule module) : Components.StackTogether(module, (uint)IconID.StackIcon, 5, 6) {
    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        foreach (var target in Targets) {
            Arena.AddCircle(target.Position, Radius, Colors.Safe);
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(PariOfPlentyStates),
    ConfigType = null, // replace null with typeof(PariOfPlentyConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = null, // replace null with typeof(AID) if applicable
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.PariOfPlenty,
    Contributors = "",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.VariantCriterion,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1079u,
    NameID = 14274u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class PariOfPlenty(WorldState ws, Actor primary) : BossModule(ws, primary, new(-760f, -805f), new ArenaBoundsSquare(20f));