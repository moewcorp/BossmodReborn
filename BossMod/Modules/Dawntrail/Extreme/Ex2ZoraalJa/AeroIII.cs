namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

class AeroIII(BossModule module) : Components.Knockback(module, ignoreImmunes: true)
{
    public readonly List<Actor> Voidzones = module.Enemies((uint)OID.BitingWind);

    private static readonly AOEShapeCircle _shape = new(4);

    public override ReadOnlySpan<Source> ActiveSources(int slot, Actor actor)
    {
        var count = Voidzones.Count;
        if (count == 0)
            return [];
        var sources = new Source[count];
        for (var i = 0; i < count; ++i)
            sources[i] = new(Voidzones[i].Position, 25f, Shape: _shape);
        return sources;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = Voidzones.Count;
        if (count == 0)
            return;
        for (var i = 0; i < count; ++i)
            _shape.Outline(Arena, Voidzones[i].Position);
        base.DrawArenaForeground(pcSlot, pc);
    }
}
