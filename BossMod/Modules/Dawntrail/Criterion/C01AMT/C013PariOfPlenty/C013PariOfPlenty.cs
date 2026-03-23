namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

// TODO
//  - Fix hints for witch hunt - currently says "Stack" "Bait" etc at the same time - most likely just disable for now until priority is added
//  - Fix timeline
//  - Clean up enums + add missing ones
//  - FireflightStackSpread - find out who gets the stack
//  - WheelofFableFlight - find out who gets the stack + clean up
//  - Improve angle working out for FireFlightFourLongNight?
//  - Improve angle working for Fireflight first mechanic?
//  - Improve visual for SpurningFlames? - Really not needed, but could add indictors on the direction to go instead of showing the AOEs (assumes BD)

// - Clean up towers + add to timeline + add cleave from enemies
// - Implement mechanic after towers - just red crystals + stacks - with icon
// - Implement final mechanic - turning (most likely need to use cast ID for new position the boss faces?) + red crystals - highlight bright yellow
//      - Show one at time and see how it looks? Then compare it to two
// - Add Pari's Curse mechanic with no prio at the moment - just remove it for now + draw tiles

class HeatBurst(BossModule module) : Components.RaidwideCast(module, (uint)AID.HeatBurst);

class FireOfVictory(BossModule module) : Components.BaitAwayCast(module, (uint)AID.FireOfVictory, 4f, true, true, true, AIHints.PredictedDamageType.Tankbuster);

class RedCrystals(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Ability_BurningGleam, new AOEShapeCross(40, 5), 3);

class RedCrystals2 : Components.SimpleAOEs {
    public RedCrystals2(BossModule module) : base(module, (uint)AID._Ability_BurningGleam, new AOEShapeCross(40, 5), 2) {
        Color = Colors.Danger;
    }
}

class KindleFlameStackIcon(BossModule module) : Components.StackTogether(module, (uint)IconID._Gen_Icon_com_share3t, 5, 6) {
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