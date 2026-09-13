namespace BossMod.Dawntrail.Savage.M05SDancingGreen;

[ConfigDisplay(Order = 0x100, Parent = typeof(DawntrailConfig))]
public sealed class M05SDancingGreenConfig() : ConfigNode()
{
    [PropertyDisplay("绘制移动的 exaflare 图案", tooltip: "若关闭，则完整的舞浪全开 exaflare 图案会从一开始就绘制；否则会像普通模式一样移动。")]
    public bool MovingExaflares = true;

    [PropertyDisplay("显示相同顺序的所有聚光灯位置", tooltip: "若启用，显示与你自身顺序匹配的所有聚光灯位置。")]
    public bool ShowFromSameOrder = true;
    [PropertyDisplay("显示不同顺序的所有聚光灯位置", tooltip: "若启用，显示与你自身顺序不匹配的所有聚光灯位置。")]
    public bool ShowFromDifferentOrder = false;

    [PropertyDisplay("聚光灯绘制前的剩余时间")]
    [PropertySlider(0.1f, 34, Speed = 1)]
    public float SpotlightTimer = 34;
}
