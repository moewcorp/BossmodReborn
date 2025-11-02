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

    [PropertyDisplay("Lock zone module window movement and mouse interaction")]
    public bool Lock = false;

    [PropertyDisplay("Make zone module windows transparent", tooltip: "Removes the black window around zone module windows; this will not work if you move the radar to a different monitor")]
    public bool TransparentMode = false;
}
