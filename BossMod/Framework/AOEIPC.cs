using BossMod.Components;
using System.Text.Json;

namespace BossMod;

// AOE data exposed over Dalamud IPC for external plugins (e.g. NyaDraw) to render native omen in the game world.
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
}

public sealed class AOEIPCDto
{
    // stable dedup key: hash of (actorID, activationTicks, shape type, params)
    public ulong Key { get; set; }
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
    public float OriginY { get; set; } // world-space height (usually player's Y), so native omen renders at correct elevation
    public float Rotation { get; set; } // final rotation in radians (aoe rotation + shape direction offset), game convention
    public double ActivationMs { get; set; } // relative milliseconds until activation
    public uint Color { get; set; } // ARGB
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

        var player = bossmod.WorldState.Party.Player();
        if (player == null)
        {
            return "[]";
        }

        var list = new List<AOEIPCDto>(16);
        var now = bossmod.WorldState.CurrentTime;
        // WPos carries no height; grab the local player's elevation from Dalamud's own object table
        // (BossMod's internal player actor is 2D and may not carry a meaningful Y).
        var defaultY = Service.ObjectTable.LocalPlayer?.Position.Y ?? 0f;
        var components = module.Components;
        var count = components.Count;
        for (var i = 0; i < count; ++i)
        {
            if (components[i] is not GenericAOEs aoes)
            {
                continue;
            }

            var active = aoes.ActiveAOEs(PartyState.PlayerSlot, player);
            foreach (ref readonly var aoe in active)
            {
                if (ConvertShape(aoe, now, defaultY) is { } dto)
                {
                    list.Add(dto);
                }
            }
        }

        return JsonSerializer.Serialize(list);
    }

    private static AOEIPCDto? ConvertShape(in GenericAOEs.AOEInstance aoe, DateTime now, float defaultY)
    {
        AOEIPCDto? dto = aoe.Shape switch
        {
            AOEShapeCircle c => new() { ShapeType = (int)AOEIPCShapeType.Circle, P1 = c.Radius },
            AOEShapeCone s => new() { ShapeType = (int)AOEIPCShapeType.Cone, P1 = s.Radius, P2 = s.HalfAngle.Rad },
            AOEShapeRect r => new() { ShapeType = (int)AOEIPCShapeType.Rect, P1 = r.LengthFront, P2 = r.HalfWidth, P3 = r.LengthBack },
            AOEShapeDonut d => new() { ShapeType = (int)AOEIPCShapeType.Donut, P1 = d.InnerRadius, P2 = d.OuterRadius },
            AOEShapeDonutSector s => new() { ShapeType = (int)AOEIPCShapeType.DonutSector, P1 = s.InnerRadius, P2 = s.OuterRadius, P3 = s.HalfAngle.Rad },
            AOEShapeCross c => new() { ShapeType = (int)AOEIPCShapeType.Cross, P1 = c.Length, P2 = c.HalfWidth },
            AOEShapeTriCone t => new() { ShapeType = (int)AOEIPCShapeType.TriCone, P1 = t.SideLength, P2 = t.HalfAngle.Rad },
            AOEShapeCapsule cap => new() { ShapeType = (int)AOEIPCShapeType.Capsule, P1 = cap.Radius, P2 = cap.Length },
            _ => null, // ArcCapsule / Custom etc: cannot be expressed by native omen
        };
        if (dto == null)
        {
            return null;
        }

        dto.OriginX = aoe.Origin.X;
        dto.OriginZ = aoe.Origin.Z;
        dto.OriginY = defaultY;
        dto.Rotation = (aoe.Rotation + DirectionOffsetOf(aoe.Shape)).Rad;
        dto.ActivationMs = (aoe.Activation - now).TotalMilliseconds;
        dto.Color = aoe.Color;
        dto.Key = (ulong)HashCode.Combine(aoe.ActorID, aoe.Activation.Ticks, dto.ShapeType, dto.P1, dto.P2, dto.P3);
        return dto;
    }

    private static Angle DirectionOffsetOf(AOEShape shape) => shape switch
    {
        AOEShapeCone s => s.DirectionOffset,
        AOEShapeRect r => r.DirectionOffset,
        AOEShapeDonutSector s => s.DirectionOffset,
        AOEShapeCross c => c.DirectionOffset,
        AOEShapeTriCone t => t.DirectionOffset,
        AOEShapeCapsule c => c.DirectionOffset,
        _ => default,
    };
}
