namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class PCTConfig : ConfigNode
{
    [PropertyDisplay("速涂与镜头方向对齐")]
    public bool AlignDashToCamera = false;
}
