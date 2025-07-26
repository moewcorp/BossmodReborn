namespace BossMod.Endwalker.Alliance.A34Eulogia;

// see A31 for details; apparently there is only 1 pattern here (rotated CW or CCW)
// unlike A31, origins are not cell centers, but south sides
class Hieroglyphika(BossModule module) : Components.GenericAOEs(module, (uint)AID.HieroglyphikaAOE)
{
    public bool BindsAssigned;
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeRect _shape = new(12f, 6f);
    private static readonly WDir[] _canonicalSafespots = [new(-18f, 18f), new(18f, -6f)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Bind)
            BindsAssigned = true;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        WDir dir = iconID switch
        {
            (uint)IconID.HieroglyphikaCW => new(-1f, 0f),
            (uint)IconID.HieroglyphikaCCW => new(1f, 0f),
            _ => default
        };
        if (dir == default)
            return;

        WDir[] safespots = [.. _canonicalSafespots.Select(d => d.Rotate(dir))];
        var activation = WorldState.FutureTime(17.1d);
        for (var z = -3; z <= 3; z += 2)
        {
            for (var x = -3; x <= 3; x += 2)
            {
                var cellOffset = new WDir(x * 6f, z * 6f);
                if (!safespots.Any(s => s.AlmostEqual(cellOffset, 1f)))
                {
                    AOEs.Add(new(_shape, (Arena.Center + cellOffset + new WDir(default, 6f)).Quantized(), 180f.Degrees(), activation));
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            var cnt = AOEs.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1f));
            if (cnt != 1)
                ReportError($"Incorrect AOE prediction: {caster.Position} matched {cnt} aoes");
        }
    }
}
