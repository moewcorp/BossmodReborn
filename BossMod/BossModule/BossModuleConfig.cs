namespace BossMod;

[ConfigDisplay(Name = "Boss模块和雷达", Order = 1)]
public class BossModuleConfig : ConfigNode
{
    // boss module settings
    [PropertyDisplay("模块加载的最低成熟度", tooltip: "某些模块将处于\"WIP\"状态，除非你更改此设置，否则不会自动加载")]
    public BossModuleInfo.Maturity MinMaturity = BossModuleInfo.Maturity.Contributed;

    [PropertyDisplay("允许模块自动使用技能", tooltip: "示例：模块可以在击退发生前自动使用防击退技能")]
    public bool AllowAutomaticActions = true;

    [PropertyDisplay("显示测试雷达和提示窗口", tooltip: "在不进行boss战时配置雷达和提示窗口非常有用", separator: true)]
    public bool ShowDemo = false;

    // radar window settings
    [PropertyDisplay("启用雷达")]
    public bool Enable = true;

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

    [PropertyDisplay("旋转雷达以匹配相机方向")]
    public bool RotateArena = true;

    [PropertyDisplay("如果旋转地图关闭，则将地图旋转 180°")]
    public bool FlipArena = false;

    [PropertyDisplay("为雷达提供额外的旋转空间", tooltip: "如果您使用上述设置，您可以在修剪边缘之前在侧面给雷达额外的空间，以便考虑在战斗过程中旋转相机或给基本方向空间。", since: "7.2.0.169")]
    [PropertySlider(1, 2, Speed = 0.1f, Logarithmic = true)]
    public float SlackForRotations = 1.5f;

    [PropertyDisplay("在雷达中显示场地边框")]
    public bool ShowBorder = true;

    [PropertyDisplay("当玩家处于危险时更改场地边框颜色", tooltip: "当你站在可能被机制击中的位置时，将白色边框变为红色")]
    public bool ShowBorderRisk = true;

    [PropertyDisplay("在雷达中显示方位名称")]
    public bool ShowCardinals = false;

    [PropertyDisplay("方位名称字体大小")]
    [PropertySlider(0.1f, 100, Speed = 1)]
    public float CardinalsFontSize = 17;

    [PropertyDisplay("场地标记字体大小")]
    [PropertySlider(0.1f, 100, Speed = 1)]
    public float WaymarkFontSize = 22;

    [PropertyDisplay("角色三角型比例大小")]
    [PropertySlider(0.1f, 10, Speed = 0.1f)]
    public float ActorScale = 1;

    [PropertyDisplay("在雷达上显示标记点")]
    public bool ShowWaymarks = false;

    [PropertyDisplay("始终显示所有存活的队员")]
    public bool ShowIrrelevantPlayers = false;

    [PropertyDisplay("在雷达上为未着色的玩家显示基于职业的颜色")]
    public bool ColorPlayersBasedOnRole = false;

    [PropertyDisplay("始终显示焦点目标的队友", separator: true)]
    public bool ShowFocusTargetPlayer = false;

    // hint window settings
    [PropertyDisplay("在单独窗口中显示文字提示", tooltip: "将雷达窗口与提示窗口分离，允许你重新定位提示窗口")]
    public bool HintsInSeparateWindow = false;

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
}
