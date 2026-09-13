namespace BossMod.Dawntrail.Ultimate.FRU;

[ConfigDisplay(Order = 0x200, Parent = typeof(DawntrailConfig))]
public sealed class FRUConfig() : ConfigNode()
{
    // TODO: fixed tethers option
    [PropertyDisplay("P1 罪壤刺（轻锐小队连线）：分组分配与机动优先级（数字越小越机动）")]
    [GroupDetails(["N prio 1", "N prio 2", "N prio 3", "N prio 4", "S prio 1", "S prio 2", "S prio 3", "S prio 4"])]
    [GroupPreset("Supports N, DD S", [0, 1, 2, 3, 4, 5, 6, 7])]
    [GroupPreset("G1 N, G2 S, TMRH", [0, 4, 3, 7, 1, 5, 2, 6])]
    public GroupAssignmentUnique P1BoundOfFaithAssignment = GroupAssignmentUnique.DefaultRoles();

    [PropertyDisplay("P1 罪壤断（锥形连线）：火车优先级（没有连线的两人以较低优先级加入奇数组）")]
    [GroupDetails(["1", "2", "3", "4", "5", "6", "7", "8"])]
    [GroupPreset("HHTTMMRR", [2, 3, 0, 1, 4, 5, 6, 7])]
    [GroupPreset("HRMTTMRH", [3, 4, 0, 7, 2, 5, 1, 6])]
    public GroupAssignmentUnique P1FallOfFaithAssignment = new() { Assignments = [2, 3, 0, 1, 4, 5, 6, 7] };

    [PropertyDisplay("P1 罪壤断（锥形连线）：奇数组走西（而非北）")]
    public bool P1FallOfFaithEW = false;

    [PropertyDisplay("P1 爆炸：塔填充优先级（数字越小越靠北）")]
    [GroupDetails(["Tank N", "Tank S", "Fixed N", "Fixed Center", "Fixed S", "Flex 1", "Flex 2", "Flex 3"])]
    [GroupPreset("H1-R2-H2 fixed, M1-M2-R1 flex", [0, 1, 2, 4, 5, 6, 7, 3])]
    public GroupAssignmentUnique P1ExplosionsAssignment = new() { Assignments = [0, 1, 2, 4, 5, 6, 7, 3] };

    [PropertyDisplay("P1 爆炸：机动职责仅在其自然塔为1时填充3/4（而非火车）")]
    public bool P1ExplosionsPriorityFill;

    [PropertyDisplay("P1 爆炸：让坦克在坦克杀手上分摊（有减伤可存活，简化无损）")]
    public bool P1ExplosionsTankbusterCheese;

    [PropertyDisplay("P2 钻石星尘：正点分配")]
    [GroupDetails(["Support N", "Support E", "Support S", "Support W", "DD N", "DD E", "DD S", "DD W"])]
    [GroupPreset("Default", [0, 2, 3, 1, 7, 6, 4, 5])]
    public GroupAssignmentUnique P2DiamondDustCardinals = new() { Assignments = [0, 2, 3, 1, 7, 6, 4, 5] };

    [PropertyDisplay("P2 钻石星尘：辅助前往逆时针斜点")]
    public bool P2DiamondDustSupportsCCW;

    [PropertyDisplay("P2 钻石星尘：输出前往逆时针斜点")]
    public bool P2DiamondDustDDCCW;

    [PropertyDisplay("P2 钻石星尘：击退组")]
    [GroupDetails(["G1 (CCW from N)", "G2 (CW from NE)"])]
    public GroupAssignmentLightParties P2DiamondDustKnockbacks = GroupAssignmentLightParties.DefaultLightParties();

    [PropertyDisplay("P2 光之失控：火车站位（数字越小越靠西，假设顺时针旋转）")]
    [GroupDetails(["N1", "N2", "N3", "N4", "S1", "S2", "S3", "S4"])]
    [GroupPreset("HHTT/RRMM", [2, 3, 0, 1, 6, 7, 4, 5])]
    public GroupAssignmentUnique P2LightRampantAssignment = GroupAssignmentUnique.DefaultRoles();

    [PropertyDisplay("P3 时间压缩·绝：站位分配（数字越小越靠西北/西南）")]
    [GroupDetails(["1", "2", "3", "4"])]
    [GroupPreset("HTTH/RMMR", [1, 2, 0, 3, 1, 2, 0, 3])]
    public GroupAssignmentDDSupportPairs P3UltimateRelativityAssignment = GroupAssignmentDDSupportPairs.DefaultMeleeTogether();

