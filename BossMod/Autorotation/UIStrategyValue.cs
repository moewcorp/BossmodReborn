using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace BossMod.Autorotation;

public static class UIStrategyValue
{
    private static readonly (string Name, float Value)[] PriorityBaselines =
    [
        ("Very Low", ActionQueue.Priority.VeryLow),
        ("Low", ActionQueue.Priority.Low),
        ("Medium", ActionQueue.Priority.Medium),
        ("High", ActionQueue.Priority.High),
        ("Very High", ActionQueue.Priority.VeryHigh),
    ];

    public static List<string> Preview(StrategyValue value, StrategyConfigTrack cfg, BossModuleRegistry.Info? moduleInfo)
    {
        switch (value)
        {
            case StrategyValueTrack t:
                var opt = cfg.Options[t.Option];
                return [
                    $"Option: {opt.UIName}",
                    $"Comment: {value.Comment}",
                    $"Priority: {(float.IsNaN(t.PriorityOverride) ? $"default ({opt.DefaultPriority:f})" : t.PriorityOverride.ToString("f"))}",
                    $"Target: {PreviewTarget(t, moduleInfo)}"
                ];
            default:
                return [];
        }
    }

    public static string PreviewTarget(StrategyValueTrack value, BossModuleRegistry.Info? moduleInfo)
    {
        var targetDetails = value.Target switch
        {
            StrategyTarget.PartyByAssignment => ((PartyRolesConfig.Assignment)value.TargetParam).ToString(),
            StrategyTarget.PartyWithLowestHP => PreviewParam((StrategyPartyFiltering)value.TargetParam),
            StrategyTarget.EnemyWithHighestPriority => $"{(StrategyEnemySelection)value.TargetParam}",
            StrategyTarget.EnemyByOID => $"{(moduleInfo?.ObjectIDType != null ? GeneratedEnumMetadata.ValueByRaw(moduleInfo.ObjectIDType, (uint)value.TargetParam).ToString() : "???")} (0x{value.TargetParam:X})",
            StrategyTarget.PointWaymark => $"{(Waymark)value.TargetParam}",
            _ => ""
        };
        var offsetDetails = value.Target == StrategyTarget.PointAbsolute ? $" {value.Offset1}x{value.Offset2}" : value.Offset1 != 0 ? $" + R{value.Offset1}, dir={value.Offset2}" : "";
        return (targetDetails.Length > 0 ? $"{value.Target} ({targetDetails})" : $"{value.Target}") + offsetDetails;
    }

    public static bool DrawEditor(StrategyValue value, StrategyConfigTrack cfg, BossModuleRegistry.Info? moduleInfo, int? level)
    {
        var modified = false;
        if (value is StrategyValueTrack tr)
        {
            modified |= DrawEditorTrackOption(tr, cfg, level);
            modified |= ImGui.InputText("备注", ref value.Comment, 512);
            modified |= DrawEditorPriority(tr);
            modified |= DrawEditorTarget(tr, cfg.Options[tr.Option].SupportedTargets, moduleInfo);
        }
        return modified;
    }

    public static bool DrawEditorTrackOption(StrategyValueTrack value, StrategyConfigTrack cfg, int? level, string label = "Option")
    {
        var modified = false;
        using (var combo = ImRaii.Combo(label, cfg.Options[value.Option].UIName))
        {
            if (combo)
            {
                for (var i = 0; i < cfg.Options.Count; ++i)
                {
                    var opt = cfg.Options[i];
                    if (level < opt.MinLevel || level > opt.MaxLevel)
                        continue; // filter out options outside our level

                    if (ImGui.Selectable(cfg.Options[i].UIName, i == value.Option))
                    {
                        modified = true;
                        value.Option = i;
                    }
                }
            }
        }
        return modified;
    }

