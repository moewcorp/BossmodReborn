namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN4Index;

[ConfigDisplay(Order = 0x174, Parent = typeof(DawntrailConfig))]
public sealed class FTMN4IndexConfig : ConfigNode
{
    [PropertyDisplay("生成时强制 AI 攻击最近的增援")]
    public bool ForceAddTargeting = false;

    [PropertyDisplay("若无增援且无当前目标，强制 AI 攻击首领")]
    public bool ForceBossTargeting = false;
}
