namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

[ConfigDisplay(Order = 0x130, Parent = typeof(DawntrailConfig))]
public sealed class M08SHowlingBladeConfig() : ConfigNode()
{
    [PropertyDisplay("显示平台编号")]
    public bool ShowPlatformNumbers = true;

    [PropertyDisplay("平台编号颜色：")]
    public Color[] PlatformNumberColors = [new(0xffffffff), new(0xffffffff), new(0xffffffff), new(0xffffffff), new(0xffffffff)];

    [PropertyDisplay("平台编号字号")]
    [PropertySlider(0.1f, 100, Speed = 1)]
    public float PlatformNumberFontSize = 22;

    public enum ReignStrategy
    {
        [PropertyDisplay("显示当前职责的两个安全点")]
        Any,
        [PropertyDisplay("从场地中心面向首领时，假设 G1 在左、G2 在右")]
        Standard,
        [PropertyDisplay("从场地中心面向首领时，假设 G1 在右、G2 在左")]
        Inverse,
        [PropertyDisplay("无")]
        Disabled
    }

    [PropertyDisplay("旋击群狼剑/扫击群狼剑 站位提示")]
    public ReignStrategy ReignHints = ReignStrategy.Standard;

    [PropertyDisplay("为独狼的诅咒显示 Rinon/Toxic Friends 塔位")]
    public bool LoneWolfsLamentHints = true;
}
