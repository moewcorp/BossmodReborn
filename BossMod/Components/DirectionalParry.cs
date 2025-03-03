﻿namespace BossMod.Components;

// generic 'directional parry' component that shows actors and sides it's forbidden to attack them from
// uses common status + custom prediction
public class DirectionalParry(BossModule module, uint[] actorOID) : AddsMulti(module, actorOID)
{
    [Flags]
    public enum Side
    {
        None = 0x0,
        Front = 0x1,
        Back = 0x2,
        Left = 0x4,
        Right = 0x8,
        All = 0xF
    }

    public const uint ParrySID = 680; // common 'directional parry' status

    public readonly Dictionary<ulong, int> ActorStates = []; // value == active-side | (imminent-side << 4)
    public bool Active => ActorStates.Values.Any(s => ActiveSides(s) != Side.None);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var target = Actors.FirstOrDefault(w => w.InstanceID == actor.TargetID);
        if (target != null && ActorStates.TryGetValue(actor.TargetID, out var targetState))
        {
            var forbiddenSides = ActiveSides(targetState) | ImminentSides(targetState);
            var attackDir = (actor.Position - target.Position).Normalized();
            var facing = target.Rotation.ToDirection();
            var attackingFromForbidden = attackDir.Dot(facing) switch
            {
                > 0.7071067f => forbiddenSides.HasFlag(Side.Front),
                < -0.7071067f => forbiddenSides.HasFlag(Side.Back),
                _ => forbiddenSides.HasFlag(attackDir.Dot(facing.OrthoL()) > 0 ? Side.Left : Side.Right)
            };
            if (attackingFromForbidden)
                hints.Add("Attack target from unshielded side!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (id, targetState) in ActorStates)
        {
            var target = WorldState.Actors.Find(id);
            if (target == null)
                continue;

            void forbidDirection(Angle offset) => hints.AddForbiddenZone(new AOEShapeCone(100f, 45f.Degrees()), target.Position, target.Rotation + offset, DateTime.MaxValue, target.InstanceID);

            var forbiddenSides = ActiveSides(targetState);
            var attackDir = (actor.Position - target.Position).Normalized();
            var facing = target.Rotation.ToDirection();
            var attackingFromForbidden = attackDir.Dot(facing) switch
            {
                > 0.7071067f => forbiddenSides.HasFlag(Side.Front),
                < -0.7071067f => forbiddenSides.HasFlag(Side.Back),
                _ => forbiddenSides.HasFlag(attackDir.Dot(facing.OrthoL()) > 0 ? Side.Left : Side.Right)
            };

            if (attackingFromForbidden)
            {
                hints.SetPriority(target, AIHints.Enemy.PriorityForbidden);

                // make AI move to an area where it can attack target safely
                if (actor.TargetID == id)
                {
                    if (forbiddenSides.HasFlag(Side.Front))
                        forbidDirection(default);
                    if (forbiddenSides.HasFlag(Side.Left))
                        forbidDirection(90f.Degrees());
                    if (forbiddenSides.HasFlag(Side.Back))
                        forbidDirection(180f.Degrees());
                    if (forbiddenSides.HasFlag(Side.Right))
                        forbidDirection(270f.Degrees());
                }
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var a in ActiveActors)
        {
            if (ActorStates.TryGetValue(a.InstanceID, out var aState))
            {
                var active = ActiveSides(aState);
                var imminent = ImminentSides(aState);
                DrawParry(a, 0.Degrees(), active, imminent, Side.Front);
                DrawParry(a, 180.Degrees(), active, imminent, Side.Back);
                DrawParry(a, 90.Degrees(), active, imminent, Side.Left);
                DrawParry(a, 270.Degrees(), active, imminent, Side.Right);
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == ParrySID)
        {
            // TODO: front+back is 3, left+right is C, but I don't really know which is which, didn't see examples yet...
            // remove any predictions
            ActorStates[actor.InstanceID] = status.Extra & 0xF;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == ParrySID)
        {
            UpdateState(actor.InstanceID, ActorState(actor.InstanceID) & ~0xF);
        }
    }

    public void PredictParrySide(ulong instanceID, Side sides)
    {
        UpdateState(instanceID, ((int)sides << 4) | ActorState(instanceID) & 0xF);
    }

    private void DrawParry(Actor actor, Angle offset, Side active, Side imminent, Side check)
    {
        if (active.HasFlag(check))
            DrawParry(actor, offset, Colors.Enemy);
        else if (imminent.HasFlag(check))
            DrawParry(actor, offset);
    }

    private void DrawParry(Actor actor, Angle offset, uint color = 0)
    {
        var dir = actor.Rotation + offset;
        Arena.PathArcTo(actor.Position, 1.5f, (dir - 45.Degrees()).Rad, (dir + 45.Degrees()).Rad);
        MiniArena.PathStroke(false, color);
    }

    public int ActorState(ulong instanceID) => ActorStates.GetValueOrDefault(instanceID, 0);

    public void UpdateState(ulong instanceID, int state)
    {
        if (state == 0)
            ActorStates.Remove(instanceID);
        else
            ActorStates[instanceID] = state;
    }

    private static Side ActiveSides(int state) => (Side)(state & (int)Side.All);
    private static Side ImminentSides(int state) => ActiveSides(state >> 4);
}
