namespace BossMod.Dawntrail.Alliance.A11Prishe;

class KnuckleSandwich(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly HashSet<AID> castEnd = [AID.KnuckleSandwich1, AID.KnuckleSandwich2, AID.KnuckleSandwich3,
    AID.BrittleImpact1, AID.BrittleImpact2, AID.BrittleImpact3];
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(9), new AOEShapeCircle(18), new AOEShapeCircle(27), new AOEShapeDonut(9, 60),
    new AOEShapeDonut(18, 60), new AOEShapeDonut(27, 60)];
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        // extra ai hint: stay close to the edge of the first aoe
        if (_aoes.Count == 2 && _aoes[1].Shape is AOEShapeDonut donut)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(_aoes[0].Origin, donut.InnerRadius + 2), _aoes[0].Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = (AID)spell.Action.ID switch
        {
            AID.KnuckleSandwich1 => _shapes[0],
            AID.KnuckleSandwich2 => _shapes[1],
            AID.KnuckleSandwich3 => _shapes[2],
            AID.BrittleImpact1 => _shapes[3],
            AID.BrittleImpact2 => _shapes[4],
            AID.BrittleImpact3 => _shapes[5],
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell)));
            _aoes.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (castEnd.Contains((AID)spell.Action.ID))
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}
