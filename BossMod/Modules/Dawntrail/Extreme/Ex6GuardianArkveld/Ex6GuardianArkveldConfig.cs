namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

[ConfigDisplay(Order = 0x150, Parent = typeof(DawntrailConfig))]
public class Ex6GuardianArkveldConfig : ConfigNode
{
    public enum LimitCutStrategy
    {
        [PropertyDisplay("在正点轮流（1 西/东）")]
        Circle,
        [PropertyDisplay("偶数在北，奇数在南")]
        EvenNorth,
    }

    [PropertyDisplay("Limit Cut 站位提示")]
    public LimitCutStrategy LimitCutHints = LimitCutStrategy.Circle;
}
