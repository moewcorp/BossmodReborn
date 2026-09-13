namespace BossMod.Endwalker.Ultimate.TOP;

[ConfigDisplay(Order = 0x210, Parent = typeof(EndwalkerConfig))]
public sealed class TOPConfig() : ConfigNode()
{
    [PropertyDisplay("P1 循环程序：分配（G1 从西北顺时针，G2 默认逆时针，冲突时编号“较小”的机动）")]
    [GroupDetails(["G1 prio1", "G1 prio2", "G1 prio3", "G1 prio4", "G2 prio1", "G2 prio2", "G2 prio3", "G2 prio4"])]
    [GroupPreset("LPDU (global): M1>M2>MT>OT>R1>R2>H1>H2", [5, 4, 1, 0, 7, 6, 3, 2])]
    [GroupPreset("NA (snake prio): TMRH, group 2 north", [4, 0, 7, 3, 5, 1, 6, 2])]
    public GroupAssignmentUnique P1ProgramLoopAssignments = new() { Assignments = [5, 4, 1, 0, 7, 6, 3, 2] };

    [PropertyDisplay("P1 循环程序：改用全局优先级——将 G1 视为编号小于 G2（故 G1 更可能机动）")]
    public bool P1ProgramLoopGlobalPriority = true;

    [PropertyDisplay("P1 全能之主：分配（G1 北，G2 南，默认顺时针调整，冲突时编号“较小”的机动）")]
    [GroupDetails(["G1 prio1", "G1 prio2", "G1 prio3", "G1 prio4", "G2 prio1", "G2 prio2", "G2 prio3", "G2 prio4"])]
    [GroupPreset("LPDU (light parties): flex T>M>R", [0, 4, 3, 7, 1, 5, 2, 6])]
    [GroupPreset("NA (snake prio): TMRH, group 2 north", [4, 0, 7, 3, 5, 1, 6, 2])]
    public GroupAssignmentUnique P1PantokratorAssignments = new() { Assignments = [0, 4, 3, 7, 1, 5, 2, 6] };

    [PropertyDisplay("P1 全能之主：队伍站位")]
    [PropertyCombo("北/南", "东北/西南")]
    public bool P1PantokratorNESW = false;

    [PropertyDisplay("P1 全能之主：改用全局优先级——将 G1 视为编号小于 G2（故 G1 更可能机动）")]
    public bool P1PantokratorGlobalPriority = false;

    [PropertyDisplay("P2 协作程序：分配（G1 左，G2 右（面对眼睛时），冲突时编号“较小”的机动）")]
    [GroupDetails(["G1 prio1", "G1 prio2", "G1 prio3", "G1 prio4", "G2 prio1", "G2 prio2", "G2 prio3", "G2 prio4"])]
    [GroupPreset("LPDU (light parties): flex R>M>H", [3, 7, 2, 6, 1, 5, 0, 4])]
    [GroupPreset("NA (HRMT conga)", [0, 4, 3, 7, 1, 5, 2, 6])]
    public GroupAssignmentUnique P2PartySynergyAssignments = new() { Assignments = [3, 7, 2, 6, 1, 5, 0, 4] };

    [PropertyDisplay("P2 协作程序：改用全局优先级——将 G1 视为编号小于 G2（故 G1 更可能机动）")]
    public bool P2PartySynergyGlobalPriority = false;

    [PropertyDisplay("P2 协作程序：G2 在故障：远（远程连线）时的顺序")]
    [PropertyCombo("GPOB（仅 B 与 G 互换）（NA）", "GOPB（顺序颠倒）（LPDU）")]
    public bool P2PartySynergyG2ReverseAll = true;

    [PropertyDisplay("P2 协作程序：若两个分摊都在同一队伍则交换优先级")]
    [PropertyCombo("最北侧的一对（NA）", "最南侧的一对（LPDU）")]
    public bool P2PartySynergyStackSwapSouth = true;

    [PropertyDisplay("P3 幕间：分散/分摊站位分配，从西到东")]
    [GroupDetails(["1", "2", "3", "4", "5", "6", "7", "8"])]
    [GroupPreset("LPDU (RMTH HTMR)", [2, 5, 3, 4, 1, 6, 0, 7])]
    [GroupPreset("NA (HRMT conga)", [3, 4, 0, 7, 2, 5, 1, 6])]
    public GroupAssignmentUnique P3IntermissionAssignments = new() { Assignments = [2, 5, 3, 4, 1, 6, 0, 7] };

    [PropertyDisplay("P3 幕间：分散/分摊站位")]
    [PropertyCombo("分摊在南，分散在北", "分摊在北，分散在南")]
    public bool P3IntermissionStacksNorth = true;

    [PropertyDisplay("P3 监视者：优先级，从北到南")]
    [GroupDetails(["1", "2", "3", "4", "5", "6", "7", "8"])]
    [GroupPreset("LPDU (HTMR)", [2, 3, 0, 1, 4, 5, 6, 7])]
    [GroupPreset("NA (HRMT conga)", [3, 4, 0, 7, 2, 5, 1, 6])]
    public GroupAssignmentUnique P3MonitorsAssignments = new() { Assignments = [2, 3, 0, 1, 4, 5, 6, 7] };

    [PropertyDisplay("P3 监视者：安全侧监视者的站位")]
    [PropertyCombo("北（LPDU）", "南（Aether）")]
    public bool P3LastMonitorSouth = false;

    [PropertyDisplay("P3 监视者：机制判定前自动面向正确方向", tooltip: "此功能需要先在「设置 → 技能调整 → 智能角色定位」中启用。")]
    public bool P3MonitorForbiddenDirections = true;

    [PropertyDisplay("P4 波动炮：优先级，从北到南（假设南侧机动）")]
    [GroupDetails(["W1", "E1", "W2", "E2", "W3", "E3", "W4", "E4"])]
    [GroupPreset("LPDU (TRHM)", [0, 1, 4, 5, 6, 7, 2, 3])]
    public GroupAssignmentUnique P4WaveCannonAssignments = new() { Assignments = [0, 1, 4, 5, 6, 7, 2, 3] };
}
