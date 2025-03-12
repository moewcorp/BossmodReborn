namespace BossMod.Endwalker.Alliance.A10Lions;

class RoaringBlaze(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shape = new(50f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.RoaringBlazeFirst or (uint)AID.RoaringBlazeSecond or (uint)AID.RoaringBlazeSolo)
        {
            _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            if (_aoes.Count > 1)
                _aoes.SortBy(x => x.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.RoaringBlazeFirst or (uint)AID.RoaringBlazeSecond or (uint)AID.RoaringBlazeSolo)
            _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
    }
}
