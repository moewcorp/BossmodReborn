using BossMod;
using BossMod.Endwalker;

namespace BossModReborn.Modules.Endwalker.Criterion.C03AAI.C032Lala;

[ConfigDisplay(Order = 0x333, Parent = typeof(EndwalkerConfig))]
public class C032LalaConfig() : ConfigNode()
{
    [PropertyDisplay("Another Solution for PlanarTactics")]
    public bool PlanarTacticsReverse = false;
}
