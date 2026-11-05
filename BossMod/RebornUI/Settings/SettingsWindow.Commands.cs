using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using RebornMaterial;

namespace BossMod.RebornUI;

internal sealed partial class SettingsWindow
{
    private static readonly (string, string)[] _availableAICommands =
    [
        ( "on", "启用 AI。" ),
        ( "off", "禁用 AI。" ),
        ( "toggle", "切换 AI 开关。" ),
        ( "targetmaster", "切换聚焦跟随对象。" ),
        ( "follow slotX", "跟随指定队伍位置，例如 Slot1。" ),
        ( "follow name", "按名称跟随指定队员。" ),
        ( "ui", "切换 AI 菜单。" ),
        ( "forbidactions", "切换禁止技能执行（仅自动循环）。" ),
        ( "forbidactions on/off", "设置禁止技能执行（仅自动循环）。" ),
        ( "forbidmovement", "切换禁止移动。" ),
        ( "forbidmovement on/off", "设置禁止移动。" ),
        ( "idlewhilemounted", "切换骑乘时待机。" ),
        ( "idlewhilemounted on/off", "设置骑乘时待机。" ),
        ( "followcombat", "切换战斗中跟随。" ),
        ( "followcombat on/off", "设置战斗中跟随。" ),
        ( "followmodule", "切换战斗模块激活时跟随。" ),
        ( "followmodule on/off", "设置战斗模块激活时跟随。" ),
        ( "followoutofcombat", "切换脱战时跟随。" ),
        ( "followoutofcombat on/off", "设置脱战时跟随。" ),
        ( "followtarget", "切换战斗中跟随目标。" ),
        ( "followtarget on/off", "设置战斗中跟随目标。" ),
        ( "positional X", "设置跟随目标时的身位（any、rear、flank、front）。" ),
        ( "maxdistancetarget X", "设置与目标的最大距离（默认 2.6）。" ),
        ( "maxdistanceslot X", "设置与队伍位置的最大距离（默认 1）。" ),
        ( "mindistance X", "设置与碰撞体的最小距离（默认 0）。" ),
        ( "prefdistance X", "设置与危险区域的期望距离（默认 0）。" ),
        ( "movedelay X", "设置 AI 移动决策延迟（默认 0）。" ),
        ( "obstaclemaps", "切换障碍地图加载。" ),
        ( "obstaclemaps on/off", "设置障碍地图加载。" ),
        ( "setpresetname X", "设置 AI 自动循环预设，例如 setpresetname vbm default。" )
    ];

    private static readonly (string, string)[] _autorotationCommands =
    [
        ( "ar clear", "清除当前预设；没有启用规划时自动循环不会执行技能" ),
        ( "ar disable", "强制禁用自动循环，即使已启用规划也不会自动执行技能。" ),
        ( "ar set Preset", "开始执行指定预设。" ),
        ( "ar toggle", "强制禁用自动循环；若已禁用则清除覆盖设置。" ),
        ( "ar toggle Preset", "执行指定预设；若已启用则清除预设" ),
        ( "ar ui", "切换自动循环界面。" ),
    ];

    private static readonly (string, string)[] _availableOtherCommands =
    [
        ( "", "打开此窗口。" ),
        ( "restorerotation", "切换使用技能后恢复角色朝向。" ),
        ( "resetcolors", "将所有颜色恢复默认值。" ),
        ( "d", "打开调试菜单。" ),
        ( "r", "打开回放菜单。" ),
        ( "r on/off", "开始或停止录制回放。" ),
        ( "gc", "触发垃圾回收。" ),
        ( "radar", "切换雷达显示。" ),
        ( "radar on/off", "开启或关闭雷达显示。" ),
        ( "radar reset", "将雷达窗口移回屏幕中央。" ),
        ( "cfg", "列出所有配置；使用 /bmr cfg <config> <field> <value> 修改设置。" )
    ];

    private static void DrawCommands()
    {
        DrawCommandCard("cmd_general", "常规", FontAwesomeIcon.Terminal, "/bmr", _availableOtherCommands, 0);
        DrawCommandCard("cmd_ai", "AI", FontAwesomeIcon.Robot, "/bmrai", _availableAICommands, 1);
        DrawCommandCard("cmd_ar", "自动循环", FontAwesomeIcon.Bolt, "/bmr", _autorotationCommands, 2);
    }

    private static void DrawCommandCard(string id, string title, FontAwesomeIcon icon, string prefix, (string, string)[] commands, int index)
    {
        using var card = M3Card.Begin(id, title, icon, M3.AccentFor(index), subtitle: $"命令前缀：{prefix}");
        using var scope = ImRaii.PushId(id);
        var button = M3Widgets.IconButtonSize;
        foreach (var (args, description) in commands)
        {
            var command = args.Length > 0 ? $"{prefix} {args}" : prefix;
            var row = M3SettingRow.Begin(command, description, new(button, button), M3.Scheme.Primary);
            ImGui.SetCursorScreenPos(row.ControlPosition);
            _ = M3Widgets.CopyIconButton($"##{command}", command, snackbar: $"已复制 {command}");
            M3SettingRow.End(row);
        }
    }
}
