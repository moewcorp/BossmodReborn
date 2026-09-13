namespace BossMod.Endwalker.Savage.P1SErichthonios;

[ConfigDisplay(Order = 0x110, Parent = typeof(EndwalkerConfig))]
public sealed class P1SConfig() : ConfigNode()
{
    public enum Corner { NW, NE, SE, SW }

    [PropertyDisplay("冰火侵蚀：在非对称阵型中与北互换的角落")]
    public Corner IntemperanceAsymmetricalSwapCorner = Corner.NW;
}
