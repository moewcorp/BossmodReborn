namespace BossMod.Dawntrail.FATE.Ttokrrone;

class Sandspheres(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circleSmall = new(6);
    private static readonly AOEShapeCircle circleBig = new(12);
    private readonly List<AOEInstance> _aoesSmall = [];
    private readonly List<AOEInstance> _aoesBig = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        int aliveSandSpheres = 0;
        var sandspheres = Module.Enemies((uint)OID.SandSphere);
        var count = sandspheres.Count;
        for (var i = 0; i < count; ++i)
        {
            if (!sandspheres[i].IsDead)
                ++aliveSandSpheres;

            if (aliveSandSpheres > 7)
                break;
        }

        var max = (aliveSandSpheres == 7) ? 7 : 8;
        var countS = _aoesSmall.Count;
        var countB = _aoesBig.Count;

        var aoes = new List<AOEInstance>(max * 2);
        for (var i = 0; i < max; i++)
        {
            if (countS > i)
                aoes.Add(_aoesSmall[i]);
            if (countB > i)
                aoes.Add(_aoesBig[i]);
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.SummoningSands:
                _aoesSmall.Add(new(circleSmall, spell.LocXZ, default, Module.CastFinishAt(spell)));
                break;
            case (uint)AID.Sandburst1:
            case (uint)AID.Sandburst2:
                _aoesBig.Add(new(circleBig, spell.LocXZ, default, Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoesSmall.Count != 0 && spell.Action.ID == (uint)AID.SummoningSands)
            _aoesSmall.RemoveAt(0);
        else if (_aoesBig.Count == 0)
            return;
        switch (spell.Action.ID)
        {
            case (uint)AID.Sandburst1:
            case (uint)AID.Sandburst2:
                _aoesBig.RemoveAt(0);
                break;
        }
    }
}
