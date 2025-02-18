﻿namespace BossMod.Endwalker.VariantCriterion.C02AMR.C021Shishio;

class EyeThunderVortex(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeCircle _shapeCircle = new(15f);
    private static readonly AOEShapeDonut _shapeDonut = new(8f, 30f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? [_aoes[0]] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NEyeOfTheThunderVortexFirst:
            case (uint)AID.SEyeOfTheThunderVortexFirst:
                AddAOEs(_shapeCircle, _shapeDonut);
                break;
            case (uint)AID.NVortexOfTheThunderEyeFirst:
            case (uint)AID.SVortexOfTheThunderEyeFirst:
                AddAOEs(_shapeDonut, _shapeCircle);
                break;
        }
        void AddAOEs(AOEShape shape1, AOEShape shape2)
        {
            _aoes.Add(new(shape1, spell.LocXZ, default, Module.CastFinishAt(spell)));
            _aoes.Add(new(shape2, spell.LocXZ, default, Module.CastFinishAt(spell, 4f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NEyeOfTheThunderVortexFirst:
            case (uint)AID.NEyeOfTheThunderVortexSecond:
            case (uint)AID.NVortexOfTheThunderEyeFirst:
            case (uint)AID.NVortexOfTheThunderEyeSecond:
            case (uint)AID.SEyeOfTheThunderVortexFirst:
            case (uint)AID.SEyeOfTheThunderVortexSecond:
            case (uint)AID.SVortexOfTheThunderEyeFirst:
            case (uint)AID.SVortexOfTheThunderEyeSecond:
                if (_aoes.Count != 0)
                    _aoes.RemoveAt(0);
                ++NumCasts;
                break;
        }
    }
}
