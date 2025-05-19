namespace BossMod.Dawntrail.Savage.M07SBruteAbombinator;

[ConfigDisplay(Order = 0x120, Parent = typeof(DawntrailConfig))]
public class M07SBruteAbombinatorConfig() : ConfigNode()
{
    [PropertyDisplay("Disable seed AOE prediction")]
    public bool EnableSeedPrediction = true;
}
