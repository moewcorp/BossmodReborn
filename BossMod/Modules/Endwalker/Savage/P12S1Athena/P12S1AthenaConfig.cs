namespace BossMod.Endwalker.Savage.P12S1Athena;

[ConfigDisplay(Order = 0x1C1, Parent = typeof(EndwalkerConfig))]
public sealed class P12S1AthenaConfig() : ConfigNode()
{
    public enum EngravementOfSouls1Strategy
    {
        None,

        [PropertyDisplay("辅助从北顺时针，在最终站位中寻找第一个匹配者")]
        Default,
    }

    [PropertyDisplay("灵魂刻印1：解法提示")]
    public EngravementOfSouls1Strategy Engravement1Hints = EngravementOfSouls1Strategy.Default;
}
