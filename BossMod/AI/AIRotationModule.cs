﻿using BossMod.Autorotation;
using BossMod.Pathfinding;

namespace BossMod.AI;

public abstract class AIRotationModule(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    protected NavigationDecision.Context NavigationContext = new();

    protected float Deadline(DateTime deadline) => Math.Max(0f, (float)(deadline - World.CurrentTime).TotalSeconds);
    protected float Speed() => Player.FindStatus((uint)ClassShared.SID.Sprint) != null ? 7.8f : 6f;

    protected bool InMeleeRange(Actor target, WPos position)
    {
        var maxRange = target.HitboxRadius + 2.6f;
        return (target.Position - position).LengthSq() < maxRange * maxRange;
    }

    protected bool InMeleeRange(Actor target) => InMeleeRange(target, Player.Position);

    protected void SetForcedMovement(WPos? pos, float tolerance = 0.1f)
    {
        if (pos != null)
        {
            var dir = pos.Value - Player.Position;
            Hints.ForcedMovement = dir.LengthSq() > tolerance * tolerance ? dir.ToVec3(Player.PosRot.Y) : default;
        }
    }

    protected WPos ClosestInRange(WPos pos, WPos target, float maxRange)
    {
        var toPlayer = pos - target;
        var range = toPlayer.Length();
        return range <= maxRange ? pos : target + maxRange * toPlayer.Normalized();
    }

    protected WPos ClosestInMelee(WPos pos, Actor target, float tolerance = 0f) => ClosestInRange(pos, target.Position, target.HitboxRadius + 2.6f - tolerance);

    // return uptime position if player will be able to reach downtime position within deadline having started movement after next action, otherwise downtime position
    protected WPos UptimeDowntimePos(WPos uptimePos, WPos downtimePos, float nextAction, float deadline)
    {
        var timeToSafety = (uptimePos - downtimePos).Length() / Speed();
        return nextAction + timeToSafety < deadline ? uptimePos : downtimePos;
    }

    // return position that will make the target (assumed aggro'd) move to specified position, ensuring we're back in melee range by the deadline
    protected WPos MoveTarget(Actor target, WPos desired, float nextAction, float targetMeleeRange = 2f)
    {
        var dir = desired - target.Position;
        if (dir.LengthSq() < 0.01f)
            return ClosestInRange(Player.Position, target.Position, target.HitboxRadius + targetMeleeRange - 0.1f);
        dir = dir.Normalized();
        var ideal = desired + dir * (target.HitboxRadius + targetMeleeRange); // if we just stay here, boss should go to the desired position
        var targetMoveDir = (ideal - target.Position).Normalized();
        var playerDotTargetMove = targetMoveDir.Dot(ideal - Player.Position);
        if (playerDotTargetMove < 0)
            ideal -= playerDotTargetMove * targetMoveDir; // don't move towards boss, though
        var targetRemaining = (ideal - target.Position).Length() - target.HitboxRadius - targetMeleeRange - (target.Position - target.PrevPosition).Length() / World.Frame.Duration * nextAction - Speed() * nextAction;
        if (targetRemaining > 0)
            ideal += targetRemaining * (target.Position - ideal).Normalized();
        return ideal;
    }
}
