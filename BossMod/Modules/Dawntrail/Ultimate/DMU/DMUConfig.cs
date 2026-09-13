namespace BossMod.Dawntrail.Ultimate.DMU;

[ConfigDisplay(Order = 0x400, Parent = typeof(DawntrailConfig))]
public sealed class DMUConfig : ConfigNode
{

    public enum P1GravenImage2Strategy
    {
        [PropertyDisplay("普通众神之像2")]
        GravenImage2Normal,

        [PropertyDisplay("无损众神之像2")]
        GravenImage2Uptime,
    }

    [PropertyDisplay("P1 众神之像2 策略")]
    public P1GravenImage2Strategy P1GravenImage2 = P1GravenImage2Strategy.GravenImage2Uptime;

    public enum P1TeleTrouncingStrategy
    {
        [PropertyDisplay("改良 Xolo")]
        Modified_Xolo,

        [PropertyDisplay("Freaky arrow 顺时针 box（Merry Go Round）")]
        Freaky_Arrow,
    }

    [PropertyDisplay("P1 唰啦啦传送 策略")]
    public P1TeleTrouncingStrategy P1TeleTrouncing = P1TeleTrouncingStrategy.Modified_Xolo;

    [PropertyDisplay("P1 众神之像3 固定站位")]
    public bool P1GravenImage3Static = true;

    public enum P2ForsakenStrategy
    {
        [PropertyDisplay("欧服 meow 无脑策略（使用无标记）")]
        Meow_Markerless,

        [PropertyDisplay("欧服 meow 无脑策略（使用 DN ZENITH 标记）")]
        Meow_DN_ZENITH_Markers,

        [PropertyDisplay("美服 Kroxy-Rinon（341 近战机动）")]
        Kroxy_Rinon_Melee_Flex,
    }

    [PropertyDisplay("P2 遗弃末世 策略")]
    public P2ForsakenStrategy P2Forsaken = P2ForsakenStrategy.Meow_Markerless;
}
