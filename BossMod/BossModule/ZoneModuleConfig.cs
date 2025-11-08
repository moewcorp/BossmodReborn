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

    [PropertyDisplay("锁定区域模块窗口移动和鼠标交互")]
    public bool Lock = false;

    [PropertyDisplay("使区域模块窗口透明", tooltip: "移除区域模块窗口周围的黑色边框；如果将雷达移至其他显示器，此功能将失效。")]
    public bool TransparentMode = false;
}
