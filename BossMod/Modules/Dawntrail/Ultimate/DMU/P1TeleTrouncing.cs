namespace BossMod.Dawntrail.Ultimate.DMU;

using Direction = P1TeleTrouncingData.Direction;

// TODO add stack arena for puddles - currently you can actually take either, it just assumes you will take the one always closer to you
// Might need to setup something up for this

/*
Keep gravenImage 3 static and non-static spot option
 */

// TODO setup knockback part - before standing in front / behind arrows
// TODO setup gravenImage 3 static and non-static spot
// TODO final mechanic - pre-position and solve it

// TODO fixes:
//  1. Tank busters - send players south, some tanks are strange and will angle the tank buster at around East and West which can kill the player
//      shouldn't happen, but can, so maintain the player keeping them south of a donut shape around the boss to potentially
//      still do positional on the back if needed
//  2. Gravitas puddles - move puddle baits slightly up so its more precise - just change the position of the forbidden zone to be more precise, I think
//      its currently 2 squares long when it should just be one
//  3. Gravitas stacks are the wrong colour - sometimes green, sometimes yellow
//  4. REFER to comment at top of page - knockback 2 where taking puddles, make a bitmask for which puddle zone they should take and get the midpoint
//      of the puddles while keeping in melee range if possible to ensure we take all the puddles
//  5. For double trouble 2 change the timer from 1.2 seconds to 0.9 seconds and see if it improves it

sealed class TeleTrouncing : Components.GenericAOEs {
    private readonly record struct arrowDebuff(Direction direction, DateTime activation);
    private readonly List<List<arrowDebuff>> debuffs = [];
    private readonly List<List<(WPos safeSpot, DateTime activation)>> arrowHints = [];
    private readonly List<AOEInstance> arrows = [];
    private readonly AOEShapeCircle shape = new(2.0f);
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public TeleTrouncing(BossModule module) : base(module) {
        for (int i = 0; i < PartyState.MaxPartySize; i++) {
            debuffs.Add([
                new arrowDebuff(),
                new arrowDebuff()
            ]);

            arrowHints.Add([]);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        Direction direction = (SID)status.ID switch {
            SID.TelePortentUP or SID.TelePortentUP2 => Direction.UP,
            SID.TelePortentDOWN or SID.TelePortentDOWN2 => Direction.DOWN,
            SID.TelePortentLEFT or SID.TelePortentLEFT2 => Direction.LEFT,
            SID.TelePortentRIGHT or SID.TelePortentRIGHT2 => Direction.RIGHT,
            _ => Direction.NONE
        };

        if (direction == Direction.NONE) {
            return;
        }

        if (Raid.FindSlot(actor.InstanceID) is var slot && slot < 0) {
            return;
        }

        var duration = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds;
        if (duration > 8) {
            debuffs[slot][1] = new arrowDebuff(direction, status.ExpireAt);
        } else {
            debuffs[slot][0] = new arrowDebuff(direction, status.ExpireAt);
        }

        var firstArrow = debuffs[slot][0];
        var secondArrow = debuffs[slot][1];

        if (firstArrow.direction == Direction.NONE || secondArrow.direction == Direction.NONE) {
            return;
        }

        if (!P1TeleTrouncingData.TryGetSafeSpots(dmuConfig.P1TeleTrouncing, firstArrow.direction, secondArrow.direction, out var safeSpots)) {
            return;
        }

        if (safeSpots.arrow1.direction == firstArrow.direction) {
            arrowHints[slot].Add((safeSpots.arrow1.safeSpot, firstArrow.activation));
            arrowHints[slot].Add((safeSpots.arrow2.safeSpot, secondArrow.activation));
        } else {
            arrowHints[slot].Add((safeSpots.arrow2.safeSpot, firstArrow.activation));
            arrowHints[slot].Add((safeSpots.arrow1.safeSpot, secondArrow.activation));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status) {
        Direction direction = (SID)status.ID switch {
            SID.TelePortentUP or SID.TelePortentUP2 => Direction.UP,
            SID.TelePortentDOWN or SID.TelePortentDOWN2 => Direction.DOWN,
            SID.TelePortentLEFT or SID.TelePortentLEFT2 => Direction.LEFT,
            SID.TelePortentRIGHT or SID.TelePortentRIGHT2 => Direction.RIGHT,
            _ => Direction.NONE
        };

        if (direction == Direction.NONE) {
            return;
        }

        if (Raid.FindSlot(actor.InstanceID) is var slot && slot < 0) {
            return;
        }

        if (arrowHints[slot].Count != 0) {
            arrowHints[slot].RemoveAt(0);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.TeleTrouncing1) {
            NumCasts++;
        }
    }

    public override void OnActorCreated(Actor actor) {
        if (actor.OID == (uint)OID.TeleTrouncingArrow) {
            arrows.Add(new(shape, actor.Position, actor.Rotation, actorID: actor.InstanceID));
        }
    }

    public override void OnActorDestroyed(Actor actor) {
        if (actor.OID == (uint)OID.TeleTrouncingArrow) {
            arrows.RemoveAll(aoe => aoe.ActorID == actor.InstanceID);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.TeleTrouncingNone) {
            return;
        }

        if (arrowHints[pcSlot].Count == 0) {
            return;
        }

        for (int i = 0; i < arrowHints[pcSlot].Count; i++) {
            Arena.ZoneCircleOutline(arrowHints[pcSlot][i].safeSpot, PositionDrawSize.NORMAL, i == 0 ? Colors.Safe : Colors.Danger, 2.0f);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc) {
        base.DrawArenaBackground(pcSlot, pc);

        if (arrows.Count == 0) {
            return;
        }

        foreach (var arrow in arrows) {
            Arena.AddLine(arrow.Origin + arrow.Rotation.ToDirection(), arrow.Origin + (arrow.Rotation + 90.Degrees()).ToDirection(), Colors.Background);
            Arena.AddLine(arrow.Origin + arrow.Rotation.ToDirection(), arrow.Origin + (arrow.Rotation - 90.Degrees()).ToDirection(), Colors.Background);
            Arena.AddLine(arrow.Origin + arrow.Rotation.ToDirection(), arrow.Origin + (arrow.Rotation + 180.Degrees()).ToDirection(), Colors.Background);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);

        if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.TeleTrouncingNone) {
            return;
        }

        if (arrowHints[slot].Count == 0 || debuffs[slot].Count == 0) {
            return;
        }

        hints.AddForbiddenZone(new SDInvertedCircle(arrowHints[slot][0].safeSpot, PositionAIRadius.PRECISE), arrowHints[slot][0].activation);
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(arrows);
}
