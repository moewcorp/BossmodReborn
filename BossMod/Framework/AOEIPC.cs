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
            default:
                return null;
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
