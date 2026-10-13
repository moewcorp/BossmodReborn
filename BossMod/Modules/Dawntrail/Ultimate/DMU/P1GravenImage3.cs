namespace BossMod.Dawntrail.Ultimate.DMU;

// Melee roles will look at boss and decide if they need to move or not
//  1. Melee is not in the lightning at all => no movement needed
//  2. Melee is slightly in the lightning => side step movement
//  3. Melee is completely in the lightning => go under the boss
// Range roles will stand still from where they're from the arrow mechanic and then freely dodge where available, but away from the melees
sealed class GravenImage3FlagrantFire(BossModule module) : FlagrantFire(module) {
    private readonly TeleTrouncingTethers? teleTrouncingTethers = module.FindComponent<TeleTrouncingTethers>();
    private readonly LightningSafeSpots? lightningSafeSpots = module.FindComponent<LightningSafeSpots>();

    protected override void drawSafeSpots(int pcSlot, Actor pc) {
        if (dmuConfig.P1GravenImage3 == DMUConfig.P1GravenImage3Strategy.GravenImage3None) {
            return;
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0) {
            return;
        }
        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

        var safeSpot = SafeSpot(pc, assignment);
        if (safeSpot == default) {
            return;
        }

        Arena.ZoneCircleOutline(safeSpot, PositionDrawSize.NORMAL, Colors.Safe, 2.0f);
    }

    protected override void addAIHintSafeSpots(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        if (dmuConfig.P1GravenImage3 == DMUConfig.P1GravenImage3Strategy.GravenImage3None) {
            return;
        }

        var safeSpot = SafeSpot(actor, assignment);
        // Lightning aoes are not active yet => pre-position
        if (lightningSafeSpots == null || lightningSafeSpots.Casters.Count == 0) {
            if (safeSpot != default) {
                hints.GoalZones.Add(AIHints.GoalProximity(safeSpot, PositionAIRadius.NORMAL, PositionWeights.PRE_POSITION));
            }
            return;
        }

        // Lightning aoes are now active => spot is dependent if the player is a melee or range.
        // Range players: They have no safeSpots (LPDU says to just basically freely spread and use the room), so we put fake spreads around the melee
        // positions since we know they will be taken by the melee players.
        if (safeSpot == default) {
            // This only matters for when it is spread
            if (mechanic == SpreadStack.Stack) {
                return;
            }

            addBasicAIHints(slot, actor, assignment, hints);
            foreach (var meleePosition in P1GravenImage3Data.MeleePrePositions.Keys) {
                hints.AddForbiddenZone(new SDCircle(SafeSpot(actor, meleePosition), SpreadRadius), lightningSafeSpots.Casters[0].Activation);
            }

            return;
        }

        // Melee players: They have a safeSpot always around the boss
        hints.AddForbiddenZone(new SDInvertedCircle(safeSpot, PositionAIRadius.PRECISE), lightningSafeSpots.Casters[0].Activation);
    }

    private WPos SafeSpot(Actor actor, PartyRolesConfig.Assignment assignment) {
        if (dmuConfig.P1GravenImage3 == DMUConfig.P1GravenImage3Strategy.GravenImage3None) {
            return default;
        }

        // lightning aoes are not active yet => go to pre-position spot
        if (lightningSafeSpots == null || lightningSafeSpots.Casters.Count == 0) {
            return prePositionSpot(assignment);
        }

        // Case: the mechanic is stack: This means we only care about the line going through the boss to stand correctly
        if (mechanic == SpreadStack.Stack) {
            var boss = Module.PrimaryActor;
            var lightningDirection = lightningSafeSpots.Casters[0].Rotation.ToDirection();
            var northZone = lightningDirection.Z < 0.0f ? lightningDirection : - lightningDirection;
            var stackZone = actor.Class.IsSupport() ? -northZone : northZone;
            var safeSpot = boss.Position + stackZone * (boss.HitboxRadius + 1.0f);

            foreach (var aoe in lightningSafeSpots.Casters) {
                if (aoe.ShapeDistance == null) {
                    continue;
                }

                var distanceFromLightning = aoe.ShapeDistance.Distance(safeSpot);
                if (distanceFromLightning >= 1.0f) {
                    continue;
                }

                var lightningSideways = aoe.Rotation.ToDirection().OrthoL();
                var side = (safeSpot - aoe.Origin).Dot(lightningSideways);
                var directionToMove = side >= 0 ? lightningSideways : -lightningSideways;
                safeSpot = safeSpot + directionToMove * (1.0f - distanceFromLightning);
            }

            return safeSpot;
        }

        // Case: the mechanic is spread: the spots are dependent if the player is a melee or range role
        if (mechanic == SpreadStack.Spread) {
            // If the player is assigned a range role they spread freely
            if (!P1GravenImage3Data.MeleePrePositions.TryGetValue(assignment, out var prePosition)) {
                return default;
            }

            foreach (var aoe in lightningSafeSpots.Casters) {
                if (aoe.ShapeDistance == null) {
                    continue;
                }

                var distanceFromLightning = aoe.ShapeDistance.Distance(prePosition);
                if (distanceFromLightning >= 1.0f) {
                    continue;
                }

                // Case: 3. Melee is completely in the lightning => go under the boss
                if (distanceFromLightning < -1.0f) {
                    return Module.PrimaryActor.Position;
                }

                // Case: 2. Melee is slightly in the lightning => side step movement
                var lightningSideways = aoe.Rotation.ToDirection().OrthoL();
                var side = (prePosition - aoe.Origin).Dot(lightningSideways);
                var directionToMove = side >= 0 ? lightningSideways : -lightningSideways;
                var safeSpotMove = prePosition + directionToMove * (1.0f - distanceFromLightning);
                return safeSpotMove;
            }

            // Case: 1. Melee is not in the lightning at all => no movement needed
            return prePositionSpot(assignment);
        }

        return default;
    }

    // Handles the pre-position for players before the lightning aoes show up
    private WPos prePositionSpot(PartyRolesConfig.Assignment assignment) {
        WPos prePosition = default;

        // Case: melee pre-position spot
        if (P1GravenImage3Data.MeleePrePositions.TryGetValue(assignment, out prePosition)) {
            return prePosition;
        }

        if (teleTrouncingTethers == null || teleTrouncingTethers.supportHasSleep == null) {
            return default;
        }

        // Case: range pre-position spot
        if (P1GravenImage3Data.RangePrePositions[teleTrouncingTethers.supportHasSleep.Value].TryGetValue(assignment, out prePosition)) {
            return prePosition;
        }

        return default;
    }
}
