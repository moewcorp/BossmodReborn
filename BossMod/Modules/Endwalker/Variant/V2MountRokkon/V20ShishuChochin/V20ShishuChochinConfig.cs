namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V20ShishuChochin;

[ConfigDisplay(Order = 0x100, Parent = typeof(EndwalkerConfig))]
public sealed class V20ShishuChochinConfig() : ConfigNode()
{
    [PropertyDisplay("启用第12路灯笼 AI")]
    public bool P12LanternAI = false;
}
