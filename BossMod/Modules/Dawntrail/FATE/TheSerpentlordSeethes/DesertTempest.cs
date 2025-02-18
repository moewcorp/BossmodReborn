namespace BossMod.Dawntrail.FATE.Ttokrrone;

class DesertTempest(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly Angle aM90 = -90.004f.Degrees();
    private static readonly Angle a90 = 89.999f.Degrees();
    private static readonly AOEShapeCone cone = new(19f, 90f.Degrees());
    private static readonly AOEShapeCircle circle = new(19f);
    private static readonly AOEShapeDonut donut = new(14f, 60f);
    private static readonly AOEShapeDonutSector donutSector = new(14f, 60f, 90f.Degrees());

    private readonly List<AOEInstance> _aoes = new(2);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DesertTempestVisualDonut:
                AddAOEs(donut);
                break;
            case (uint)AID.DesertTempestVisualCircle:
                AddAOEs(circle);
                break;
            case (uint)AID.DesertTempestVisualConeDonutSegment:
                AddAOEs(cone, donutSector);
                break;
            case (uint)AID.DesertTempestVisualDonutSegmentCone:
                AddAOEs(donutSector, cone);
                break;
        }
        void AddAOEs(AOEShape first, AOEShape? second = null)
        {
            var position = Module.PrimaryActor.Position;
            _aoes.Add(new(first, position, spell.Rotation + a90, Module.CastFinishAt(spell, 1f)));
            if (second != null)
                _aoes.Add(new(second, position, spell.Rotation + aM90, Module.CastFinishAt(spell, 1f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.DesertTempestCircle:
                case (uint)AID.DesertTempestDonut:
                case (uint)AID.DesertTempestDonutSegment1:
                case (uint)AID.DesertTempestDonutSegment2:
                case (uint)AID.DesertTempestCone1:
                case (uint)AID.DesertTempestCone2:
                    _aoes.Clear();
                    break;
            }
    }
}
