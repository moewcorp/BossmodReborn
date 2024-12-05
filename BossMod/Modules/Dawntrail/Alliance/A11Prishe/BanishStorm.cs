namespace BossMod.Dawntrail.Alliance.A11Prishe;

class BanishStorm(BossModule module) : Components.Exaflare(module, 6)
{
    public bool Done;

    private static readonly WPos[] positions = [new(815, 415), new(800, 385), new(785, 400), new(785, 385), new(815, 400), new(800, 415)];
    private static readonly WDir[] directions =
    [
        4 * (-0.003f).Degrees().ToDirection(),
        4 * 119.997f.Degrees().ToDirection(),
        4 * (-120.003f).Degrees().ToDirection(),
        4 * 180.Degrees().ToDirection(),
        4 * (-60.005f).Degrees().ToDirection(),
        4 * 60.Degrees().ToDirection(),
        4 * 89.999f.Degrees().ToDirection(),
        4 * (-150.001f).Degrees().ToDirection(),
        4 * (-30.001f).Degrees().ToDirection(),
        4 * (-90.004f).Degrees().ToDirection(),
        4 * 29.996f.Degrees().ToDirection(),
        4 * 149.996f.Degrees().ToDirection()
    ];
    private static readonly Dictionary<byte, (int position, int[] directions, int[] numExplosions)> LineConfigs = new()
    {
        { 0x0A, (0, [0, 1, 2], [5, 5, 14]) },
        { 0x34, (0, [0, 1, 2], [5, 5, 14]) },
        { 0x0D, (0, [3, 5, 4], [13, 5, 9]) },
        { 0x05, (3, [0, 1, 2], [13, 9, 5]) },
        { 0x02, (3, [3, 5, 4], [5, 14, 5]) },
        { 0x32, (3, [3, 5, 4], [5, 14, 5]) },
        { 0x0B, (1, [3, 4, 5], [5, 10, 10]) },
        { 0x35, (1, [3, 4, 5], [5, 10, 10]) },
        { 0x08, (1, [0, 2, 1], [13, 9, 9]) },
        { 0x09, (2, [9, 11, 10], [5, 10, 10]) },
        { 0x0C, (2, [6, 7, 8], [13, 9, 9]) },
        { 0x03, (4, [6, 7, 8], [5, 10, 10]) },
        { 0x06, (4, [9, 11, 10], [13, 9, 9]) },
        { 0x07, (5, [0, 1, 2], [5, 10, 10]) },
        { 0x33, (5, [0, 1, 2], [5, 10, 10]) },
        { 0x04, (5, [3, 5, 4], [13, 9, 9]) }
    };

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (LineConfigs.TryGetValue(index, out var config))
        {
            if (state == 0x00020001) // rod appear
            {
                var activation1 = WorldState.FutureTime(9.1f);
                var activation2 = WorldState.FutureTime(9.8f);

                for (var i = 0; i < 3; ++i)
                {
                    Lines.Add(new()
                    {
                        Next = positions[config.position] + (i > 0 ? directions[config.directions[i]] : default),
                        Advance = directions[config.directions[i]],
                        NextExplosion = i == 0 ? activation1 : activation2,
                        TimeToMove = 0.7f,
                        ExplosionsLeft = config.numExplosions[i],
                        MaxShownExplosions = config.numExplosions[i]
                    });
                }
            }
            else if (state == 0x00080004) // rod disappear
            {
                Done = true;
            }
            // 0x00200010 - aoe direction indicator appear
            // 0x00800040 - aoe direction indicator disappear
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Banish)
        {
            ++NumCasts;
            var index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}
