using System.IO;
using System.Text.Json;
using BossMod.Components;

namespace BossMod;

// AOE data exposed over Dalamud IPC for external plugins.
// Payload is a JSON string so both sides can deserialize independently without sharing types across assemblies.
public enum AOEIPCShapeType : byte
{
    Circle,
    Cone,
    Rect,
    Donut,
    DonutSector,
    Cross,
    TriCone,
    Capsule,
    Custom,
    Stack,   // ground circle on a stack target (friendly), P1=radius
    Spread,  // ground circle on a spread target (danger), P1=radius
    Tower,   // ground circle on a tower position (friendly), P1=radius
}

public sealed class AOEIPCDto
{
    public int ShapeType { get; set; }
    // per-shape parameters:
    //   Circle:      P1=radius
    //   Cone:        P1=radius, P2=halfAngle(rad)
    //   Rect:        P1=lengthFront, P2=halfWidth, P3=lengthBack
    //   Donut:       P1=innerRadius, P2=outerRadius
    //   DonutSector: P1=innerRadius, P2=outerRadius, P3=halfAngle(rad)
    //   Cross:       P1=armLength, P2=halfWidth
    //   TriCone:     P1=sideLength, P2=halfAngle(rad)
    //   Capsule:     P1=radius, P2=length
    public float P1 { get; set; }
    public float P2 { get; set; }
    public float P3 { get; set; }
    public float OriginX { get; set; }
    public float OriginZ { get; set; }
    public float OriginY { get; set; }
    public float Rotation { get; set; } // final rotation in radians, game convention
    public bool IsDanger { get; set; } // drawn with Colors.Danger (about to resolve) vs plain AOE color
}

