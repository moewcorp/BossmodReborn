namespace BossMod.Endwalker.VariantCriterion.C02AMR.C022Gorai;

class FlameAndSulphur(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect _shapeFlameExpand = new(46f, 5f);
    private static readonly AOEShapeRect _shapeFlameSplit = new(46f, 2.5f);
    private static readonly AOEShapeCircle _shapeRockExpand = new(11f);
    private static readonly AOEShapeDonut _shapeRockSplit = new(6f, 16f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var activation = Module.CastFinishAt(spell, 3.1f);
        switch (spell.Action.ID)
        {
            case (uint)AID.BrazenBalladExpanding:
                foreach (var a in Module.Enemies(OID.FlameAndSulphurFlame))
                    _aoes.Add(new(_shapeFlameExpand, a.Position, a.Rotation, activation));
                foreach (var a in Module.Enemies(OID.FlameAndSulphurRock))
                    _aoes.Add(new(_shapeRockExpand, a.Position, a.Rotation, activation));
                break;
            case (uint)AID.BrazenBalladSplitting:
                foreach (var a in Module.Enemies(OID.FlameAndSulphurFlame))
                {
                    var offset = a.Rotation.ToDirection().OrthoL() * 7.5f;
                    _aoes.Add(new(_shapeFlameSplit, a.Position + offset, a.Rotation, activation));
                    _aoes.Add(new(_shapeFlameSplit, a.Position - offset, a.Rotation, activation));
                }
                foreach (var a in Module.Enemies(OID.FlameAndSulphurRock))
                    _aoes.Add(new(_shapeRockSplit, a.Position, a.Rotation, activation));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NFireSpreadExpand:
            case (uint)AID.NFireSpreadSplit:
            case (uint)AID.NFallingRockExpand:
            case (uint)AID.NFallingRockSplit:
            case (uint)AID.SFireSpreadExpand:
            case (uint)AID.SFireSpreadSplit:
            case (uint)AID.SFallingRockExpand:
            case (uint)AID.SFallingRockSplit:
                ++NumCasts;
                _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1f));
                break;
        }
    }
}
