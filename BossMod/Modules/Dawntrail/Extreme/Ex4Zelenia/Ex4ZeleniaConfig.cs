namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

[ConfigDisplay(Order = 0x140, Parent = typeof(DawntrailConfig))]
public sealed class Ex4ZeleniaConfig() : ConfigNode()
{
    [PropertyDisplay("危险玫瑰格颜色：")]
    public Color RoseTileColor = new(0x80008080);
}
