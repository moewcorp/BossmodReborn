namespace BossMod.Endwalker.VariantCriterion.C03AAI.C033Statice;

[ConfigDisplay(Order = 0x333, Parent = typeof(EndwalkerConfig))]
public sealed class C033SStaticeConfig() : ConfigNode()
{
    [PropertyDisplay("飞镖2：辅助相对西，输出相对东")]
    public bool Fireworks2Invert = false;
}
