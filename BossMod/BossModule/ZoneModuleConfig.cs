namespace BossMod;

[ConfigDisplay(Name = "完整任务自动化", Order = 6)]
public sealed class ZoneModuleConfig : ConfigNode
{
    [PropertyDisplay("要加载的区域模块所需的成熟度")]
    public BossModuleInfo.Maturity MinMaturity = BossModuleInfo.Maturity.Contributed;

    [PropertyDisplay("启用任务战斗/单独任务的自动执行")]
    public bool EnableQuestBattles = false;

    [PropertyDisplay("在游戏世界中绘制航点")]
    public bool ShowWaypoints = false;

    [PropertyDisplay("使用冲刺技能进行导航（速涂、回避跳跃等）")]
    public bool UseDash = true;

    [PropertyDisplay("显示 xan 调试 UI")]
    public bool ShowXanDebugger = false;
}