    [PropertyDisplay("P3 启示：分配（G1 从北逆时针，G2 从东北顺时针，冲突时较小数字机动）")]
    [GroupDetails(["G1 prio1", "G1 prio2", "G1 prio3", "G1 prio4", "G2 prio1", "G2 prio2", "G2 prio3", "G2 prio4"])]
    [GroupPreset("TTHH/MMRR", [0, 1, 2, 3, 4, 5, 6, 7])]
    [GroupPreset("TMRH/TMRH", [0, 4, 3, 7, 1, 5, 2, 6])]
    public GroupAssignmentUnique P3ApocalypseAssignments = GroupAssignmentUnique.DefaultRoles();

    [PropertyDisplay("P3 启示：无损换位（仅考虑优先级1/2与3/4之间的换位，假设其为近战与远程）")]
    public bool P3ApocalypseUptime;

    [PropertyDisplay("P3 启示：忽略换位并使用初始固定位置进行分散")]
    public bool P3ApocalypseStaticSpreads;

    [PropertyDisplay("P4 光与暗的龙诗：分配（较低优先级更靠顺时针，最低优先级辅助占据北塔）")]
    [GroupDetails(["Support prio1", "Support prio2", "Support prio3", "Support prio4", "DD prio1", "DD prio2", "DD prio3", "DD prio4"])]
    [GroupPreset("Default (healer N)", [2, 3, 0, 1, 4, 5, 6, 7])]
    public GroupAssignmentUnique P4DarklitDragonsongAssignments = new() { Assignments = [2, 3, 0, 1, 4, 5, 6, 7] };

    [PropertyDisplay("P4 时间结晶：利爪分配（较低优先级靠西）")]
    [GroupDetails(["Prio 1", "Prio 2", "Prio 3", "Prio 4", "Prio 5", "Prio 6", "Prio 7", "Prio 8"])]
    [GroupPreset("Default HTMR", [3, 2, 1, 0, 4, 5, 6, 7])]
    public GroupAssignmentUnique P4CrystallizeTimeAssignments = new() { Assignments = [3, 2, 1, 0, 4, 5, 6, 7] };

    // ai-only settings
    [SectionStart("仅 AI 可用设置")]
    [PropertyDisplay("P1 暴风破（分摊分散轮转）：诱导钟点站位（辅助应靠近输出以处理配对）", tooltip: "仅 AI 使用")]
    [GroupDetails(["N", "NE", "E", "SE", "S", "SW", "W", "NW"])]
    [GroupPreset("Default", [0, 4, 6, 2, 5, 3, 7, 1])]
    public GroupAssignmentUnique P1CyclonicBreakSpots = new() { Assignments = [0, 4, 6, 2, 5, 3, 7, 1] };

    [PropertyDisplay("P1 暴风破（分摊分散轮转）：配对躲避方向", tooltip: "仅 AI 使用")]
    [PropertyCombo("辅助顺时针、输出逆时针", "辅助逆时针、输出顺时针")]
    public bool P1CyclonicBreakStackSupportsCCW = true;

    [PropertyDisplay("P1 暴风破（分摊分散轮转）：辅助的分散躲避方向", tooltip: "仅 AI 使用")]
    [PropertyCombo("顺时针", "逆时针")]
    public bool P1CyclonicBreakSpreadSupportsCCW;

    [PropertyDisplay("P1 暴风破（分摊分散轮转）：输出的分散躲避方向", tooltip: "仅 AI 使用")]
    [PropertyCombo("顺时针", "逆时针")]
    public bool P1CyclonicBreakSpreadDDCCW;

    [PropertyDisplay("P1 乐园绝技：初始钟点站位（MT 应靠近 OT 以处理坦克杀手）", tooltip: "仅 AI 使用")]
    [GroupDetails(["N", "NE", "E", "SE", "S", "SW", "W", "NW"])]
    [GroupPreset("Default", [0, 1, 6, 2, 5, 3, 7, 4])]
    public GroupAssignmentUnique P1UtopianSkyInitialSpots = new() { Assignments = [0, 1, 6, 2, 5, 3, 7, 4] };

    [PropertyDisplay("P1 乐园绝技：分散站位（G1 从北逆时针，G2 从东北顺时针）", tooltip: "仅 AI 使用")]
    [GroupDetails(["G1 Close", "G1 Far Center", "G1 Far Left", "G1 Far Right", "G2 Close", "G2 Far Center", "G2 Far Left", "G2 Far Right"])]
    [GroupPreset("Default", [1, 5, 0, 4, 2, 6, 3, 7])]
    public GroupAssignmentUnique P1UtopianSkySpreadSpots = new() { Assignments = [1, 5, 0, 4, 2, 6, 3, 7] };

