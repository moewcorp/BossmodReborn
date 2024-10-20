﻿using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace BossMod.AI;

// utility for simulating user actions based on AI decisions:
// - navigation
// - using actions safely (without spamming, not in cutscenes, etc)
sealed class AIController(WorldState ws, ActionManagerEx amex, MovementOverride movement)
{
    public WPos? NaviTargetPos;
    public float? NaviTargetVertical;
    public bool AllowInterruptingCastByMovement;
    public bool ForceCancelCast;

    private readonly ActionManagerEx _amex = amex;
    private readonly MovementOverride _movement = movement;
    private DateTime _nextInteract;

    public bool IsVerticalAllowed => Service.Condition[ConditionFlag.InFlight];
    public Angle CameraFacing => (Camera.Instance?.CameraAzimuth ?? 0).Radians() + 180.Degrees();
    public Angle CameraAltitude => (Camera.Instance?.CameraAltitude ?? 0).Radians();

    public void Clear()
    {
        NaviTargetPos = null;
        NaviTargetVertical = null;
        AllowInterruptingCastByMovement = false;
        ForceCancelCast = false;
    }

    public void SetFocusTarget(Actor? actor)
    {
        if (Service.TargetManager.FocusTarget?.EntityId != actor?.InstanceID)
            Service.TargetManager.FocusTarget = actor != null ? Service.ObjectTable.SearchById((uint)actor.InstanceID) : null;
    }

    public void Update(Actor? player, AIHints hints, DateTime now)
    {
        if (player == null || player.IsDead || ws.Party.Members[PartyState.PlayerSlot].InCutscene)
        {
            return;
        }

        Vector3? desiredPosition = null;

        // TODO this checks whether movement keys are pressed, we need a better solution
        var moveRequested = _movement.IsMoveRequested();
        var castInProgress = player.CastInfo != null && !player.CastInfo.EventHappened;
        var forbidMovement = moveRequested || !AllowInterruptingCastByMovement && _amex.MoveMightInterruptCast;
        if (NaviTargetPos != null && !forbidMovement && (NaviTargetPos.Value - player.Position).LengthSq() > 0.01f)
        {
            var y = NaviTargetVertical != null && IsVerticalAllowed ? NaviTargetVertical.Value : player.PosRot.Y;
            desiredPosition = new(NaviTargetPos.Value.X, y, NaviTargetPos.Value.Z);
        }
        else
        {
            _amex.ForceCancelCastNextFrame |= ForceCancelCast && castInProgress;
        }

        if (hints.ForcedMovement == null && desiredPosition != null)
            hints.ForcedMovement = desiredPosition.Value - player.PosRot.XYZ();

        if (hints.InteractWithTarget is Actor tar && player.DistanceToHitbox(tar) <= 3)
            ExecuteInteract(now, tar);
    }

    private unsafe void ExecuteInteract(DateTime now, Actor target)
    {
        if (_amex.EffectiveAnimationLock > 0 || now < _nextInteract)
            return;
        var obj = GameObjectManager.Instance()->Objects.IndexSorted[target.SpawnIndex].Value;
        if (obj == null || obj->GetGameObjectId() != target.InstanceID)
            return;
        TargetSystem.Instance()->OpenObjectInteraction(obj);
        _nextInteract = now.AddMilliseconds(100);
    }
}
