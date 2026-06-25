namespace BossMod.Dawntrail.Ultimate.DMU;

[ConfigDisplay(Order = 0x400, Parent = typeof(DawntrailConfig))]
public class DMUConfig : ConfigNode {

    public enum P1TeleTrouncingStrategy {
        [PropertyDisplay("Modified Xolo")]
        Modified_Xolo,

        [PropertyDisplay("Freaky arrow CW box")]
        Freaky_Arrow,
    }

    [PropertyDisplay("P1 TeleTrouncing strategy")]
    public P1TeleTrouncingStrategy P1TeleTrouncing = P1TeleTrouncingStrategy.Modified_Xolo;

    [PropertyDisplay("P1 Graven Image 3 Static Spots")]
    public bool P1GravenImage3Static = true;

    public enum P2ForsakenStrategy {
        [PropertyDisplay("EU meow braindead strategy using markerless")]
        Meow_Markerless,

        [PropertyDisplay("EU meow braindead strategy using DN ZENITH markers")]
        Meow_DN_ZENITH_Markers,
    }

    [PropertyDisplay("P2 Forsaken strategy")]
    public P2ForsakenStrategy P2Forsaken = P2ForsakenStrategy.Meow_Markerless;
}
