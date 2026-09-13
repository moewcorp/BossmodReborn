namespace BossMod.Endwalker.Ultimate.DSW1;

[ConfigDisplay(Order = 0x200, Parent = typeof(EndwalkerConfig))]
public sealed class DSW1Config() : ConfigNode()
{
    public enum HeavensflameHints
    {
        [PropertyDisplay("不显示任何提示")]
        None,

        [PropertyDisplay("匹配场景标记颜色：圆=红，三角=绿，十字=蓝，方=紫")]
        Waymarks,

        [PropertyDisplay("LPDU（正/斜）点：十字=北/南，方=东北/西南，圆=东/西，三角=东南/西北")]
        LPDU,
    }

    [PropertyDisplay("天火解法提示")]
    public HeavensflameHints Heavensflame = HeavensflameHints.None;
}