    public static bool DrawEditorPriority(StrategyValueTrack value)
    {
        var modified = false;
        var overridePriority = !float.IsNaN(value.PriorityOverride);
        if (ImGui.Checkbox("覆盖优先级", ref overridePriority))
        {
            modified = true;
            value.PriorityOverride = overridePriority ? ActionQueue.Priority.Low : float.NaN;
        }
        ImGui.SameLine();
        UIMisc.HelpMarker("""
            为对应动作定义自定义优先级。
            优先级会与其他候选动作进行比较；建议使用预定义基准并添加小偏移量来区分多个动作。
            基准优先级如下：
            * 非常低 (1000) - 仅在没有其他可用的动作时使用该动作。
            * 低 (2000) - 仅当不会延迟任何输出动作时使用（例如，在没有溢出风险时，可能会延迟使用第二次充能）。
            * 中 (3000) - 会在下一个可能的即刻技能槽位使用，但不会延迟 GCD 或任何极其重要的即刻技能；每个 GCD 至少可期望有 1 个中优先级动作槽位。
            * 高 (4000) - 会在下一个可能的即刻技能槽位使用；不会延迟 GCD，但如果使用不当可能在某些情况下打乱循环。
            * 非常高 (5000) - 会尽快使用；必要时会延迟 GCD。
            """);

        if (overridePriority)
        {
            var priority = value.PriorityOverride;
            var upperBound = Array.FindIndex(PriorityBaselines, b => b.Value > priority);
            var baselineIndex = upperBound switch
            {
                -1 => PriorityBaselines.Length - 1,
                0 => 0,
                _ => upperBound - 1
            };
            var priorityDelta = value.PriorityOverride - PriorityBaselines[baselineIndex].Value;

            using var indent = ImRaii.PushIndent();
            ImGui.SetNextItemWidth(100);
            using (var combo = ImRaii.Combo("###baseline", PriorityBaselines[baselineIndex].Name))
            {
                if (combo)
                {
                    for (var i = 0; i < PriorityBaselines.Length; ++i)
                    {
                        if (ImGui.Selectable(PriorityBaselines[i].Name, i == baselineIndex))
                        {
                            modified = true;
                            value.PriorityOverride = PriorityBaselines[i].Value + priorityDelta;
                        }
                    }
                }
            }
            ImGui.SameLine();
            ImGui.TextUnformatted("+");
            ImGui.SameLine();
            if (ImGui.InputFloat("###delta", ref priorityDelta))
            {
                modified = true;
                value.PriorityOverride = PriorityBaselines[baselineIndex].Value + priorityDelta;
            }
        }

        return modified;
    }

    public static bool DrawEditorTarget(StrategyValueTrack value, ActionTargets supportedTargets, BossModuleRegistry.Info? moduleInfo)
    {
        var modified = false;
        using (var combo = ImRaii.Combo("Target", value.Target.ToString()))
        {
            if (combo)
            {
                for (var i = StrategyTarget.Automatic; i < StrategyTarget.Count; ++i)
                {
                    if (AllowTarget(i, supportedTargets, moduleInfo) && ImGui.Selectable(i.ToString(), i == value.Target))
                    {
                        value.Target = i;
                        value.TargetParam = 0;
                        modified = true;
                    }
                }
            }
        }

        using var indent = ImRaii.PushIndent();
        switch (value.Target)
        {
            case StrategyTarget.PartyByAssignment:
                modified |= DrawEditorTargetParamCombo<PartyRolesConfig.Assignment>(ref value.TargetParam, "Assignment");
                break;
            case StrategyTarget.PartyWithLowestHP:
                if (supportedTargets.HasFlag(ActionTargets.Self))
                    modified |= DrawEditorTargetParamFlags(ref value.TargetParam, StrategyPartyFiltering.IncludeSelf, "Allow self", false);
                modified |= DrawEditorTargetParamFlags(ref value.TargetParam, StrategyPartyFiltering.ExcludeTanks, "Allow tanks", true);
                modified |= DrawEditorTargetParamFlags(ref value.TargetParam, StrategyPartyFiltering.ExcludeHealers, "Allow healers", true);
                modified |= DrawEditorTargetParamFlags(ref value.TargetParam, StrategyPartyFiltering.ExcludeMelee, "Allow melee", true);
                modified |= DrawEditorTargetParamFlags(ref value.TargetParam, StrategyPartyFiltering.ExcludeRanged, "Allow ranged", true);
                modified |= DrawEditorTargetParamFlags(ref value.TargetParam, StrategyPartyFiltering.ExcludeNoPredictedDamage, "Only if more damage is expected", false);
                break;
            case StrategyTarget.EnemyWithHighestPriority:
                modified |= DrawEditorTargetParamCombo<StrategyEnemySelection>(ref value.TargetParam, "Criterion");
                break;
            case StrategyTarget.EnemyByOID:
                if (moduleInfo?.ObjectIDType != null)
                {
                    var v = GeneratedEnumMetadata.ValueByRaw(moduleInfo.ObjectIDType, (uint)value.TargetParam);
                    if (UICombo.Enum("OID", ref v))
                    {
                        value.TargetParam = (int)(uint)(object)v;
                        modified = true;
                    }
                }
                break;
            case StrategyTarget.PointWaymark:
                var wm = (Waymark)value.TargetParam;
                if (UICombo.Enum("Waymark", ref wm))
                {
                    value.TargetParam = (int)wm;
                    modified = true;
                }
                break;
        }

        if (supportedTargets.HasFlag(ActionTargets.Area))
        {
            if (value.Target == StrategyTarget.PointAbsolute)
            {
                modified |= ImGui.InputFloat("X", ref value.Offset1);
                modified |= ImGui.InputFloat("Z", ref value.Offset2);
            }
            else
            {
                modified |= ImGui.DragFloat("偏移", ref value.Offset1, 0.1f, 0, 30);
                modified |= ImGui.DragFloat("方向", ref value.Offset2, 1, -180, 180);
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip($"单位为度；0 为正南，逆时针增加（90 为正东，180 为正北，-90 为正西）");
            }
        }

        return modified;
    }

