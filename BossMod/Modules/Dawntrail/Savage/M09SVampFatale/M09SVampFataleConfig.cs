namespace BossMod.Dawntrail.Savage.M09SVampFatale;

[ConfigDisplay(Order = 0x140, Parent = typeof(DawntrailConfig))]
public sealed class M09SVampFataleConfig : ConfigNode
{
    //[PropertyDisplay("Enable force targetting adds", tooltip: "Prioritizes Deadly Doornail; melee move to flails once puddle is too big")]
    //public bool EnableForcedTarget = false;

    [PropertyDisplay("显示静态以太流失位置（NA / Hector）", tooltip: "若已配置队伍职责，仅显示自身位置。")]
    public bool ShowStaticAetherletting = false;

    [PropertyDisplay("启用 AI 静态以太流失分散位置（NA / Hector）", tooltip: "除非已配置队伍职责，否则无效")]
    public bool EnableStaticAetherlettingPuddle = false;

    [PropertyDisplay("显示笼中地狱塔顺序（北 顺时针 / Hector）", tooltip: "若已配置队伍职责，仅显示自身塔")]
    public bool ShowTowerOrder = false;

    [PropertyDisplay("启用 AI 笼中地狱塔顺序（北 顺时针 / Hector）", tooltip: "除非已配置队伍职责，否则无效")]
    public bool EnableTowerOrder = false;

    //[PropertyDisplay("Show Ultrasonic Amp/Spread bait positions (NA / Hector)")]
    //public bool ShowUltrasonicBait = false;

    [PropertyDisplay("启用 AI 血蝠死斗（LP1 北/西）", tooltip: "除非已配置队伍职责，否则无效")]
    public bool EnableDeathmatch = false;
}