public static class AOEIPC
{
    public static string CollectActiveAOEs(BossModuleManager bossmod)
    {
        var module = bossmod.ActiveModule;
        if (module == null)
        {
            return "[]";
        }

        var list = new List<AOEIPCDto>(16);
        var defaultY = Service.ObjectTable.LocalPlayer?.Position.Y ?? 0f;
        foreach (var zone in MiniArena.DrawnZones)
        {
            if (ConvertZone(zone, defaultY) is { } dto)
            {
                list.Add(dto);
            }
        }

        // stacks & spreads: GenericStackSpread components draw them as arena outlines, which never
        // enter DrawnZones (only filled danger zones do), so expose them explicitly here.
        foreach (var comp in module.Components)
        {
            if (comp is not GenericStackSpread ss)
                continue;
            foreach (var s in ss.Stacks)
            {
                if (!ss.IncludeDeadTargets && s.Target.IsDead)
                    continue;
                list.Add(new AOEIPCDto
                {
                    ShapeType = (int)AOEIPCShapeType.Stack,
                    OriginX = s.Target.Position.X,
                    OriginZ = s.Target.Position.Z,
                    OriginY = defaultY,
                    Rotation = 0,
                    P1 = s.Radius,
                    IsDanger = true,
                });
            }
            foreach (var sp in ss.Spreads)
            {
                if (!ss.IncludeDeadTargets && sp.Target.IsDead)
                    continue;
                list.Add(new AOEIPCDto
                {
                    ShapeType = (int)AOEIPCShapeType.Spread,
                    OriginX = sp.Target.Position.X,
                    OriginZ = sp.Target.Position.Z,
                    OriginY = defaultY,
                    Rotation = 0,
                    P1 = sp.Radius,
                    IsDanger = true,
                });
            }
        }

        // baits: GenericBaitAway also draws them as arena outlines; expose explicitly. ActiveBaits
        // already applies source-alive / target-dead filtering, mirrors what the arena draws.
        foreach (var comp in module.Components)
        {
            if (comp is not GenericBaitAway ba || ba.OnlyShowOutlines)
                continue;
            foreach (var b in ba.ActiveBaits)
            {
                var origin = (ba.CenterAtTarget ? b.Target : b.Source).Position + b.Offset;
                if (ConvertShape(b.Shape, origin, b.Rotation, defaultY) is { } dto)
                {
                    list.Add(dto);
                }
            }
        }

        // towers: GenericTowers also draws them as arena outlines; expose explicitly.
        foreach (var comp in module.Components)
        {
            if (comp is GenericTowers tw)
            {
                foreach (var t in tw.Towers)
                {
                    var radius = t.Shape is AOEShapeCircle c ? c.Radius : 4f;
                    list.Add(new AOEIPCDto
                    {
                        ShapeType = (int)AOEIPCShapeType.Tower,
                        OriginX = t.Position.X,
                        OriginZ = t.Position.Z,
                        OriginY = defaultY,
                        Rotation = 0,
                        P1 = radius,
                        IsDanger = true,
                    });
                }
            }
        }

        // shared tankbusters: GenericSharedTankbuster draws the shape only for specific roles
        // (background fill for non-tanks, outline for tanks), so tanks never see it in
        // DrawnZones; expose explicitly so every role gets the circle. Non-tank viewers already
        // drew the fill, skip those to avoid a doubled overlay.
        foreach (var comp in module.Components)
        {
            if (comp is not GenericSharedTankbuster st)
                continue;
            var src = SharedSrcField?.GetValue(st) as Actor;
            var tgt = SharedTgtField?.GetValue(st) as Actor;
            if (src == null || tgt == null)
                continue;
            var origin = st.OriginAtTarget ? tgt.Position : src.Position;
            var rot = st.OriginAtTarget ? default : Angle.FromDirection(tgt.Position - src.Position);
            var dto = ConvertShape(st.Shape, origin, rot, defaultY);
            if (dto == null)
                continue;
            if (MiniArena.DrawnZones.Any(z => z.Shape == dto.ShapeType
                && MathF.Abs(z.Origin.X - dto.OriginX) < 0.01f
                && MathF.Abs(z.Origin.Z - dto.OriginZ) < 0.01f
                && MathF.Abs(z.P1 - dto.P1) < 0.01f
                && MathF.Abs(z.P2 - dto.P2) < 0.01f))
                continue;
            list.Add(dto);
        }

        var sig = string.Join(",", list.Select(d => $"{(AOEIPCShapeType)d.ShapeType}@({d.OriginX:0},{d.OriginZ:0}){(d.IsDanger ? "*" : "")}"));
        if (list.Count != _lastSentCount || sig != _lastSentSig)
        {
            DebugLog($"[BossMod] zones {_lastSentCount} -> {list.Count} [{sig}]");
            _lastSentCount = list.Count;
            _lastSentSig = sig;
        }
        return JsonSerializer.Serialize(list);
    }

    private static int _lastSentCount = -1;
    private static string? _lastSentSig;

