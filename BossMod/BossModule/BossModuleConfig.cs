using Dalamud.Bindings.ImGui;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BossMod;

[ConfigDisplay(Name = "Boss模块和雷达", Order = 1)]
public sealed class BossModuleConfig : ConfigNode
{
    public bool RadarResize;

    public override void DrawCustom(UITree tree, WorldState ws)
    {
        if (ImGui.Button("重置窗口位置"))
        {
            Service.BossModWindow?.RecenterWindow();
        }
    }

    // boss module settings
    [JsonIgnore]
    public BossModuleInfo.Maturity MinMaturity = BossModuleInfo.Maturity.WIP;

    // Modules explicitly disabled from Supported fights. Primary actor OIDs are used as unique module IDs.
    public uint[] DisabledModuleOIDs = [];
    [JsonIgnore]
    internal HashSet<uint>? _disabledModuleOIDs;

    [PropertyDisplay("允许模块自动使用技能", tooltip: "示例：模块可以在击退发生前自动使用防击退技能")]
    public bool AllowAutomaticActions = true;

    [PropertyDisplay("显示测试雷达和提示窗口", tooltip: "在不进行boss战时配置雷达和提示窗口非常有用", separator: true, depends: nameof(EnableRadar))]
    public bool ShowDemo = false;

    // radar window settings
    [PropertyDisplay("启用雷达", separator: true)]
    public bool EnableRadar = true;

    [PropertyDisplay("将雷达投影到 3D 世界中")]
    public bool ProjectRadarInto3DWorld = false;

    [PropertyDisplay("在 3D 世界中显示角色三角", tooltip: "显示普通角色三角。禁用时，机制标记（包括击退目的地）仍保持可见。", depends: nameof(ProjectRadarInto3DWorld))]
    public bool ShowActorTrianglesIn3DWorld = true;

    [PropertyDisplay("在 3D 世界中绘制场地轮廓", tooltip: "如果启用将雷达投影到 3D 世界，则也可以绘制轮廓", depends: nameof(ProjectRadarInto3DWorld))]
    public bool EnableArenaOutlineIn3DWorld = true;

    [PropertyDisplay("允许在 3D 世界中绘制文本和图标广告牌", tooltip: "如果启用将雷达投影到 3D 世界，则也可以绘制广告牌", depends: nameof(ProjectRadarInto3DWorld))]
    public bool EnableTextIconBillboards = true;

    [PropertyDisplay("广告牌高度偏移", tooltip: "广告牌应出现在地面上方多少 yalms。包括视线、文本和图标。", depends: nameof(ProjectRadarInto3DWorld))]
    [PropertySlider(0f, 20f, Speed = 0.1f, Logarithmic = true)]
    public float BillboardHeightOffset = 5f;

    [PropertyDisplay("文本广告牌字体大小", tooltip: "更改 3D 世界广告牌的文本大小", depends: nameof(ProjectRadarInto3DWorld))]
    [PropertySlider(17f, 250f, Speed = 0.5f, Logarithmic = true)]
    public float TextBillboardFontSize = 110f;

    [PropertyDisplay("图标广告牌字体大小", tooltip: "更改 3D 世界广告牌的图标大小", separator: true, depends: nameof(ProjectRadarInto3DWorld))]
    [PropertySlider(17f, 250f, Speed = 0.5f, Logarithmic = true)]
    public float IconBillboardFontSize = 110f;

    [PropertyDisplay("锁定雷达和提示窗口的移动和鼠标交互")]
    public bool Lock = false;

    [PropertyDisplay("透明雷达窗口背景", tooltip: "去除雷达周围的黑色窗口；如果您将雷达移至其他的显示器，这将不起作用")]
    public bool TrishaMode = true;

    [PropertyDisplay("为雷达中的场地添加不透明背景")]
    public bool OpaqueArenaBackground = true;

