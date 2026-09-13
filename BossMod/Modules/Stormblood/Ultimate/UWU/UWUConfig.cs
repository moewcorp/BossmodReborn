namespace BossMod.Stormblood.Ultimate.UWU;

[ConfigDisplay(Order = 0x210, Parent = typeof(StormbloodConfig))]
public sealed class UWUConfig() : ConfigNode()
{
    [PropertyDisplay("泰坦的牢狱优先级（近 < 远）")]
    [GroupDetails(["0", "1", "2", "3", "4", "5", "6", "7"])]
    public GroupAssignmentUnique P3GaolPriorities = GroupAssignmentUnique.Default();

    [PropertyDisplay("显示寒风之歌 2 的固定位置")]
    public bool P1MistralSongFixedLocation = true;
}
