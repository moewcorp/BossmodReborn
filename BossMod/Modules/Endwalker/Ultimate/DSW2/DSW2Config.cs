namespace BossMod.Endwalker.Ultimate.DSW2;

[ConfigDisplay(Order = 0x201, Parent = typeof(EndwalkerConfig))]
public sealed class DSW2Config() : ConfigNode()
{
    [PropertyDisplay("P2 苍穹之阵：圣杖（蓄能）：队伍分配")]
    [GroupDetails(["West/Across", "East/Behind"])]
    [GroupPreset("Default light parties", [0, 1, 0, 1, 0, 1, 0, 1])]
    [GroupPreset("Inverted light parties", [1, 0, 1, 0, 1, 0, 1, 0])]
    public GroupAssignmentLightParties P2SanctityGroups = GroupAssignmentLightParties.DefaultLightParties();

    [PropertyDisplay("P2 苍穹之阵：圣杖（蓄能）：队伍相对 DRK（对面/身后）而非绝对（西/东）")]
    public bool P2SanctityRelative = false;

    [PropertyDisplay("P2 苍穹之阵：圣杖（蓄能）：负责平衡队伍的职能（若未设置，则与职能搭档互换）")]
    public Role P2SanctitySwapRole;

    [PropertyDisplay("P2 苍穹之阵：圣杖（陨石）：需要时自动使用击退免疫")]
    public bool P2Sanctity2AutomaticAntiKB = true;

    [PropertyDisplay("P2 苍穹之阵：圣杖（陨石）：配对分配")]
    [GroupDetails(["North", "East", "South", "West"])]
    [GroupPreset("MT/R1 N, OT/R2 S, H1/M1 E, H2/M2 W", [0, 2, 1, 3, 1, 3, 0, 2])]
    [GroupPreset("MT/R1 N, OT/R2 S, H1/M1 W, H2/M2 E", [0, 2, 3, 1, 3, 1, 0, 2])]
    public GroupAssignmentDDSupportPairs P2Sanctity2Pairs = GroupAssignmentDDSupportPairs.DefaultOneMeleePerPair();

    public enum P2PreyCardinals
    {
        [PropertyDisplay("北/南 始终")]
        AlwaysNS,

        [PropertyDisplay("东/西 始终")]
        AlwaysEW,

        [PropertyDisplay("北/南，除非两个猎物都起始于东和西")]
        PreferNS,

        [PropertyDisplay("东/西，除非两个猎物都起始于北和南")]
        PreferEW,
    }

    [PropertyDisplay("P2 苍穹之阵：圣杖（陨石）：猎物目标偏好的正点")]
    public P2PreyCardinals P2Sanctity2PreyCardinals;

    [PropertyDisplay("P2 苍穹之阵：圣杖（陨石）：即使120度阵型也强制使用偏好正点（换位更简单，但走位更刁钻）")]
    public bool P2Sanctity2ForcePreferredPrey = true;

    public enum P2PreySwapDirection
    {
        [PropertyDisplay("所有猎物职能顺时针旋转")]
        RotateCW,

        [PropertyDisplay("所有猎物职能逆时针旋转")]
        RotateCCW,

        [PropertyDisplay("成对：北 <-> 东，南 <-> 西")]
        PairsNE,

        [PropertyDisplay("成对：北 <-> 西，南 <-> 东")]
        PairsNW,
    }

    [PropertyDisplay("P2 苍穹之阵：圣杖（陨石）：若两个猎物目标都处于错误的正点则交换方向")]
    public P2PreySwapDirection P2Sanctity2SwapDirection;

    [PropertyDisplay("P2 苍穹之阵：圣杖（陨石）：猎物职能偏好的外侧塔")]
    [PropertyCombo("逆时针（面向场外时最左侧）", "顺时针（面向场外时最右侧）")]
    public bool P2Sanctity2PreferCWTowerAsPrey = true;

    public enum P2OuterTowers
    {
        [PropertyDisplay("不尝试分配外侧塔")]
        None,

        [PropertyDisplay("始终使用偏好方向")]
        AlwaysPreferred,

        [PropertyDisplay("若角度更优，两个猎物目标都使用共同的相反方向；无猎物目标的象限内的玩家仍使用偏好方向")]
        SynchronizedTargets,

        [PropertyDisplay("若角度更优，两个猎物目标都使用共同的相反方向；所有象限内的玩家都使用相同方向")]
        SynchronizedRole,

        [PropertyDisplay("猎物目标使用能得到最佳角度的任意方向；无猎物目标的象限内的玩家仍使用偏好方向")]
        Individual
    }

    [PropertyDisplay("P2 苍穹之阵：圣杖（陨石）：外侧塔分配策略")]
    public P2OuterTowers P2Sanctity2OuterTowers = P2OuterTowers.Individual;

    public enum P2InnerTowers
    {
        [PropertyDisplay("不尝试分配内侧塔")]
        None,

        [PropertyDisplay("分配最近且无歧义的内侧塔")]
        Closest,

        [PropertyDisplay("分配第一个未被更近的人占用的顺时针塔")]
        CW,
    }

    [PropertyDisplay("P2 苍穹之阵：圣杖（陨石）：内侧塔分配策略")]
    public P2InnerTowers P2Sanctity2InnerTowers = P2InnerTowers.CW;

    [PropertyDisplay("P2 苍穹之阵：圣杖（陨石）：第二轮塔中非猎物职能的斜点")]
    [PropertyCombo("逆时针", "顺时针")]
    public bool P2Sanctity2NonPreyTowerCW = false;

    [PropertyDisplay("P3 堕天龙炎冲：看西边的箭头而非东边（即前进箭头去东位，后退箭头去西位）")]
    public bool P3DiveFromGraceLookWest = false;

    [PropertyDisplay("P3 点数塔：分配")]
    [GroupDetails(["NW Flex", "NE Flex", "SE Flex", "SW Flex", "NW Stay", "NE Stay", "SE Stay", "SW Stay"])]
    [GroupPreset("LPDU", [1, 3, 6, 0, 2, 4, 5, 7])]
    [GroupPreset("LPDU but CCW", [0, 2, 5, 7, 1, 3, 4, 6])]
    [GroupPreset("NA", [1, 3, 4, 6, 0, 2, 5, 7])]
    public GroupAssignmentUnique P3DarkdragonDiveCounterGroups = GroupAssignmentUnique.Default();

    [PropertyDisplay("P3 点数塔：优先向逆时针塔机动（而非顺时针）")]
    public bool P3DarkdragonDiveCounterPreferCCWFlex = false;

    public enum P6MortalVow
    {
        [PropertyDisplay("不假设任何顺序")]
        None,

        [PropertyDisplay("LPDU：MT->OT->M1（M2 作为后备）->R1")]
        TanksMeleeR1,

        [PropertyDisplay("LPDU：MT->OT->M1（M2 作为后备）->R2")]
        TanksMeleeR2,
    }

    [PropertyDisplay("P6 灭杀的誓言 传递顺序")]
    public P6MortalVow P6MortalVowOrder = P6MortalVow.None;
}
