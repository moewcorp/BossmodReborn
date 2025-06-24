namespace BossMod.Dawntrail.Savage.M05SDancingGreen;

sealed class FunkyFloor(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect square = new(2.5f, 2.5f, 2.5f);
    private readonly List<AOEInstance> _aoes = new(64);
    private static readonly WPos[] ENVC20001 = GenerateCheckerboard(0); // 03.20001 top left active
    private static readonly WPos[] ENVC200010 = GenerateCheckerboard(1); // 03.200010 top left inactive
    private bool? _activeSet;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activeSet is bool set)
            return CollectionsMarshal.AsSpan(_aoes)[set ? ..32 : 32..];
        return [];
    }

    private static WPos[] GenerateCheckerboard(int offset)
    {
        var centers = new WPos[32];
        var index = 0;
        for (var i = 0; i < 8; ++i)
        {
            var z = i * 5;
            var start = (i + offset) % 2;
            for (var j = start; j < 8; j += 2)
            {
                centers[index++] = new(82.5f + j * 5, 82.5f + z);
            }
        }
        return centers;
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index != 0x03 || _activeSet != null)
            return;

        var act = WorldState.FutureTime(3.2d);
        AddAOESet(ENVC20001, act);
        AddAOESet(ENVC200010, act);

        _activeSet = state switch
        {
            0x00020001 => true,
            0x00200010 => false,
            _ => _activeSet
        };
        void AddAOESet(WPos[] positions, DateTime activation)
        {
            for (var i = 0; i < 32; ++i)
            {
                _aoes.Add(new(square, positions[i], default, activation));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.FunkyFloor)
            return;

        _activeSet = !_activeSet;
        var act = WorldState.FutureTime(4d);
        var aoes = CollectionsMarshal.AsSpan(_aoes);

        var start = _activeSet == true ? 0 : 32;
        for (var i = 0; i < 32; ++i)
            aoes[start + i].Activation = act;

        ++NumCasts;
    }
}
