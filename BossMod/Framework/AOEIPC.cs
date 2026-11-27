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
    Tower,   // RESERVED - no longer sent (tower drawing was removed); value kept so later members keep their wire values
    FriendlyRect, // stack line (LineStack), P1=lenFront P2=halfWidth P3=lenBack
    Knockback, // knockback arrow on the local player: Origin=push start, Rotation=push direction, P1=distance
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
    public bool IsFriendly { get; set; } // drawn with Colors.SafeFromAOE (share/safe zone players must stand in)
    public int Batch { get; set; } // component index + 1 (0 = module-own drawing); same value for one mechanic batch
}

public static class AOEIPC
{
    public static string CollectActiveAOEs(BossModuleManager bossmod) => bossmod.GetExternalAOEs();

    internal static string CollectActiveAOEs(BossModule module, Actor pc)
    {
        var list = new List<AOEIPCDto>(16);
        var playerY = module.Arena.ExternalZoneHeight(module.ResolveArenaProjectionLayer(pc), pc.PosRot.Y);
        foreach (var zone in module.CaptureExternalZones(PartyState.PlayerSlot, pc))
        {
            if (ConvertZone(zone) is { } dto)
            {
                list.Add(dto);
            }
        }
        var capturedCount = list.Count;

        bool MatchesFloor(int? layer, bool? restrict) => module.ActorMatchesArenaProjectionLayer(pc, layer, restrict);
        float TargetHeight(Actor target, int? layer) => module.Arena.ExternalZoneHeight(layer, target.PosRot.Y);

        // stacks & spreads: GenericStackSpread components draw them as arena outlines, which never
        // enter DrawnZones (only filled danger zones do), so expose them explicitly here.
        var batch = 0;
        foreach (var comp in module.Components)
        {
            ++batch;
            if (comp is not GenericStackSpread ss)
                continue;
            foreach (var s in ss.Stacks)
            {
                var layer = s.ResolveArenaProjectionLayer(module);
                if ((!ss.IncludeDeadTargets && s.Target.IsDead) || !MatchesFloor(layer, s.RestrictToArenaProjectionLayer))
                    continue;
                list.Add(new AOEIPCDto
                {
                    ShapeType = (int)AOEIPCShapeType.Stack,
                    OriginX = s.Target.Position.X,
                    OriginZ = s.Target.Position.Z,
                    OriginY = TargetHeight(s.Target, layer),
                    Rotation = 0,
                    P1 = s.Radius,
                    IsDanger = true,
                    Batch = batch,
                });
            }
            foreach (var sp in ss.Spreads)
            {
                var layer = sp.ResolveArenaProjectionLayer(module);
                if ((!ss.IncludeDeadTargets && sp.Target.IsDead) || !MatchesFloor(layer, sp.RestrictToArenaProjectionLayer))
                    continue;
                list.Add(new AOEIPCDto
                {
                    ShapeType = (int)AOEIPCShapeType.Spread,
                    OriginX = sp.Target.Position.X,
                    OriginZ = sp.Target.Position.Z,
                    OriginY = TargetHeight(sp.Target, layer),
                    Rotation = 0,
                    P1 = sp.Radius,
                    IsDanger = true,
                    Batch = batch,
                });
            }
        }

        // baits: GenericBaitAway also draws them as arena outlines; expose explicitly. ActiveBaits
        // already applies source-alive / target-dead filtering, mirrors what the arena draws.
        // Outline-only components are included too so the IPC bridge matches the reflect bridge's
        // always-draw behavior (the native omen renders a filled approximation).
        batch = 0;
        foreach (var comp in module.Components)
        {
            ++batch;
            if (comp is not GenericBaitAway ba)
                continue;
            var isStack = comp is GenericBaitStack;
            foreach (var b in ba.ActiveBaits)
            {
                var layer = b.ResolveArenaProjectionLayer(module);
                if (!MatchesFloor(layer, b.RestrictToArenaProjectionLayer))
                    continue;
                var originActor = ba.CenterAtTarget ? b.Target : b.Source;
                if (ConvertShape(b.Shape, originActor.Position + b.Offset, b.Rotation, module.Arena.ExternalZoneHeight(layer, playerY)) is not { } dto)
                    continue;
                dto.Batch = batch;
                var capturedIndex = FindCapturedZone(list, capturedCount, dto);
                if (isStack && capturedIndex >= 0)
                {
                    list[capturedIndex].IsFriendly = true;
                    list[capturedIndex].IsDanger = true;
                }
                else if (isStack)
                {
                    // Preserve the established wire values for circle and line stacks.
                    dto.IsFriendly = true;
                    dto.ShapeType = (AOEIPCShapeType)dto.ShapeType switch
                    {
                        AOEIPCShapeType.Rect => (int)AOEIPCShapeType.FriendlyRect,
                        AOEIPCShapeType.Circle => (int)AOEIPCShapeType.Stack,
                        _ => dto.ShapeType,
                    };
                    list.Add(dto);
                }
                else if (capturedIndex < 0)
                    list.Add(dto);
            }
        }

        // towers (GenericTowers) are deliberately NOT exported any more: they are drawn as arena
        // outlines only and the native omen approximation was redundant. The AOEIPCShapeType.Tower
        // member stays reserved (it is a wire value - removing it would shift every later member),
        // and the NyaDraw receiver keeps its explicit no-op case so an older bridge that still sends
        // Tower dtos draws nothing either.

        // shared tankbusters: GenericSharedTankbuster draws the shape only for specific roles
        // (background fill for non-tanks, outline for tanks), so tanks never see it in
        // DrawnZones; expose explicitly so every role gets the circle. Non-tank viewers already
        // drew the fill, skip those to avoid a doubled overlay.
        batch = 0;
        foreach (var comp in module.Components)
        {
            ++batch;
            if (comp is not GenericSharedTankbuster st)
                continue;
            var src = SharedSrcField?.GetValue(st) as Actor;
            var tgt = SharedTgtField?.GetValue(st) as Actor;
            if (src == null || tgt == null)
                continue;
            var layer = module.ResolveTargetArenaProjectionLayer(tgt, st.ArenaProjectionLayer, st.RestrictToArenaProjectionLayer);
            if (!MatchesFloor(layer, st.RestrictToArenaProjectionLayer))
                continue;
            var origin = st.OriginAtTarget ? tgt : src;
            var rot = st.OriginAtTarget ? default : Angle.FromDirection(tgt.Position - src.Position);
            var dto = ConvertShape(st.Shape, origin.Position, rot, module.Arena.ExternalZoneHeight(layer, playerY));
            if (dto == null)
                continue;
            dto.Batch = batch;
            if (FindCapturedZone(list, capturedCount, dto) >= 0)
                continue;
            list.Add(dto);
        }

        // knockbacks: GenericKnockback renders them as world-projected lines (DrawArenaForeground ->
        // Arena.ActorProjected + AddLine), which never enter DrawnZones; expose the local player's own
        // push as an arrow instead. The movement comes from the component's CalculateMovements, so the
        // kind (away / towards / directional), the shape containment test, knockback immunity (Arm's
        // Length & friends) and the StopAtWall/StopAfterWall arena clamps are all inherited from
        // BossMod - no per-mechanic guessing here.
        batch = 0;
        foreach (var comp in module.Components)
        {
            ++batch;
            if (comp is not GenericKnockback kb)
                continue;
            var movements = kb.CalculateMovements(PartyState.PlayerSlot, pc);
            foreach (var (from, to) in movements)
            {
                var dir = to - from;
                var distance = dir.Length();
                if (distance < 0.1f)
                    continue;
                list.Add(new AOEIPCDto
                {
                    ShapeType = (int)AOEIPCShapeType.Knockback,
                    OriginX = from.X,
                    OriginZ = from.Z,
                    OriginY = playerY,
                    Rotation = Angle.FromDirection(dir).Rad,
                    P1 = distance,
                    Batch = batch,
                });
            }
        }

        list.RemoveAll(static dto => !float.IsFinite(dto.OriginX) || !float.IsFinite(dto.OriginY) || !float.IsFinite(dto.OriginZ)
            || !float.IsFinite(dto.Rotation) || !float.IsFinite(dto.P1) || !float.IsFinite(dto.P2) || !float.IsFinite(dto.P3));
        return JsonSerializer.Serialize(list);
    }

