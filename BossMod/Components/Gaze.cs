﻿using ImGuiNET;

namespace BossMod.Components;

// generic gaze/weakpoint component, allows customized 'eye' position
public abstract class GenericGaze(BossModule module, ActionID aid = new(), bool inverted = false) : CastCounter(module, aid)
{
    public record struct Eye(
        WPos Position,
        DateTime Activation = new(),
        Angle Forward = new(), // if non-zero, treat specified side as 'forward' for hit calculations
        float Range = 10000,
        ulong? ActorID = null);

    public bool Inverted = inverted; // if inverted, player should face eyes instead of averting

    private const float _eyeOuterH = 10, _eyeOuterV = 6, _eyeInnerR = 4;
    private const float _eyeOuterR = (_eyeOuterH * _eyeOuterH + _eyeOuterV * _eyeOuterV) / (2 * _eyeOuterV);
    private const float _eyeOffsetV = _eyeOuterR - _eyeOuterV;

    private static readonly float _eyeHalfAngle = (float)Math.Asin(_eyeOuterH / _eyeOuterR);
    private static readonly Vector2 offset = new(0, _eyeOffsetV);
    private static readonly float halfPIHalfAngleP = Angle.HalfPi + _eyeHalfAngle;
    private static readonly float halfPIHalfAngleM = Angle.HalfPi - _eyeHalfAngle;

    public abstract ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var eyes = ActiveEyes(slot, actor);
        var len = eyes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var eye = ref eyes[i];
            if (actor.Position.InCircle(eye.Position, eye.Range) && HitByEye(ref actor, eye) != Inverted)
            {
                hints.Add(Inverted ? "Face the eye!" : "Turn away from gaze!");
                break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var eyes = ActiveEyes(slot, actor);
        var len = eyes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var eye = ref eyes[i];
            if (actor.Position.InCircle(eye.Position, eye.Range))
            {
                var direction = Inverted ? Angle.FromDirection(actor.Position - eye.Position) - eye.Forward
                    : Angle.FromDirection(eye.Position - actor.Position) - eye.Forward;

                var angle = Inverted ? 135f.Degrees() : 45f.Degrees();
                hints.ForbiddenDirections.Add((direction, angle, eye.Activation));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var eyes = ActiveEyes(pcSlot, pc);
        var len = eyes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var eye = ref eyes[i];
            var danger = HitByEye(ref pc, eye) != Inverted;
            var eyeCenter = IndicatorScreenPos(eye.Position);
            DrawEye(eyeCenter, danger);

            if (pc.Position.InCircle(eye.Position, eye.Range))
            {
                var (min, max) = Inverted ? (45f, 315f) : (-45f, 45f);
                Arena.PathArcTo(pc.Position, 1, (pc.Rotation + eye.Forward + min.Degrees()).Rad, (pc.Rotation + eye.Forward + max.Degrees()).Rad);
                MiniArena.PathStroke(false, Colors.Enemy);
            }
        }
    }

    public static void DrawEye(Vector2 eyeCenter, bool danger)
    {
        var dl = ImGui.GetWindowDrawList();
        dl.PathArcTo(eyeCenter - offset, _eyeOuterR, halfPIHalfAngleP, halfPIHalfAngleM);
        dl.PathArcTo(eyeCenter + offset, _eyeOuterR, -halfPIHalfAngleP, -halfPIHalfAngleM);
        dl.PathFillConvex(danger ? Colors.Enemy : Colors.PC);
        dl.AddCircleFilled(eyeCenter, _eyeInnerR, Colors.Border);
    }

    public static bool HitByEye(ref Actor actor, Eye eye) => (actor.Rotation + eye.Forward).ToDirection().Dot((eye.Position - actor.Position).Normalized()) >= 0.707107f; // 45-degree

    private Vector2 IndicatorScreenPos(WPos eye)
    {
        if (Arena.InBounds(eye) || Arena.Bounds is not ArenaBoundsCircle && Arena.Bounds is ArenaBoundsComplex circle && !circle.IsCircle)
        {
            return Arena.WorldPositionToScreenPosition(eye);
        }
        else
        {
            var dir = (eye - Arena.Center).Normalized();
            return Arena.ScreenCenter + Arena.RotatedCoords(dir.ToVec2()) * (Arena.ScreenHalfSize + Arena.ScreenMarginSize * 0.5f);
        }
    }
}

// gaze that happens on cast end
public class CastGaze(BossModule module, ActionID aid, bool inverted = false, float range = 10000, int maxCasts = int.MaxValue) : GenericGaze(module, aid, inverted)
{
    public readonly List<Eye> Eyes = [];
    public int MaxCasts = maxCasts; // used for staggered gazes, when showing all active would be pointless

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        var count = Eyes.Count;
        if (count == 0)
            return [];

        var max = count > MaxCasts ? MaxCasts : count;

        var eyes = new Eye[max];
        for (var i = 0; i < max; ++i)
        {
            var eye = Eyes[i];
            eyes[i] = eye;
        }
        return eyes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Eyes.Add(new(spell.LocXZ, Module.CastFinishAt(spell), default, range, caster.InstanceID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var count = Eyes.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (Eyes[i].ActorID == id)
                {
                    Eyes.RemoveAt(i);
                    break;
                }
            }
        }
    }
}

// cast weakpoint component: a number of casts (with supposedly non-intersecting shapes), player should face specific side determined by active status to the caster for aoe he's in
public class CastWeakpoint(BossModule module, ActionID aid, AOEShape shape, uint statusForward, uint statusBackward, uint statusLeft, uint statusRight) : GenericGaze(module, aid, true)
{
    public AOEShape Shape = shape;
    public readonly uint[] Statuses = [statusForward, statusLeft, statusBackward, statusRight]; // 4 elements: fwd, left, back, right
    private readonly List<Actor> _casters = [];
    private readonly Dictionary<ulong, Angle> _playerWeakpoints = [];

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        var count = _casters.Count;
        if (count == 0)
            return [];
        Actor? caster = null;
        var minRemainingTime = float.MaxValue;
        // if there are multiple casters, take one that finishes first
        for (var i = 0; i < count; ++i)
        {
            var a = _casters[i];
            if (Shape.Check(actor.Position, a.Position, a.CastInfo!.Rotation))
            {
                if (a.CastInfo!.RemainingTime < minRemainingTime)
                {
                    caster = a;
                    minRemainingTime = a.CastInfo.RemainingTime;
                }
            }
        }

        if (caster != null && _playerWeakpoints.TryGetValue(actor.InstanceID, out var angle))
            return new Eye[1] { new(caster.Position, Module.CastFinishAt(caster.CastInfo), angle) };
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Remove(caster);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var statusKind = Array.IndexOf(Statuses, status.ID);
        if (statusKind >= 0)
            _playerWeakpoints[actor.InstanceID] = statusKind * 90.Degrees();
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        var statusKind = Array.IndexOf(Statuses, status.ID);
        if (statusKind >= 0)
            _playerWeakpoints.Remove(actor.InstanceID);
    }
}