    [PropertyDisplay("在各种雷达标记上显示轮廓和阴影")]
    public bool ShowOutlinesAndShadows = true;

    [PropertyDisplay("雷达场地缩放系数", tooltip: "雷达窗口内场地的缩放比例")]
    [PropertySlider(0.1f, 10, Speed = 0.1f, Logarithmic = true)]
    public float ArenaScale = 1;

    [PropertyDisplay("雷达元素厚度比例因子", tooltip: "全局缩放雷达元素的轮廓厚度")]
    [PropertySlider(0.1f, 10, Speed = 0.1f, Logarithmic = true)]
    public float ThicknessScale = 1;

    [PropertyDisplay("旋转雷达以匹配相机方向")]
    public bool RotateArena = true;

    [PropertyDisplay("如果旋转地图关闭，则将地图旋转 180°")]
    public bool FlipArena = false;

    [PropertyDisplay("为雷达提供额外的旋转空间", tooltip: "如果您使用上述设置，您可以在修剪边缘之前在侧面给雷达额外的空间，以便在战斗中旋转相机或给正点方位留出空间。")]
    [PropertySlider(1, 2, Speed = 0.1f, Logarithmic = true)]
    public float SlackForRotations = 1.5f;

    [PropertyDisplay("在雷达中显示场地边框")]
    public bool ShowBorder = true;

    [PropertyDisplay("当玩家处于危险时更改场地边框颜色", tooltip: "当你站在可能被机制击中的位置时，将白色边框变为红色")]
    public bool ShowBorderRisk = true;

    [PropertyDisplay("当玩家处于危险时屏幕边缘脉冲提示", tooltip: "当玩家警告激活时，以危险场地边框颜色（敌人颜色）脉冲发光。独立于雷达和 3D 投影设置工作。")]
    public bool ShowScreenRiskBorder = false;

    [PropertyDisplay("屏幕危险脉冲强度", depends: nameof(ShowScreenRiskBorder))]
    [PropertySlider(0f, 10f, Speed = 0.1f)]
    public float ScreenRiskBorderIntensity = 2.5f;

    [PropertyDisplay("在雷达中显示方位名称")]
    public bool ShowCardinals = false;

    [PropertyDisplay("方位名称字体大小", depends: nameof(ShowCardinals))]
    [PropertySlider(0.1f, 100f, Speed = 1f)]
    public float CardinalsFontSize = 17f;

    [PropertyDisplay("在雷达上显示场景标记")]
    public bool ShowWaymarks = false;

    [PropertyDisplay("场景标记字体大小", depends: nameof(ShowWaymarks))]
    [PropertySlider(0.1f, 100f, Speed = 1f)]
    public float WaymarkFontSize = 22f;

    [PropertyDisplay("在雷达上显示信号（“攻击”、“止步”、“禁止”和形状标记）")]
    public bool ShowSigns = false;

    [PropertyDisplay("始终显示所有存活的队员")]
    public bool ShowIrrelevantPlayers = false;

    [PropertyDisplay("在雷达上为未着色的玩家按职能着色")]
    public bool ColorPlayersBasedOnRole = false;

    [PropertyDisplay("始终显示焦点目标的队友", separator: true)]
    public bool ShowFocusTargetPlayer = false;

    [PropertyDisplay("角色三角形缩放系数")]
    [PropertySlider(0.1f, 10f, Speed = 0.1f)]
    public float ActorScale = 1f;

    // hint window settings
    [PropertyDisplay("开怪前显示机制提示弹窗", tooltip: "在开怪前显示该战斗的专属提示。单个战斗可永久隐藏，之后可在该战斗的配置窗口中重新启用。")]
    public bool ShowPrePullHints = true;

    // Persisted separately from module-specific config so every encounter can support "Never show again". Primary actor OIDs are used as unique module IDs.
    public uint[] SuppressedPrePullHintOIDs = [];

    [JsonIgnore]
    internal HashSet<uint>? _suppressedPrePullHintOIDs;