    // GenericSharedTankbuster keeps Source/Target protected; read them via cached reflection.
    private static readonly System.Reflection.FieldInfo? SharedSrcField = typeof(GenericSharedTankbuster).GetField("Source", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    private static readonly System.Reflection.FieldInfo? SharedTgtField = typeof(GenericSharedTankbuster).GetField("Target", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

    private static AOEIPCDto? ConvertZone(in MiniArena.DrawnZone zone, float defaultY)
    {
        var dto = new AOEIPCDto
        {
            ShapeType = zone.Shape,
            OriginX = zone.Origin.X,
            OriginZ = zone.Origin.Z,
            OriginY = defaultY,
            Rotation = zone.Rotation.Rad,
            IsDanger = zone.IsDanger,
        };
        switch ((AOEIPCShapeType)zone.Shape)
        {
            case AOEIPCShapeType.Circle:
                dto.P1 = zone.P1;
                break;
            case AOEIPCShapeType.Cone:
                dto.P1 = zone.P2;
                dto.P2 = zone.P3;
                break;
            case AOEIPCShapeType.Donut:
                dto.P1 = zone.P1;
                dto.P2 = zone.P2;
                break;
            case AOEIPCShapeType.DonutSector:
                dto.P1 = zone.P1;
                dto.P2 = zone.P2;
                dto.P3 = zone.P3;
                break;
            case AOEIPCShapeType.Rect:
                dto.P1 = zone.P1;
                dto.P2 = zone.P3;
                dto.P3 = zone.P2;
                break;
            case AOEIPCShapeType.Cross:
                dto.P1 = zone.P1;
                dto.P2 = zone.P2;
                break;
            case AOEIPCShapeType.Capsule:
                dto.P1 = zone.P1;
                dto.P2 = zone.P2;
                break;
            case AOEIPCShapeType.TriCone:
                dto.P1 = zone.P1;
                dto.P2 = zone.P2;
                break;
            default:
                return null;
        }
        return dto;
    }

    private static AOEIPCDto? ConvertShape(AOEShape shape, WPos origin, Angle rotation, float defaultY)
    {
        var dto = new AOEIPCDto
        {
            OriginX = origin.X,
            OriginZ = origin.Z,
            OriginY = defaultY,
            IsDanger = true,
        };
        switch (shape)
        {
            case AOEShapeCircle c:
                dto.ShapeType = (int)AOEIPCShapeType.Circle;
                dto.P1 = c.Radius;
                break;
            case AOEShapeCone c:
                dto.ShapeType = (int)AOEIPCShapeType.Cone;
                dto.P1 = c.Radius;
                dto.P2 = c.HalfAngle.Rad;
                dto.Rotation = (rotation + c.DirectionOffset).Rad;
                break;
            case AOEShapeDonutSector c:
                dto.ShapeType = (int)AOEIPCShapeType.DonutSector;
                dto.P1 = c.InnerRadius;
                dto.P2 = c.OuterRadius;
                dto.P3 = c.HalfAngle.Rad;
                dto.Rotation = (rotation + c.DirectionOffset).Rad;
                break;
            case AOEShapeDonut c:
                dto.ShapeType = (int)AOEIPCShapeType.Donut;
                dto.P1 = c.InnerRadius;
                dto.P2 = c.OuterRadius;
                break;
            case AOEShapeRect c:
                dto.ShapeType = (int)AOEIPCShapeType.Rect;
                dto.P1 = c.LengthFront;
                dto.P2 = c.HalfWidth;
                dto.P3 = c.LengthBack;
                dto.Rotation = (rotation + c.DirectionOffset).Rad;
                break;
            case AOEShapeCross c:
                dto.ShapeType = (int)AOEIPCShapeType.Cross;
                dto.P1 = c.Length;
                dto.P2 = c.HalfWidth;
                dto.Rotation = (rotation + c.DirectionOffset).Rad;
                break;
            case AOEShapeTriCone c:
                dto.ShapeType = (int)AOEIPCShapeType.TriCone;
                dto.P1 = c.SideLength;
                dto.P2 = c.HalfAngle.Rad;
                dto.Rotation = (rotation + c.DirectionOffset).Rad;
                break;
            case AOEShapeCapsule c:
                dto.ShapeType = (int)AOEIPCShapeType.Capsule;
                dto.P1 = c.Radius;
                dto.P2 = c.Length;
                dto.Rotation = (rotation + c.DirectionOffset).Rad;
                break;
            default:
                return null; // ArcCapsule / Custom: not expressible via native omen
        }
        return dto;
    }

    // standalone debug log for the BossMod<->NyaDraw AOE bridge; kept out of the game's main log so
    // it can be collected continuously while playing and handed over for diagnosis.
    private static string? _debugLogPath;
    private static long _debugLogSize;
    private static string DebugLogPath
    {
        get
        {
            if (_debugLogPath == null)
            {
                try
                {
                    _debugLogPath = Path.Combine(Service.PluginInterface.ConfigDirectory.FullName, "AOEIPCDebug.log");
                }
                catch
                {
                    _debugLogPath = Path.Combine(Path.GetTempPath(), "AOEIPCDebug.log");
                }
            }
            return _debugLogPath;
        }
    }

    private static void DebugLog(string msg)
    {
        try
        {
            var path = DebugLogPath;
            _debugLogSize += msg.Length + 2;
            if (_debugLogSize > 5 * 1024 * 1024)
            {
                File.WriteAllText(path, "");
                _debugLogSize = 0;
            }
            File.AppendAllText(path, $"{DateTime.Now:HH:mm:ss.fff} {msg}\n");
        }
        catch
        {
        }
    }
}
