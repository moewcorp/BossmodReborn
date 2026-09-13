namespace BossMod.Endwalker.Savage.P12S2PallasAthena;

[ConfigDisplay(Order = 0x1C2, Parent = typeof(EndwalkerConfig))]
public sealed class P12S2PallasAthenaConfig() : ConfigNode()
{
    [PropertyDisplay("泛生论：踩塔分配策略")]
    [PropertyCombo("2+0：第一轮塔由短色与 0 层不稳定踩；之后两人都去北侧踩第二轮塔", "2+1：第一轮塔由短色与 1 层不稳定踩；之后两人分别去不同的第二轮塔")]
    public bool PangenesisFirstStack = true;
}