    [PropertyDisplay("P2 镜中奇遇：首次分摊分散轮转的分散站位（从BOSS朝向蓝色镜面看）", tooltip: "仅 AI 使用")]
    [GroupDetails(["Boss opposite right", "Boss opposite left", "Boss side right", "Boss side left", "Mirror diagonal right", "Mirror diagonal left", "Mirror wall right", "Mirror wall left"])]
    [GroupPreset("Default", [0, 1, 4, 5, 2, 3, 6, 7])]
    public GroupAssignmentUnique P2MirrorMirror1SpreadSpots = new() { Assignments = [0, 1, 4, 5, 2, 3, 6, 7] };

    [PropertyDisplay("P2 镜中奇遇：第二次分摊分散轮转的分散站位（朝向红色镜面看，若两面红镜对称则假设顺时针旋转）", tooltip: "仅 AI 使用")]
    [GroupDetails(["Boss wall opposite other", "Boss wall facing other", "Boss center", "Boss diagonal", "Mirror wall right", "Mirror wall left", "Mirror center right", "Mirror center left"])]
    [GroupPreset("Default", [1, 0, 6, 7, 2, 3, 4, 5])]
    public GroupAssignmentUnique P2MirrorMirror2SpreadSpots = new() { Assignments = [1, 0, 6, 7, 2, 3, 4, 5] };

    [PropertyDisplay("P2 强放逐（光之失控后）：分散钟点站位（辅助应靠近输出以处理配对）", tooltip: "仅 AI 使用")]
    [GroupDetails(["N", "NE", "E", "SE", "S", "SW", "W", "NW"])]
    [GroupPreset("Default", [0, 4, 6, 2, 5, 3, 7, 1])]
    public GroupAssignmentUnique P2Banish2SpreadSpots = new() { Assignments = [0, 4, 6, 2, 5, 3, 7, 1] };

    [PropertyDisplay("P2 强放逐（光之失控后）：从默认分散站位移动以处理配对的责任", tooltip: "仅 AI 使用")]
    [PropertyCombo("输出", "辅助")]
    public bool P2Banish2SupportsMoveToStack = true;

    [PropertyDisplay("P2 强放逐（光之失控后）：移动以处理配对的方向", tooltip: "仅 AI 使用")]
    [PropertyCombo("顺时针", "逆时针")]
    public bool P2Banish2MoveCCWToStack = true;

    [PropertyDisplay("P2 间歇：钟点站位（正点优先其水晶，斜点诱导）", tooltip: "仅 AI 使用")]
    [GroupDetails(["N", "NE", "E", "SE", "S", "SW", "W", "NW"])]
    [GroupPreset("Default", [0, 2, 5, 3, 4, 6, 7, 1])]
    public GroupAssignmentUnique P2IntermissionClockSpots = new() { Assignments = [0, 2, 5, 3, 4, 6, 7, 1] };

    [PropertyDisplay("P3 启示：第1组的黑暗狂水1方向（第2组相反）", tooltip: "仅 AI 使用")]
    [PropertySlider(-180, 180)]
    public float P3ApocalypseDarkWater1ReferenceDirection = -90;

    [PropertyDisplay("P3 暗夜舞蹈：诱导者", tooltip: "仅 AI 使用")]
    [PropertyCombo("MT", "OT")]
    public bool P3DarkestDanceOTBait;

    [PropertyDisplay("P4 真夜舞蹈：诱导者", tooltip: "仅 AI 使用")]
    [PropertyCombo("MT", "OT")]
    public bool P4SomberDanceOTBait = true;

    [PropertyDisplay("P5 死亡轮回：左右分配", tooltip: "仅 AI 使用")]
    [GroupDetails(["Left (looking at boss)", "Right (looking at boss)"])]
    public GroupAssignmentLightParties P5AkhMornAssignments = GroupAssignmentLightParties.DefaultLightParties();

    [PropertyDisplay("P5 星灵之剑：诱导顺序", tooltip: "仅 AI 使用")]
    [GroupDetails(["Left 1", "Left 2", "Left 3", "Left 4", "Right 1", "Right 2", "Right 3", "Right 4"])]
    [GroupPreset("TMRH", [0, 4, 3, 7, 1, 5, 2, 6])]
    public GroupAssignmentUnique P5PolarizingStrikesAssignments = new() { Assignments = [0, 4, 3, 7, 1, 5, 2, 6] };
}
