namespace BossMod.Endwalker.Trial.T08Asura;

class SixBladedKhadga(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(6);
    private static readonly Angle a180 = 180f.Degrees();
    private static readonly AOEShapeCone cone = new(20f, 90f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = new AOEInstance[max];
        for (var i = 0; i < max; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoes[0].Rotation.AlmostEqual(aoe.Rotation + a180, Angle.DegToRad) ? aoe with { Risky = false } : aoe;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.KhadgaTelegraph1:
            case (uint)AID.KhadgaTelegraph2:
            case (uint)AID.KhadgaTelegraph3:
                _aoes.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 11.9f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.Khadga1:
                case (uint)AID.Khadga2:
                case (uint)AID.Khadga3:
                case (uint)AID.Khadga4:
                case (uint)AID.Khadga5:
                case (uint)AID.Khadga6:
                    _aoes.RemoveAt(0);
                    break;
            }
    }
}
