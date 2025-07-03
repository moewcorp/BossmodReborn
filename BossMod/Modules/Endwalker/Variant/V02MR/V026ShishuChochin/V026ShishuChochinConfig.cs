namespace BossMod.Endwalker.VariantCriterion.V02MR.V026ShishuChochin;

[ConfigDisplay(Order = 0x100, Parent = typeof(EndwalkerConfig))]
public sealed class V026ShishuChochinConfig() : ConfigNode()
{
    [PropertyDisplay("Enable path 12 lantern AI")]
    public bool P12LanternAI = false;
}
