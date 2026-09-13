namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN1TwoHeadedAevis;

[ConfigDisplay(Order = 0x171, Parent = typeof(DawntrailConfig))]
public sealed class FTMN1TwoHeadedAevisConfig : ConfigNode
{
    [PropertyDisplay("强制 AI 攻击指定的头")]
    public bool ForceTargeting = false;
}
