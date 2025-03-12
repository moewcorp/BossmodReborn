﻿namespace BossMod.Dawntrail.Savage.M02SHoneyBLovely;

class StageCombo(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(11);

    private static readonly AOEShapeCircle _shapeOut = new(7f);
    private static readonly AOEShapeDonut _shapeIn = new(7f, 30f);
    private static readonly AOEShapeCross _shapeCross = new(30f, 7f);
    private static readonly AOEShapeCone _shapeCone = new(30f, 22.5f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var deadline = _aoes[0].Activation.AddSeconds(1d);

        var index = 0;
        while (index < count && _aoes[index].Activation < deadline)
            ++index;

        return CollectionsMarshal.AsSpan(_aoes)[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (first, last, firstRot) = spell.Action.ID switch
        {
            (uint)AID.CenterstageCombo => (_shapeIn, _shapeOut, default),
            (uint)AID.OuterstageCombo => (_shapeOut, _shapeIn, 45f.Degrees()),
            _ => ((AOEShape?)null, (AOEShape?)null, new Angle())
        };
        if (first != null && last != null)
        {
            var firstActivation = Module.CastFinishAt(spell, 1.2f);
            var lastActivation = Module.CastFinishAt(spell, 7.5f);
            _aoes.Add(new(first, Arena.Center, 180f.Degrees(), firstActivation));
            AddAOEs(firstRot, Angle.AnglesCardinals, firstActivation);
            _aoes.Add(new(_shapeCross, Arena.Center, 180f.Degrees(), Module.CastFinishAt(spell, 4.2f)));
            _aoes.Add(new(last, Arena.Center, 180f.Degrees(), lastActivation));
            AddAOEs(firstRot, Angle.AnglesIntercardinals, lastActivation);
        }
    }

    private void AddAOEs(Angle firstRot, Angle[] angles, DateTime activation)
    {
        for (var i = 0; i < 4; ++i)
            _aoes.Add(new(_shapeCone, Arena.Center, firstRot + angles[i], activation));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.LacerationOut => _shapeOut,
            (uint)AID.LacerationCross => _shapeCross,
            (uint)AID.LacerationCone => _shapeCone,
            (uint)AID.LacerationIn => _shapeIn,
            _ => null
        };
        if (shape != null)
        {
            ++NumCasts;
            var firstActivation = _aoes.Count > 0 ? _aoes[0].Activation : default;
            var count = _aoes.RemoveAll(aoe => aoe.Shape == shape && aoe.Rotation.AlmostEqual(caster.Rotation, 0.1f) && aoe.Activation == firstActivation);
            if (count != 1)
                ReportError($"Unexpected aoe: {spell.Action} @ {caster.Rotation}deg");
        }
    }
}
