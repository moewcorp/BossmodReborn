using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using RebornMaterial;
using System.Diagnostics;
using System.IO;

namespace BossMod.RebornUI;

internal sealed class HomePage(DirectoryInfo? replayDir, Action<string> navigate)
{
    public const string DiscordUrl = "https://discord.gg/p54TZMPnC9";
    public const string GitHubUrl = "https://github.com/FFXIV-CombatReborn/BossmodReborn";
    public const string WikiUrl = "https://github.com/awgil/ffxiv_bossmod/wiki";

    private static readonly Vector4 DiscordColor = Color.FromComponents(88u, 101u, 242u).ToFloat4();

    private readonly record struct Feature(string Title, FontAwesomeIcon Icon, string Page, string Action, string[] Points);

    private readonly Feature[] _features =
    [
        new("雷达", FontAwesomeIcon.Bullseye, nameof(BossModuleConfig), "雷达设置",
        [
            "在屏幕上的场地小地图中显示玩家、Boss、即将发生的 AOE 和其他机制。",
            "无需记忆所有技能效果，即可判断自己是否会受到 AOE 攻击。",
            "适用于支持的战斗列表中的所有战斗。",
        ]),
        new("自动循环（不提供支持）", FontAwesomeIcon.Bolt, SettingsWindow.PresetsPage, "编辑预设",
        [
            "VBM 的自动循环系统。",
            "Combat Reborn 团队不提供支持，但会随 VBM 更新以兼容其他插件。",
        ]),
        new("冷却规划", FontAwesomeIcon.CalendarAlt, SettingsWindow.FightsPage, "支持的战斗",
        [
            "为支持的 Boss 创建冷却规划。",
            "安排技能在指定时刻使用，并专门兼容 Rotation Solver Reborn。",
        ]),
        new("自动移动", FontAwesomeIcon.Robot, nameof(AI.AIConfig), "自动移动设置",
        [
            "将角色移动到战斗模块在雷达上标出的安全位置。",
            "请勿在与陌生玩家组队时使用。",
        ]),
        new("回放", FontAwesomeIcon.Film, nameof(ReplayManagementConfig), "回放设置",
        [
            "用于开发战斗模块、分析问题和制作冷却规划。",
            "求助时请附上回放。未匿名化的回放包含角色名称。",
            "在回放设置中启用回放管理窗口或自动录制。",
        ]),
    ];

    public void Draw()
    {
        var s = M3.Scheme;
        using (M3Card.Begin("home_intro", "BossMod Reborn", FontAwesomeIcon.None, s.Primary, M3CardStyle.Elevated))
        {
            var logoSize = 64f * M3.Scale;
            ImGui.Dummy(new(logoSize));
            RebornFoldWindow.DrawLogo(ImGui.GetWindowDrawList(), ImGui.GetItemRectMin(), logoSize);
            ImGui.SameLine(0f, M3.Space3);
            using (ImRaii.Group())
            {
                M3Text.Draw("BossMod Reborn（BMR）提供战斗雷达、自动移动和冷却规划，各项功能均可独立开关。如需支持，请前往 Combat Reborn Discord。", M3TextStyle.Body, true);
            }
            ImGui.Dummy(new(0f, M3.Space1));
            M3Widgets.Banner("##home_fork", "这是原始 BossMod（VBM）的分支。请仅在 Combat Reborn Discord 求助。请勿同时加载 VBM 与 BMR，同时使用的后果未知且不受支持。",
                M3Severity.Warning, FontAwesomeIcon.ExclamationTriangle);
        }

        DrawFeatures();
        DrawLinks();
    }

    private void DrawFeatures()
    {
        var columns = ImGui.GetContentRegionAvail().X >= 620f * M3.Scale ? 2 : 1;
        using var table = ImRaii.Table("##home_features", columns, ImGuiTableFlags.SizingStretchSame);
        if (!table)
        {
            return;
        }

        for (var i = 0; i < _features.Length; ++i)
        {
            ImGui.TableNextColumn();
            ref readonly var feature = ref _features[i];
            using var card = M3Card.Begin($"home_feature_{i}", feature.Title, feature.Icon, M3.AccentFor(i));
            foreach (var point in feature.Points)
            {
                M3Text.Bullet(point, M3TextStyle.Muted, dot: M3.AccentFor(i));
            }

            ImGui.Dummy(new(0f, M3.Space1));
            if (M3Widgets.Button($"##home_open_{i}", feature.Action, M3ButtonStyle.Tonal, FontAwesomeIcon.ArrowRight))
            {
                navigate(feature.Page);
            }
        }
    }

    private void DrawLinks()
    {
        using var card = M3Card.Begin("home_links", "链接", FontAwesomeIcon.Link, M3.Scheme.Secondary);
        var error = "";
        var flow = new M3Flow();

        flow.Next(M3Widgets.ButtonWidth(FontAwesomeIcon.Comments, "Combat Reborn Discord"));
        if (M3Widgets.Button("##home_discord", "Combat Reborn Discord", M3ButtonStyle.Filled, FontAwesomeIcon.Comments, accent: DiscordColor))
        {
            error = OpenLink(DiscordUrl);
        }

        flow.Next(M3Widgets.ButtonWidth(FontAwesomeIcon.CodeBranch, "GitHub"));
        if (M3Widgets.Button("##home_github", "GitHub", M3ButtonStyle.Tonal, FontAwesomeIcon.CodeBranch))
        {
            error = OpenLink(GitHubUrl);
        }

        flow.Next(M3Widgets.ButtonWidth(FontAwesomeIcon.Book, "BossMod wiki"));
        if (M3Widgets.Button("##home_wiki", "BossMod wiki", M3ButtonStyle.Tonal, FontAwesomeIcon.Book))
        {
            error = OpenLink(WikiUrl);
        }

        flow.Next(M3Widgets.ButtonWidth(FontAwesomeIcon.FolderOpen, "打开回放文件夹"));
        if (M3Widgets.Button("##home_replays", "打开回放文件夹", M3ButtonStyle.Outlined, FontAwesomeIcon.FolderOpen, enabled: replayDir != null, tooltip: replayDir?.FullName))
        {
            error = OpenDirectory(replayDir!);
        }

        if (error.Length > 0)
        {
            M3Snackbar.Show(error, severity: M3Severity.Error);
        }
    }

    public static string OpenLink(string link)
    {
        try
        {
            Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
            return "";
        }
        catch (Exception e)
        {
            Service.Log($"Error opening link {link}: {e}");
            return $"无法打开链接 '{link}'，请在浏览器中手动打开。";
        }
    }

    private static string OpenDirectory(DirectoryInfo dir)
    {
        if (!dir.Exists)
        {
            return $"找不到目录 '{dir}'。";
        }

        try
        {
            Process.Start(new ProcessStartInfo(dir.FullName) { UseShellExecute = true });
            return "";
        }
        catch (Exception e)
        {
            Service.Log($"Error opening directory {dir}: {e}");
            return $"无法打开文件夹 '{dir}'，请手动打开。";
        }
    }
}
