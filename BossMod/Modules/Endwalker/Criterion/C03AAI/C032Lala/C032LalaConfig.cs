using BossMod;
using BossMod.Endwalker;

namespace BossModReborn.Modules.Endwalker.Criterion.C03AAI.C032Lala;

[ConfigDisplay(Order = 0x333, Parent = typeof(EndwalkerConfig))]
public class C032LalaConfig() : ConfigNode()
{
    [PropertyDisplay("平面炸弹战术的另一种解法")]
    public bool PlanarTacticsReverse = false;
}
