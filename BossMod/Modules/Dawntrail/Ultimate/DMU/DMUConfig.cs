namespace BossMod.Dawntrail.Ultimate.DMU;

[ConfigDisplay(Order = 0x400, Parent = typeof(DawntrailConfig))]
public sealed class DMUConfig : ConfigNode {
    [SectionStart("策略设置")]
    [PropertyDisplay("P1 众神之像1 策略")]
    public P1GravenImage1Strategy P1GravenImage1 = P1GravenImage1Strategy.GravenImage1LPDU;

    public enum P1GravenImage1Strategy {
        [PropertyDisplay("无")]
        GravenImage1None,

        [PropertyDisplay("LPDU")]
        GravenImage1LPDU,
    }

    [PropertyDisplay("P1 波动炮排队优先级", tooltip: "排队顺序为从西到东")]
    [GroupDetails(["1", "2", "3", "4", "5", "6", "7", "8"])]
    [GroupPreset("HHTTMMRR", [3, 2, 1, 0, 4, 5, 6, 7])]
    public GroupAssignmentUnique P1WaveCannonAssignment = new() { Assignments = [3, 2, 1, 0, 4, 5, 6, 7] };

    [PropertyDisplay("P1 波动炮安全站位提示", tooltip: "显示引导攻击时的大致站位")]
    public bool P1WaveCannonHints = false;

    public enum P1DoubleTroubleStrategy {
        [PropertyDisplay("无")]
        DoubleTroubleNone,

        [PropertyDisplay("LPDU")]
        DoubleTroubleLPDU
    }

    [PropertyDisplay("P1 连环环陷阱策略")]
    public P1DoubleTroubleStrategy P1DoubleTrouble = P1DoubleTroubleStrategy.DoubleTroubleLPDU;

    [PropertyDisplay("P1 连环环陷阱击退安全站位提示", tooltip: "显示分摊时的大致站位，仍可能需要自行调整")]
    public bool P1DoubleTroubleKnockbackHints = false;

    public enum P1GravenImage2Strategy {
        [PropertyDisplay("无")]
        GravenImage2None,

        [PropertyDisplay("LPDU - 无损众神之像2")]
        GravenImage2LPDUUptime,
    }

    [PropertyDisplay("P1 众神之像2 策略")]
    public P1GravenImage2Strategy P1GravenImage2 = P1GravenImage2Strategy.GravenImage2LPDUUptime;

    public enum P1TeleTrouncingStrategy {
        [PropertyDisplay("无")]
        TeleTrouncingNone,

        [PropertyDisplay("改良 Xolo")]
        Modified_Xolo,

        [PropertyDisplay("Freaky arrow 顺时针 box（Merry Go Round）")]
        Freaky_Arrow,
    }

    [PropertyDisplay("P1 唰啦啦传送 策略")]
    public P1TeleTrouncingStrategy P1TeleTrouncing = P1TeleTrouncingStrategy.Freaky_Arrow;

    [PropertyDisplay("P1 唰啦啦传送 固定站位")]
    public bool P1TeleTrouncingStaticSpots = true;

    [PropertyDisplay("P1 众神之像3 策略")]
    public P1GravenImage3Strategy P1GravenImage3 = P1GravenImage3Strategy.GravenImage3LPDU;

    public enum P1GravenImage3Strategy {
        [PropertyDisplay("无")]
        GravenImage3None,

        [PropertyDisplay("LPDU")]
        GravenImage3LPDU,
    }

    public enum P2ForsakenStrategy {
        [PropertyDisplay("欧服 meow 无脑策略（使用无标记）")]
        Meow_Markerless,

        [PropertyDisplay("欧服 meow 无脑策略（使用 DN ZENITH 标记）")]
        Meow_DN_ZENITH_Markers,

        [PropertyDisplay("美服 Kroxy-Rinon（341 近战机动）")]
        Kroxy_Rinon_Melee_Flex,
    }

    [PropertyDisplay("P2 遗弃末世 策略")]
    public P2ForsakenStrategy P2Forsaken = P2ForsakenStrategy.Meow_Markerless;

    [SectionStart("AI 设置")]
    [PropertyDisplay("P1 恶狠狠毁荡始终以场地正北为基准", tooltip: "仅供 AI 使用，目前尚未生效")]
    public bool P1RevoltingRuinIIIAlwaysAroundTrueNorth = true;

    [PropertyDisplay("P1 恶狠狠毁荡首次引导", tooltip: "仅供 AI 使用，目前尚未生效")]
    [PropertyCombo("MT", "OT")]
    public bool P1RevoltingRuinIIIBait1OT = false;

    [PropertyDisplay("P1 超驱动始终以场地正北为基准", tooltip: "仅供 AI 使用，目前尚未生效")]
    public bool P1HyperDriveAlwaysAroundTrueNorth = true;

    [PropertyDisplay("P1 超驱动首次引导", tooltip: "仅供 AI 使用，目前尚未生效")]
    [PropertyCombo("MT", "OT")]
    public bool P1HyperDriveBait1OT = false;

    [SectionStart("调试设置")]
    [PropertyDisplay("P1 众神之像1 击退额外提示", tooltip: "仅供调试：H1 和 R1 仍需较大幅度移动；" +
                                                                              "若北侧安全，其他位置只需小幅调整")]
    public bool P1GravenImage1KnockbackAdditionalHints = false;
}
