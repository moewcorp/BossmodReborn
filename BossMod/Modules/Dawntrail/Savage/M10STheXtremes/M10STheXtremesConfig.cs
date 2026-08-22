using Dalamud.Bindings.ImGui;

namespace BossMod.Dawntrail.Savage.M10STheXtremes;

public enum Strategy
{
    [PropertyDisplay("Hector (NA)")]
    Hector,

    [PropertyDisplay("game8 (JP)")]
    game8,
}

[ConfigDisplay(Order = 0x150, Parent = typeof(DawntrailConfig))]
public class M10STheXtremesConfig : ConfigNode
{
    [PropertyDisplay("选择用于提示的策略")]
    public Strategy HintOption = Strategy.Hector;

    // NA does NW - S - NW - N, drops puddle along voidzone in L-shape
    // JP does NE - NW - NE - NW, drops puddle along voidzone + wall in L-shape
    [PropertyDisplay("显示浪顶炽火的落点")]
    public bool ShowFlameFloaterHints = false;

    [PropertyDisplay("显示双重/交错旋水的落点")]
    public bool ShowWaterAlleyOopHints = false;

    //[GroupPreset("TTHH/MMRR", [0, 1, 2, 3, 4, 5, 6, 7])]
    // index is the role assignment, value is index of GroupDetails
    // Hector (NA) and game8 (JP) use same boss-relative positions for 1st Deep Blue only one
    // different positions for watersnaking (3-1 instead of 2-2)
    // same spots during split arena
    // same spots after IA2
    [PropertyDisplay("双重/交错旋水的顺序（Boss相对）")]
    [GroupDetails(["N", "NE", "E", "SE", "S", "SW", "W", "NW"])]
    [GroupPreset("Default", [0, 4, 6, 2, 5, 3, 7, 1])]
    public GroupAssignmentUnique WaterAlleyOopAssignment = new() { Assignments = [0, 4, 6, 2, 5, 3, 7, 1] };

    [PropertyDisplay("显示浪顶炽火的落点（火焰）")]
    public bool ShowFireAlleyOopHints = false;

    [PropertyDisplay("提前显示浪尖转体的劈斩")]
    public bool ShowDeepVarialEarly = false;

    public override void DrawCustom(UITree tree, WorldState ws)
    {
        var needAssignments = ShowWaterAlleyOopHints || ShowFireAlleyOopHints;
        if (needAssignments)
        {
            var partyConfig = Service.Config.Get<PartyRolesConfig>();
            var playerAssignment = partyConfig[ws.Party.Members[PartyState.PlayerSlot].ContentId];

            if (playerAssignment == PartyRolesConfig.Assignment.Unassigned)
            {
                ImGui.TextColoredWrapped(Colors.TextColor2, "请在\"队伍职责分配\"中设置玩家职责，否则所选选项将无法生效！");
            }
        }

        if (!WaterAlleyOopAssignment.Validate())
        {
            ImGui.TextColoredWrapped(Colors.TextColor2, "双重/交错旋水的职责分配无效！");
        }
    }
}
