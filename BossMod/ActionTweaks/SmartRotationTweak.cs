﻿namespace BossMod;

[ConfigDisplay(Name = "智能角色定位", Parent = typeof(ActionTweaksConfig), Order = -20)]
class SmartRotationConfig : ConfigNode
{
    [PropertyDisplay("启用该功能", tooltip: "用更智能的替代方案替换游戏中的“自动面向目标”选项。\n使用技能时，仅当目标不在正面锥体中时才改变方向。\n施法期间，保持角色面向目标。")]
    public bool Enabled = false;

    [PropertyDisplay("背对机制自动躲避")]
    public bool AvoidGazes = true;

    [PropertyDisplay("背对机制判定前开始回避的时间")]
    [PropertySlider(0, 10, Speed = 0.01f)]
    public float MinTimeToAvoid = 0.5f;
}

// Tweak to automatically rotate a character to avoid gazes and similar mechanics.
// - when gaze is expected and attack is initiated, if it's possible to rotate so that target is in frontal cone and avoid gaze, do so
// - when gaze is expected (with some configurable leeway) and would hit player with current facing, rotate away
// - when gaze is imminent (with some configurable short leeway) and it's not possible to hit target without being hit by a gaze, block casts and attacks
public sealed class SmartRotationTweak(WorldState ws, AIHints hints)
{
    private static readonly SmartRotationConfig _config = Service.Config.Get<SmartRotationConfig>();
    private readonly DisjointSegmentList _forbidden = new();
    private static readonly Angle _minWindow = 5f.Degrees();
    public bool Enabled => _config.Enabled;

    // return 'ideal orientation' for a spell, or null if spell is not oriented (self-targeted or does not require facing)
    public Angle? GetSpellOrientation(uint spellId, WPos playerPos, bool targetIsSelf, WPos? targetPos, WPos targetLoc)
    {
        var data = Service.LuminaRow<Lumina.Excel.Sheets.Action>(spellId);
        if (data == null || !data.Value.NeedToFaceTarget || data.Value.Range == 0) // does not require facing
            return null;
        if (data.Value.TargetArea)
            return Angle.FromDirection(targetLoc - playerPos);
        // see ActionManager.ResolveTarget
        targetIsSelf |= ActionDefinitions.Instance.SpellAllowedTargets(data.Value) == ActionTargets.Self;
        return targetIsSelf || targetPos == null ? null : Angle.FromDirection(targetPos.Value - playerPos); // self-targeted don't have ideal orientation
    }

    public Angle? GetSafeRotation(Angle currentDirection, Angle? preferredDirection, Angle preferredHalfWidth)
    {
        var aiEnabled = AI.AIManager.Instance?.Beh != null;
        if (!_config.Enabled && !aiEnabled)
            return null;

        var midpoint = preferredDirection ?? default; // center angles in forbidden list around this midpoint, to simplify preferred check later
        var currentOffset = (currentDirection - midpoint).Normalized();

        _forbidden.Clear();
        if (_config.AvoidGazes || aiEnabled)
        {
            var deadline = ws.FutureTime(_config.MinTimeToAvoid);
            foreach (var d in hints.ForbiddenDirections.Where(d => d.activation <= deadline))
            {
                var center = (d.center - midpoint).Normalized();
                var min = center - d.halfWidth;
                if (min.Rad < -MathF.PI)
                {
                    _forbidden.Add(min.Rad + Angle.DoublePI, MathF.PI);
                    min = -MathF.PI.Radians();
                }
                var max = center + d.halfWidth;
                if (max.Rad > MathF.PI)
                {
                    _forbidden.Add(-MathF.PI, max.Rad - Angle.DoublePI);
                    max = MathF.PI.Radians();
                }
                _forbidden.Add(min.Rad, max.Rad);
            }
        }

        if (preferredDirection != null && currentOffset.Abs().Rad >= preferredHalfWidth.Rad)
        {
            // current direction is bad, we want to rotate to preferred, if possible
            // note that midpoint is equal to preferred, and corresponds to 0 in forbidden list
            if (!_forbidden.Contains(0))
                return midpoint;
        }
        else
        {
            // current direction is ok, do nothing if it's safe
            if (!_forbidden.Contains(currentOffset.Rad))
                return null;
        }

        // ok, we need to rotate to safety
        // see if we can find a safe direction within preferred window
        static (float width, float mid) initBest(float min, float max) => (max - min, 0.5f * (min + max));
        static void updateBest(ref (float width, float mid) best, float min, float max)
        {
            var width = max - min;
            if (width > best.width)
            {
                best.width = width;
                best.mid = 0.5f * (min + max);
            }
        }

        if (preferredDirection != null)
        {
            var coneMin = -preferredHalfWidth.Rad;
            var coneMax = +preferredHalfWidth.Rad;
            var intersection = _forbidden.Intersect(coneMin, coneMax);
            if (intersection.count == 0)
                return midpoint; // entire frontal cone is safe, rotate to preferred

            // find widest safe range in a cone around preferred direction
            var best = initBest(coneMin, Math.Max(_forbidden[intersection.first].Min, coneMin));
            for (var i = 1; i < intersection.count; ++i)
                updateBest(ref best, _forbidden[intersection.first + i - 1].Max, _forbidden[intersection.first + i].Min);
            updateBest(ref best, Math.Min(_forbidden[intersection.first + intersection.count - 1].Max, coneMax), coneMax);

            if (best.width >= _minWindow.Rad)
                return midpoint + best.mid.Radians();
        }

        // find widest safe range in the whole circle
        {
            var best = initBest(_forbidden[^1].Max, _forbidden[0].Min + Angle.DoublePI);
            for (var i = 1; i < _forbidden.Count; ++i)
                updateBest(ref best, _forbidden[i - 1].Max, _forbidden[i].Min);
            return midpoint + best.mid.Radians();
        }
    }
}
