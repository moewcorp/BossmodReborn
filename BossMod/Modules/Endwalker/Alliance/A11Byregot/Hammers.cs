namespace BossMod.Endwalker.Alliance.A11Byregot;

class HammersCells(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.DestroySideTiles), "GTFO from dangerous cell!")
{
    public bool Active;
    public bool MovementPending;
    private readonly int[] _lineOffset = new int[5];
    private readonly int[] _lineMovement = new int[5];

    private static readonly AOEShapeRect _shape = new(5, 5, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Active)
            yield break;

        for (var z = -2; z <= 2; ++z)
        {
            for (var x = -2; x <= 2; ++x)
            {
                if (CellDangerous(x, z, true))
                    yield return new(_shape, CellCenter(x, z), Color: Colors.AOE);
                else if (CellDangerous(x, z, false))
                    yield return new(_shape, CellCenter(x, z), Color: Colors.SafeFromAOE);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!Active)
            return;

        Arena.AddLine(Arena.Center + new WDir(-15, -25), Arena.Center + new WDir(-15, +25), Colors.Border);
        Arena.AddLine(Arena.Center + new WDir(-05, -25), Arena.Center + new WDir(-05, +25), Colors.Border);
        Arena.AddLine(Arena.Center + new WDir(+05, -25), Arena.Center + new WDir(+05, +25), Colors.Border);
        Arena.AddLine(Arena.Center + new WDir(+15, -25), Arena.Center + new WDir(+15, +25), Colors.Border);
        Arena.AddLine(Arena.Center + new WDir(-25, -15), Arena.Center + new WDir(+25, -15), Colors.Border);
        Arena.AddLine(Arena.Center + new WDir(-25, -05), Arena.Center + new WDir(+25, -05), Colors.Border);
        Arena.AddLine(Arena.Center + new WDir(-25, +05), Arena.Center + new WDir(+25, +05), Colors.Border);
        Arena.AddLine(Arena.Center + new WDir(-25, +15), Arena.Center + new WDir(+25, +15), Colors.Border);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Active = true;
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index is >= 7 and <= 11)
        {
            var i = index - 7;
            (_lineOffset[i], _lineMovement[i]) = state switch
            {
                0x00020001 => (00, +1),
                0x08000400 => (-1, +1),
                0x00800040 => (00, -1),
                0x80004000 => (+1, -1),
                _ => (_lineOffset[i], 0),
            };
            MovementPending = true;
            if (_lineMovement[i] == 0)
                ReportError($"Unexpected env-control {i}={state:X}, offset={_lineOffset[i]}");
        }
        else if (index == 26)
        {
            MovementPending = false;
            for (var i = 0; i < 5; ++i)
            {
                _lineOffset[i] += _lineMovement[i];
                _lineMovement[i] = 0;
            }
        }
        else if (index == 79 && state == 0x00080004)
        {
            Active = false;
            Array.Fill(_lineOffset, 0);
            Array.Fill(_lineMovement, 0);
        }
    }

    private WPos CellCenter(int x, int z) => Arena.Center + 10 * new WDir(x, z);

    private bool CellDangerous(int x, int z, bool future)
    {
        var off = _lineOffset[z + 2];
        if (future)
            off += _lineMovement[z + 2];
        return Math.Abs(x - off) > 1;
    }
}

class HammersLevinforge(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Levinforge), new AOEShapeRect(50, 5));
class HammersSpire(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ByregotSpire), new AOEShapeRect(50, 15));
