namespace BossMod.Dawntrail.Raid.M12NLindwurm;

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    private readonly List<Rectangle> _rects = [];
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00)
        {
            // arena break after Bring Down The House
            if (state == 0x00020001)
            {
                _rects.Add(new(Arena.Center - new WDir(-15f, 7.5f), 5f, 7.5f));
                _rects.Add(new(Arena.Center - new WDir(-5f, -7.5f), 5f, 7.5f));
                _rects.Add(new(Arena.Center - new WDir(5f, 7.5f), 5f, 7.5f));
                _rects.Add(new(Arena.Center - new WDir(15f, -7.5f), 5f, 7.5f));
                Arena.Bounds = new ArenaBoundsCustom([.. _rects]);
            }
            // other set of tiles break
            else if (state == 0x02000100)
            {
                _rects.Add(new(Arena.Center - new WDir(-15f, -7.5f), 5f, 7.5f));
                _rects.Add(new(Arena.Center - new WDir(-5f, 7.5f), 5f, 7.5f));
                _rects.Add(new(Arena.Center - new WDir(5f, -7.5f), 5f, 7.5f));
                _rects.Add(new(Arena.Center - new WDir(15f, 7.5f), 5f, 7.5f));
                Arena.Bounds = new ArenaBoundsCustom([.. _rects]);
            }
            // arena partial fix through middle
            else if (state is 0x00200010 or 0x08000400)
            {
                _rects.Add(new(Arena.Center, 20f, 5f));
                Arena.Bounds = new ArenaBoundsCustom([.. _rects]);
            }
            // arena reset
            else if (state is 0x10000004 or 0x00080004)
            {
                _rects.Clear();
                Arena.Bounds = M12NLindwurm.ArenaBounds;
            }
        }
    }
}