    public static bool AllowTarget(StrategyTarget t, ActionTargets supported, BossModuleRegistry.Info? moduleInfo) => supported.HasFlag(ActionTargets.Area) || t switch
    {
        StrategyTarget.Self => supported.HasFlag(ActionTargets.Self),
        StrategyTarget.PartyByAssignment => supported.HasFlag(ActionTargets.Party),
        StrategyTarget.PartyWithLowestHP => supported.HasFlag(ActionTargets.Party),
        StrategyTarget.EnemyWithHighestPriority => supported.HasFlag(ActionTargets.Hostile),
        StrategyTarget.EnemyByOID => supported.HasFlag(ActionTargets.Hostile) && moduleInfo != null,
        StrategyTarget.PointAbsolute or StrategyTarget.PointCenter or StrategyTarget.PointWaymark => false,
        _ => true
    };

    private static string PreviewParam(StrategyPartyFiltering pf)
    {
        string excludeIfSet(StrategyPartyFiltering flag, string value) => pf.HasFlag(flag) ? $", exclude {value}" : "";
        return $"{(pf.HasFlag(StrategyPartyFiltering.IncludeSelf) ? "include" : "exclude")} self"
            + excludeIfSet(StrategyPartyFiltering.ExcludeTanks, "tanks")
            + excludeIfSet(StrategyPartyFiltering.ExcludeHealers, "healers")
            + excludeIfSet(StrategyPartyFiltering.ExcludeMelee, "melee")
            + excludeIfSet(StrategyPartyFiltering.ExcludeRanged, "ranged")
            + excludeIfSet(StrategyPartyFiltering.ExcludeNoPredictedDamage, "players not expecting damage");
    }

    private static bool DrawEditorTargetParamCombo<E>(ref int current, string text) where E : Enum
    {
        var value = (E)(object)current;
        if (!UICombo.Enum(text, ref value))
            return false;
        current = (int)(object)value;
        return true;
    }

    private static bool DrawEditorTargetParamFlags(ref int current, StrategyPartyFiltering flag, string text, bool inverted)
    {
        var isChecked = ((StrategyPartyFiltering)current).HasFlag(flag) != inverted;
        if (!ImGui.Checkbox(text, ref isChecked))
            return false;
        current ^= (int)flag;
        return true;
    }
}

[AttributeUsage(AttributeTargets.Enum)]
public sealed class RendererAttribute(Type type) : Attribute
{
    public Type Type => type;
}

public class RendererFactory
{
    private static RendererFactory? _instance;
    private readonly Dictionary<Type, IStrategyRenderer> _dict = [];

