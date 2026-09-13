namespace BossMod.Endwalker.Savage.P8S2;

[ConfigDisplay(Order = 0x182, Parent = typeof(EndwalkerConfig))]
public sealed class P8S2Config() : ConfigNode()
{
    [PropertyDisplay("万象灰烬：坦克/治疗使用右侧")]
    public bool LimitlessDesolationTHRight = false;

    [PropertyDisplay("概念支配1：长减益踩南塔")]
    public bool HC1LongGoS = true;
}
