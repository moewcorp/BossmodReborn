namespace BossMod.Dawntrail.Savage.M07SBruteAbombinator;

[ConfigDisplay(Order = 0x120, Parent = typeof(DawntrailConfig))]
public sealed class M07SBruteAbombinatorConfig() : ConfigNode()
{
    [PropertyDisplay("启用种子 AOE 预测")]
    public bool EnableSeedPrediction = true;

    [PropertyDisplay("启用碎颈臂预测")]
    public bool EnableLariatPrediction = true;
}
