namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

[ConfigDisplay(Order = 0x030, Parent = typeof(DawntrailConfig))]
class Ex3QueenEternalConfig() : ConfigNode()
{
    [PropertyDisplay("Absolute Authority: ignore flares, stack together")]
    public bool AbsoluteAuthorityIgnoreFlares = true;
}
