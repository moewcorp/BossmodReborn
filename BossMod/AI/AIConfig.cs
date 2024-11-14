namespace BossMod.AI;

[ConfigDisplay(Name = "AI配置", Order = 7)]
sealed class AIConfig : ConfigNode
{
    [PropertyDisplay("在标题栏中显示AI状态")]
    public bool ShowDTR = false;

    [PropertyDisplay("显示AI窗口")]
    public bool DrawUI = true;

    [PropertyDisplay("聚焦目标敌人")]
    public bool FocusTargetLeader = true;

    [PropertyDisplay("将按键广播到其他窗口")]
    public bool BroadcastToSlaves = false;

    [PropertyDisplay("跟随第几位队友")]
    public int FollowSlot = 0;

    [PropertyDisplay("禁止技能")]
    public bool ForbidActions = false;

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

    [PropertyDisplay("Enable auto AFK", tooltip: "Enables auto AFK if out of combat. While AFK AI will not use autorotation or target anything")]
    public bool AutoAFK = false;

    [PropertyDisplay("Enable out of combat AFK mode", tooltip: "Time in seconds out of combat until AFK mode enables. Any movement will reset timer or disable AFK mode if already active.")]
    public float AFKModeTimer = 10;

    public string? AIAutorotPresetName;
}
