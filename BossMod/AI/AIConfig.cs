namespace BossMod.AI;

[ConfigDisplay(Name = "AI配置", Order = 7)]
sealed class AIConfig : ConfigNode
{
    [PropertyDisplay("在标题栏中显示AI状态")]
    public bool ShowDTR = false;

    [PropertyDisplay("显示AI界面")]
    public bool DrawUI = false;

    [PropertyDisplay("焦点目标敌人")]
    public bool FocusTargetMaster = false;

    [PropertyDisplay("将按键广播到其他窗口")]
    public bool BroadcastToSlaves = false;

    [PropertyDisplay("跟随第几位队友")]
    public int FollowSlot = 0;

    [PropertyDisplay("禁止技能")]
    public bool ForbidActions = false;

    [PropertyDisplay("手动定位")]
    public bool ManualTarget = false;

    [PropertyDisplay("禁止移动")]
    public bool ForbidMovement = false;

    [PropertyDisplay("战斗时跟随")]
    public bool FollowDuringCombat = false;

    [PropertyDisplay("在boss模块启用期间跟随")]
    public bool FollowDuringActiveBossModule = false;

    [PropertyDisplay("脱战时跟随")]
    public bool FollowOutOfCombat = false;

    [PropertyDisplay("跟随目标")]
    public bool FollowTarget = false;

    [PropertyDisplay("跟随目标时所需的位置")]
    [PropertyCombo(["任何", "侧面", "背面", "正面"])]
    public Positional DesiredPositional = Positional.Any;

    [PropertyDisplay("到队友的最大距离")]
    public float MaxDistanceToSlot = 1;

    [PropertyDisplay("到目标的最大距离")]
    public float MaxDistanceToTarget = 2.6f;

    [PropertyDisplay("启用自动 AFK", tooltip: "脱离战斗时启用自动 AFK。AFK 期间 AI 不会使用自动循环或选中任何东西")]
    public bool AutoAFK = false;

    [PropertyDisplay("自动 AFK 时间", tooltip: "离开战斗直至启用 AFK 模式的时间（秒）。任何移动都将重置计时器或禁用已激活的 AFK 模式")]
    public float AFKModeTimer = 10;

    [PropertyDisplay("禁用加载障碍物地图", tooltip: "可能需要启用某些内容，例如深层迷宫")]
    public bool DisableObstacleMaps = false;

    [PropertyDisplay("运动决策延迟", tooltip: "更改此值需您自担风险，并保持此值较低！太高则无法及时适应某些机制。请确保针对不同内容重新调整此值")]
    public double MoveDelay = 0;

    [PropertyDisplay("骑乘时空闲")]
    public bool ForbidAIMovementMounted = false;

    public string? AIAutorotPresetName;
}
