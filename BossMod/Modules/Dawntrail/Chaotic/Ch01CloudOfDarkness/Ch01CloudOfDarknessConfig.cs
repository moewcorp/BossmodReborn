namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

[ConfigDisplay(Order = 0x020, Parent = typeof(DawntrailConfig))]
sealed class Ch01CloudOfDarknessConfig() : ConfigNode()
{
    [PropertyDisplay("在雷达上显示已占用的格子", tooltip: "AI 不踩到已占用格子所必需。")]
    public bool ShowOccupiedTiles = true;
}