    [PropertyDisplay("在单独窗口中显示文字提示", tooltip: "将雷达窗口与提示窗口分离，允许你重新定位提示窗口")]
    public bool HintsInSeparateWindow = false;

    [PropertyDisplay("使单独的提示窗口透明")]
    public bool HintsInSeparateWindowTransparent = false;

    [PropertyDisplay("显示机制序列和计时提示")]
    public bool ShowMechanicTimers = true;

    [PropertyDisplay("显示团队范围提示")]
    public bool ShowGlobalHints = true;

    [PropertyDisplay("显示玩家提示和警告", separator: true)]
    public bool ShowPlayerHints = true;

    // misc. settings
    [PropertyDisplay("在游戏中显示移动提示", tooltip: "使用较少，但可以在游戏中显示箭头，指示在某些机制中移动的位置")]
    public bool ShowWorldArrows = false;

    [PropertyDisplay("显示近战范围指示器")]
    public bool ShowMeleeRangeIndicator = false;

    [PropertyDisplay("最大加载距离", tooltip: "最大加载距离（单位：yalms，出于安全考虑上限为 100 yalms）。如果 Boss 距离超过该值，则不会加载模块；若模块已激活，也不会卸载。")]
    [PropertySlider(100f, 500f, Speed = 0.1f, Logarithmic = true)]
    public float MaxLoadDistance = 500f;

    public override void Deserialize(JsonElement j, JsonSerializerOptions ser)
    {
        base.Deserialize(j, ser);
        _disabledModuleOIDs = null;
        _suppressedPrePullHintOIDs = null;
    }

    public bool IsModuleEnabled(uint primaryActorOID) => !DisabledModuleOIDSet().Contains(primaryActorOID);

    public bool IncludeInSupportedFightControls(BossModuleRegistry.Info info)
        => info.Maturity != BossModuleInfo.Maturity.Dummy || MinMaturity == BossModuleInfo.Maturity.Dummy;

    public void SetModuleEnabled(uint primaryActorOID, bool enabled)
    {
        var set = DisabledModuleOIDSet();
        var disabled = set.Contains(primaryActorOID);
        if (enabled == !disabled)
        {
            return;
        }

        if (enabled)
        {
            set.Remove(primaryActorOID);
        }
        else
        {
            set.Add(primaryActorOID);
        }

        PersistDisabledModuleOIDs(set);
        Modified.Fire();
    }

    public void SetModulesEnabled(List<uint> primaryActorOIDs, bool enabled)
    {
        var set = DisabledModuleOIDSet();
        var changed = false;
        var count = primaryActorOIDs.Count;
        for (var i = 0; i < count; ++i)
        {
            changed |= enabled ? set.Remove(primaryActorOIDs[i]) : set.Add(primaryActorOIDs[i]);
        }

        if (changed)
        {
            PersistDisabledModuleOIDs(set);
            Modified.Fire();
        }
    }

    public (bool anyEnabled, bool allEnabled) ModulesEnabledState(List<uint> primaryActorOIDs)
    {
        var set = DisabledModuleOIDSet();
        var anyEnabled = false;
        var allEnabled = true;
        var count = primaryActorOIDs.Count;
        for (var i = 0; i < count; ++i)
        {
            var enabled = !set.Contains(primaryActorOIDs[i]);
            anyEnabled |= enabled;
            allEnabled &= enabled;
        }
        return (anyEnabled, count > 0 && allEnabled);
    }

    public void SetExpansionEnabled(BossModuleInfo.Expansion expansion, bool enabled)
    {
        var set = DisabledModuleOIDSet();
        var changed = false;
        foreach (var info in BossModuleRegistry.RegisteredModules.Values)
        {
            if (info.Expansion != expansion || !IncludeInSupportedFightControls(info))
            {
                continue;
            }
            changed |= enabled ? set.Remove(info.PrimaryActorOID) : set.Add(info.PrimaryActorOID);
        }

        if (changed)
        {
            PersistDisabledModuleOIDs(set);
            Modified.Fire();
        }
    }

