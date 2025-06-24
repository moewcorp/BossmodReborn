namespace BossMod.Endwalker.Alliance.A31Thaliak;

// arena is split into 4x4 squares, with 2 safe spots - one along edge, another in farthest corner
// there are only two possible patterns here - safe spot is either along N or E edge (out of 8 possible ones):
// XXOX  and  OXXX
// XXXX       XXXX
// XXXX       XXXO
// OXXX       XXXX
// the pattern is then rotated CW or CCW, giving 4 possible results
class Hieroglyphika(BossModule module) : Components.GenericAOEs(module, (uint)AID.HieroglyphikaAOE)
{
    public bool BindsAssigned;
    public WDir SafeSideDir;
    public readonly List<AOEInstance> AOEs = new(14);

    private static readonly AOEShapeRect _shape = new(6f, 6f, 6f);
    private static readonly WDir[] _canonicalSafespots = [new(6f, -18f), new(-18f, 18f)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Bind)
            BindsAssigned = true;
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state != 0x00020001)
            return;

        WDir dir = index switch
        {
            0x17 => new(-1f, default),
            0x4A => new(default, 1f),
            _ => default
        };
        if (dir != default)
            SafeSideDir = dir;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var dir = iconID switch
        {
            (uint)IconID.HieroglyphikaCW => SafeSideDir.OrthoR(),
            (uint)IconID.HieroglyphikaCCW => SafeSideDir.OrthoL(),
            _ => default
        };
        if (dir == default)
            return;

        var safespots = new WDir[2];
        for (var i = 0; i < 2; ++i)
            safespots[i] = _canonicalSafespots[i].Rotate(dir);

        var activation = WorldState.FutureTime(17.1d);
        for (var z = -3; z <= 3; z += 2)
        {
            for (var x = -3; x <= 3; x += 2)
            {
                var cellOffset = new WDir(x * 6f, z * 6f);
                var found = false;
                for (var i = 0; i < 2; ++i)
                {
                    if (safespots[i].AlmostEqual(cellOffset, 1f))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    AOEs.Add(new(_shape, WPos.ClampToGrid(Arena.Center + cellOffset), default, activation));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID == WatchedAction)
        {
            AOEs.Clear();
            ++NumCasts;
        }
    }
}
