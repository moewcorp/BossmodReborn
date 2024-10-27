﻿using ImGuiNET;
using BossMod;

namespace UIDev;

class MiniArenaTest : TestWindow
{
    private readonly MiniArena _arena = new(new(), new(100, 100), new ArenaBoundsSquare(20));
    private bool _arenaIsCircle;
    private float _azimuth = -72;
    private float _altitude = 90;
    private bool _lineEnabled;
    private bool _coneEnabled = true;
    private bool _kbContourEnabled = true;
    private readonly List<Vector3> _shapeVertices = [];
    private Vector4 _lineEnds = new(90, 90, 110, 110);
    private Vector2 _playerPos = new(100, 90);
    private Vector2 _conePos = new(100, 80);
    private Vector2 _coneRadius = new(0, 100);
    private Vector2 _coneAngles = new(185, 161);
    private Vector2 _kbCenter = new(110, 100);
    private float _kbDistance = 5;

    public MiniArenaTest() : base("Arena test", new(400, 400), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse) { }

    public override void Draw()
    {
        ImGui.DragFloat("Camera azimuth", ref _azimuth, 1, -180, +180);
        ImGui.DragFloat("Camera altitude", ref _altitude, 1, -90, +90);
        if (ImGui.Checkbox("Circle shape", ref _arenaIsCircle))
        {
            _arena.Bounds = _arenaIsCircle ? new ArenaBoundsCircle(_arena.Bounds.Radius) : new ArenaBoundsSquare(_arena.Bounds.Radius);
        }

        _ = _arena.Begin(_azimuth.Degrees());
        if (_coneEnabled)
            _arena.ZoneCone(new(_conePos), _coneRadius.X, _coneRadius.Y, _coneAngles.X.Degrees(), _coneAngles.Y.Degrees(), Colors.Safe);
        _arena.Border(Colors.Border);
        if (_lineEnabled)
            _arena.AddLine(new(_lineEnds.X, _lineEnds.Y), new(_lineEnds.Z, _lineEnds.W), Colors.TextColor14);
        if (_kbContourEnabled)
        {
            foreach (var p in KBContour())
                _arena.PathLineTo(p);
            MiniArena.PathStroke(true, Colors.Vulnerable);
        }
        _arena.Actor(new(_playerPos), 0.Degrees(), Colors.PC);
        MiniArena.End();

        // arena config
        ImGui.DragFloat2("Player pos", ref _playerPos);
        ImGui.Checkbox("Draw cone", ref _coneEnabled);
        if (_coneEnabled)
        {
            ImGui.DragFloat2("Center", ref _conePos);
            ImGui.DragFloat2("Radius", ref _coneRadius, 1, 0);
            ImGui.DragFloat2("Angles", ref _coneAngles);
        }
        ImGui.Checkbox("Knockback contour", ref _kbContourEnabled);
        if (_kbContourEnabled)
        {
            ImGui.DragFloat2("KB center", ref _kbCenter);
            ImGui.DragFloat("KB distance", ref _kbDistance);
        }

        if (ImGui.TreeNode("Clipped shape"))
        {
            for (var i = 0; i < _shapeVertices.Count; ++i)
            {
                var v = _shapeVertices[i];
                if (ImGui.DragFloat3($"Vertex {i}", ref v))
                    _shapeVertices[i] = v;
                ImGui.SameLine();
                if (ImGui.Button($"Delete##{i}"))
                    _shapeVertices.RemoveAt(i--);
            }
            if (ImGui.Button("Add"))
                _shapeVertices.Add(new());
            ImGui.TreePop();
        }

        //var clippedShape = MiniArena.ClipPolygonToRect(_shapeVertices, _arena.WorldNW, _arena.WorldSE);
        //if (clippedShape.Count > 2)
        //{
        //    foreach (var v in clippedShape)
        //        _arena.PathLineTo(v);
        //    _arena.PathStroke(true, Colors.PlayerGeneric, 2);
        //}

        ImGui.Checkbox("Draw line", ref _lineEnabled);
        if (_lineEnabled)
            ImGui.DragFloat4("Endpoints", ref _lineEnds);
    }

    private IEnumerable<WPos> KBContour()
    {
        var cnt = 256;
        var coeff = 2 * MathF.PI / cnt;
        WPos kbCenter = new(_kbCenter);
        var centerOffset = kbCenter - _arena.Center;
        var c = centerOffset.LengthSq() - _arena.Bounds.Radius * _arena.Bounds.Radius;
        for (var i = 0; i < cnt; ++i)
        {
            var phi = (i * coeff).Radians();
            var dir = phi.ToDirection();
            var offDotDir = dir.Dot(centerOffset);
            var d = -offDotDir + MathF.Sqrt(offDotDir * offDotDir - c);
            yield return kbCenter + (d - _kbDistance) * dir;
        }
    }
}