    public void SetCategoryEnabled(BossModuleInfo.Category category, bool enabled)
    {
        var set = DisabledModuleOIDSet();
        var changed = false;
        foreach (var info in BossModuleRegistry.RegisteredModules.Values)
        {
            if (info.Category != category || !IncludeInSupportedFightControls(info))
            {
                continue;
            }
            changed |= enabled ? set.Remove(info.PrimaryActorOID) : set.Add(info.PrimaryActorOID);
        }

        if (changed)
        {
            PersistDisabledModuleOIDs(set);
            Modified.Fire();
        }
    }

    public (bool anyEnabled, bool allEnabled) ExpansionEnabledState(BossModuleInfo.Expansion expansion)
    {
        var set = DisabledModuleOIDSet();
        var anyEnabled = false;
        var allEnabled = true;
        var any = false;
        foreach (var info in BossModuleRegistry.RegisteredModules.Values)
        {
            if (info.Expansion != expansion || !IncludeInSupportedFightControls(info))
            {
                continue;
            }
            any = true;
            var enabled = !set.Contains(info.PrimaryActorOID);
            anyEnabled |= enabled;
            allEnabled &= enabled;
        }
        return (anyEnabled, any && allEnabled);
    }

    public (bool anyEnabled, bool allEnabled) CategoryEnabledState(BossModuleInfo.Category category)
    {
        var set = DisabledModuleOIDSet();
        var anyEnabled = false;
        var allEnabled = true;
        var any = false;
        foreach (var info in BossModuleRegistry.RegisteredModules.Values)
        {
            if (info.Category != category || !IncludeInSupportedFightControls(info))
            {
                continue;
            }
            any = true;
            var enabled = !set.Contains(info.PrimaryActorOID);
            anyEnabled |= enabled;
            allEnabled &= enabled;
        }
        return (anyEnabled, any && allEnabled);
    }

    private HashSet<uint> DisabledModuleOIDSet()
    {
        if (_disabledModuleOIDs != null)
        {
            return _disabledModuleOIDs;
        }

        var len = DisabledModuleOIDs.Length;
        var set = new HashSet<uint>(len);
        for (var i = 0; i < len; ++i)
        {
            set.Add(DisabledModuleOIDs[i]);
        }
        return _disabledModuleOIDs = set;
    }

    private void PersistDisabledModuleOIDs(HashSet<uint> set)
    {
        var persisted = new uint[set.Count];
        set.CopyTo(persisted);
        Array.Sort(persisted);
        DisabledModuleOIDs = persisted;
    }

    public bool ShowPrePullHintsFor(uint primaryActorOID) => !SuppressedPrePullHintOIDSet().Contains(primaryActorOID);

    public void SetShowPrePullHintsFor(uint primaryActorOID, bool show)
    {
        var set = SuppressedPrePullHintOIDSet();
        var suppressed = set.Contains(primaryActorOID);
        if (show == !suppressed)
        {
            return;
        }

        if (show)
        {
            set.Remove(primaryActorOID);
        }
        else
        {
            set.Add(primaryActorOID);
        }

        var persisted = new uint[set.Count];
        set.CopyTo(persisted);
        Array.Sort(persisted);
        SuppressedPrePullHintOIDs = persisted;
        Modified.Fire();
    }

    private HashSet<uint> SuppressedPrePullHintOIDSet()
    {
        if (_suppressedPrePullHintOIDs != null)
        {
            return _suppressedPrePullHintOIDs;
        }

        var len = SuppressedPrePullHintOIDs.Length;
        var set = new HashSet<uint>(len);
        for (var i = 0; i < len; ++i)
        {
            set.Add(SuppressedPrePullHintOIDs[i]);
        }
        return _suppressedPrePullHintOIDs = set;
    }
}