    public static bool Draw(StrategyContext context, StrategyConfig config, ref StrategyValue value)
    {
        var inst = (_instance ??= new()).Get(config.Renderer);

        ImGui.TableNextRow();
        using var _ = ImRaii.PushId(config.InternalName);
        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        inst.DrawLabel(context, config);
        ImGui.TableNextColumn();
        return inst.DrawValue(context, config, ref value);
    }

    private IStrategyRenderer Get(Type t) => _dict.TryGetValue(t, out var r) ? r : (_dict[t] = GeneratedFactories.CreateStrategyRenderer(t));
}

public interface IStrategyRenderer
{
    public void DrawLabel(StrategyContext context, StrategyConfig config);
    public bool DrawValue(StrategyContext context, StrategyConfig config, ref StrategyValue value);
}

public class TrackRenderer : IStrategyRenderer
{
    public virtual void DrawLabel(StrategyContext context, StrategyConfig config) => ImGui.TextWrapped(config.UIName);
    public bool DrawValue(StrategyContext context, StrategyConfig config, ref StrategyValue value)
    {
        var v = (StrategyValueTrack)value;
        if (DrawValue(context, (StrategyConfigTrack)config, ref v))
        {
            value = v;
            return true;
        }
        return false;
    }

    public virtual bool DrawValue(StrategyContext context, StrategyConfigTrack config, ref StrategyValueTrack value)
    {
        string print(int ix) => config.Options[ix].DisplayName.Length > 0
            ? config.Options[ix].DisplayName
            : UICombo.EnumString((Enum)GeneratedEnumMetadata.Values(config.OptionEnum).GetValue(ix)!);
        bool filter(int ix) => (config.Options[ix].Context & context) != StrategyContext.None;

        return UICombo.EnumIndex(
            "",
            config.OptionEnum,
            ref value.Option,
            print,
            filter
        );
    }
}

public class FloatRenderer : IStrategyRenderer
{
    public void DrawLabel(StrategyContext context, StrategyConfig config) => ImGui.TextWrapped(config.UIName);
    public bool DrawValue(StrategyContext context, StrategyConfig config, ref StrategyValue value)
    {
        var cfg = (StrategyConfigFloat)config;
        var f = ((StrategyValueFloat)value).Value;
        ImGui.SetNextItemWidth(200 * ImGuiHelpers.GlobalScale);
        if (cfg.Drag)
        {
            if (ImGui.SliderFloat("", ref f, cfg.MinValue, cfg.MaxValue))
            {
                value = new StrategyValueFloat() { Value = f };
                return true;
            }
        }
        else
        {
            if (ImGui.InputFloat("", ref f, cfg.Speed))
            {
                value = new StrategyValueFloat() { Value = f };
                return true;
            }
        }

        return false;
    }
}

public class IntRenderer : IStrategyRenderer
{
    public void DrawLabel(StrategyContext context, StrategyConfig config) => ImGui.TextWrapped(config.UIName);
    public bool DrawValue(StrategyContext context, StrategyConfig config, ref StrategyValue value)
    {
        var cfg = (StrategyConfigInt)config;
        var f = ((StrategyValueInt)value).Value;
        ImGui.SetNextItemWidth(200 * ImGuiHelpers.GlobalScale);
        if (cfg.Drag)
        {
            if (ImGui.SliderLong("", ref f, cfg.MinValue, cfg.MaxValue))
            {
                value = new StrategyValueInt() { Value = f };
                return true;
            }
        }
        else
        {
            if (ImGui.InputLong("", ref f, (long)cfg.Speed))
            {
                value = new StrategyValueInt() { Value = f };
                return true;
            }
        }

        return false;
    }
}

public class FakeFloatRenderer : TrackRenderer
{
    public override bool DrawValue(StrategyContext context, StrategyConfigTrack config, ref StrategyValueTrack value)
    {
        var cur = (value.Option + 10) / 10f;
        var isOnHitbox = value.Option == 0;
        var modified = false;
        using (ImRaii.Disabled(isOnHitbox))
        {
            if (ImGui.SliderFloat("", ref cur, 1.1f, 30, "%.1f"))
            {
                value.Option = (int)(cur * 10f - 10f);
                modified = true;
            }
        }
        if (ImGui.Checkbox("保持在命中体积边缘", ref isOnHitbox))
        {
            value.Option = isOnHitbox ? 0 : 1;
            modified = true;
        }
        return modified;
    }
}
