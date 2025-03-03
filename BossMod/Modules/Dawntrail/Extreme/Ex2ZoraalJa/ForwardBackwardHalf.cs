namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

class ForwardBackwardHalf(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.HalfFullShortAOE))
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shapeEdge = new(50, 30, 10);
    private static readonly AOEShapeRect _shapeSide = new(60, 60);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (relevant, front, left) = spell.Action.ID switch
        {
            (uint)AID.ForwardHalfR or (uint)AID.ForwardHalfLongR => (true, true, false),
            (uint)AID.ForwardHalfL or (uint)AID.ForwardHalfLongL => (true, true, true),
            (uint)AID.BackwardHalfR or (uint)AID.BackwardHalfLongR => (true, false, false),
            (uint)AID.BackwardHalfL or (uint)AID.BackwardHalfLongL => (true, false, true),
            _ => default
        };
        if (!relevant)
            return;

        var cleaveDir = spell.Rotation + (front ? 180 : 0).Degrees();
        _aoes.Add(new(_shapeEdge, caster.Position, cleaveDir, Module.CastFinishAt(spell)));
        _aoes.Add(new(_shapeSide, caster.Position, cleaveDir + (left ? 90 : -90).Degrees(), Module.CastFinishAt(spell)));
    }
}

class HalfFull(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.HalfFullLongAOE))
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shapeSide = new(60, 60);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.HalfFullR or (uint)AID.HalfFullL)
        {
            var cleaveDir = spell.Rotation + (spell.Action.ID == (uint)AID.HalfFullL ? 90 : -90).Degrees();
            _aoes.Add(new(_shapeSide, caster.Position, cleaveDir, Module.CastFinishAt(spell)));
        }
    }
}
