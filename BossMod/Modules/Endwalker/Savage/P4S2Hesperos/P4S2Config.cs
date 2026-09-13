namespace BossMod.Endwalker.Savage.P4S2Hesperos;

[ConfigDisplay(Order = 0x142, Parent = typeof(EndwalkerConfig))]
public sealed class P4S2Config() : ConfigNode()
{
    [PropertyDisplay("第四幕：逆时针走 1/8 去踩带黑暗减益的塔")]
    public bool Act4DarkSoakCCW = false;

    [PropertyDisplay("第四幕：逆时针走 3/8 去解除水流连线")]
    public bool Act4WaterBreakCCW = false;

    [PropertyDisplay("荆棘悲剧：谢幕：输出优先解除减益")]
    public bool CurtainCallDDFirst = false;
}
