namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

class RadicalShift(BossModule module) : Components.GenericAOEs(module)
{
    public enum Rotation { None, Left, Right }

    private ArenaBoundsComplex? _left;
    private ArenaBoundsComplex? _right;
    private Rotation _nextRotation;
    private AOEInstance? _aoe;
    private static readonly Square[] defaultSquare = [new(Ex3QueenEternal.ArenaCenter, 20)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 12)
        {
            var rot = state switch
            {
                0x01000080 => Rotation.Left,
                0x08000400 => Rotation.Right,
                _ => Rotation.None
            };
            if (rot != Rotation.None)
            {
                _nextRotation = rot;
                UpdateAOE(NextPlatform);
            }
        }
        else if (state is 0x00020001 or 0x00200010)
        {
            var platform = index switch
            {
                9 => Ex3QueenEternal.WindBounds,
                10 => Ex3QueenEternal.EarthBounds,
                11 => Ex3QueenEternal.IceBridgeBounds,
                _ => null
            };
            if (platform != null)
            {
                (state == 0x00020001 ? ref _right : ref _left) = platform;
                UpdateAOE(NextPlatform);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RadicalShift)
        {
            var platform = NextPlatform;
            if (platform != null)
            {
                Arena.Bounds = platform;
                Arena.Center = platform.Center;
            }
            _left = _right = null;
            _nextRotation = Rotation.None;
            _aoe = null;
        }
    }

    private ArenaBoundsComplex? NextPlatform => _nextRotation switch
    {
        Rotation.Left => _left,
        Rotation.Right => _right,
        _ => null
    };

    private void UpdateAOE(ArenaBoundsComplex? platform)
    {
        AOEShapeCustom? aoe = null;
        var center = Arena.Center;
        if (platform == Ex3QueenEternal.WindBounds)
            aoe = new(defaultSquare, Trial.T03QueenEternal.T03QueenEternal.XArenaRects, Origin: center);
        else if (platform == Ex3QueenEternal.EarthBounds)
            aoe = new(defaultSquare, Trial.T03QueenEternal.T03QueenEternal.SplitArenaRects, Origin: center);
        else if (platform == Ex3QueenEternal.IceBridgeBounds)
            aoe = new(defaultSquare, Ex3QueenEternal.IceRectsAll, Origin: center);
        if (aoe != null)
            _aoe = new(aoe, center, default, WorldState.FutureTime(6));
    }
}

class RadicalShiftAOE(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.RadicalShiftAOE), 5);
