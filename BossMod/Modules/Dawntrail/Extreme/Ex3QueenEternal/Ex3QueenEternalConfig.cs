namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

[ConfigDisplay(Order = 0x010, Parent = typeof(DawntrailConfig))]
sealed class Ex3QueenEternalConfig() : ConfigNode()
{
    [PropertyDisplay("绝对君权：忽略火柱，集中分摊")]
    public bool AbsoluteAuthorityIgnoreFlares = true;

    [PropertyDisplay("西/东侧锁链的固定桥位", tooltip: "西/东不交叉的拉伸锁链通常用于 EU/NA 集合石，另一选项通常由 JP 使用")]
    [PropertyCombo("不交叉", "交叉（日服）")]
    public bool SideTethersCrossStrategy = false;
}
