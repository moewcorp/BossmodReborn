using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BossMod.Stormblood.Ultimate.UCOB;

[ConfigDisplay(Order = 0x200, Parent = typeof(StormbloodConfig))]
public sealed class UCOBConfig() : ConfigNode()
{
    [PropertyDisplay("P1 火球 1：为坦克与治疗添加分摊规避提示")]
    public bool P1Fireball1LBHints = true;

    [PropertyDisplay("P3 进军/天地三重奏：安全点分配（假设巴哈姆特为相对北/上，L 组向左；L1/R1 最靠近首领）")]
    [GroupDetails(["L1", "L2", "L3", "L4", "R1", "R2", "R3", "R4"])]
    [GroupPreset("Hector: THMR", [0, 4, 1, 5, 2, 6, 3, 7])]
    [GroupPreset("LPDU: HTTH/RMMR", [1, 2, 0, 3, 5, 6, 4, 7])]
    public GroupAssignmentUnique P3QuickmarchTrioAssignments = new() { Assignments = [0, 4, 1, 5, 2, 6, 3, 7] };

    [PropertyDisplay("P3 天地的三重奏：塔优先级，从尼尔开始顺时针")]
    [GroupDetails(["0", "1", "2", "3", "4", "5", "6", "7"])]
    [GroupPreset("Hector: THMR, G1 CCW, G2 CW", [7, 0, 6, 1, 5, 2, 4, 3])]
    public GroupAssignmentUnique P3HeavensfallTrioTowers = new() { Assignments = [7, 0, 6, 1, 5, 2, 4, 3] };

    [SectionStart("仅 AI 可用设置")]
    [PropertyDisplay("P1：被分配踩垂直下落的玩家（用于极限技）", renderer: typeof(RolesRenderer))]
    public BitMask P1PlummetTargets = new();
}

public sealed class RolesRenderer : PropertyRenderer
{
    public override bool Draw(PropertyDisplayAttribute attrs, bool nested, ConfigNode node, object value, ConfigRoot root, UITree tree, WorldState ws)
    {
        var cfg = (UCOBConfig)node;

        ConfigUI.DrawHelp(attrs.Tooltip, nested);

        var modified = false;

        foreach (var _ in tree.Node(attrs.Label, false))
        {
            using (ImRaii.PushIndent())
            {
                for (var i = 2; i < 8; ++i)
                {
                    var assignment = (PartyRolesConfig.Assignment)i;
                    var isChecked = cfg.P1PlummetTargets[i];
                    if (ImGui.Checkbox($"{assignment}###plummet{i}", ref isChecked))
                    {
                        if (isChecked)
                        {
                            cfg.P1PlummetTargets.Set(i);
                        }
                        else
                        {
                            cfg.P1PlummetTargets.Clear(i);
                        }
                        modified = true;
                    }
                    if (i < 7)
                    {
                        ImGui.SameLine();
                    }
                }
            }
        }

        return modified;
    }
}