    private static int FindCapturedZone(List<AOEIPCDto> list, int count, AOEIPCDto dto)
    {
        for (var i = 0; i < count; ++i)
        {
            var zone = list[i];
            if (zone.Batch == dto.Batch && zone.ShapeType == dto.ShapeType
                && MathF.Abs(zone.OriginX - dto.OriginX) < 0.01f
                && MathF.Abs(zone.OriginY - dto.OriginY) < 0.01f
                && MathF.Abs(zone.OriginZ - dto.OriginZ) < 0.01f
                && MathF.Abs(zone.P1 - dto.P1) < 0.01f
                && MathF.Abs(zone.P2 - dto.P2) < 0.01f
                && MathF.Abs(zone.P3 - dto.P3) < 0.01f
                && MathF.Abs(MathF.IEEERemainder(zone.Rotation - dto.Rotation, MathF.Tau)) < 0.001f)
                return i;
        }
        return -1;
    }

    // GenericSharedTankbuster keeps Source/Target protected; read them via cached reflection.
    private static readonly System.Reflection.FieldInfo? SharedSrcField = typeof(GenericSharedTankbuster).GetField("Source", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    private static readonly System.Reflection.FieldInfo? SharedTgtField = typeof(GenericSharedTankbuster).GetField("Target", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

    private static AOEIPCDto? ConvertZone(in MiniArena.DrawnZone zone)
    {
        var dto = new AOEIPCDto
        {
            ShapeType = zone.Shape,
            OriginX = zone.Origin.X,
            OriginZ = zone.Origin.Z,
            OriginY = zone.OriginY,
            Rotation = zone.Rotation.Rad,
            IsDanger = zone.IsDanger,
            IsFriendly = zone.IsFriendly,
            Batch = zone.Batch,
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
}
