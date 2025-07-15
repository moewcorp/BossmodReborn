namespace BossMod.Shadowbringers.Alliance.A33RedGirl;

sealed class WipeBlackWhite(BossModule module) : Components.GenericAOEs(module)
{
    private readonly ArenaChanges _arena = module.FindComponent<ArenaChanges>()!;
    private readonly List<AOEInstance> _aoes = new(2);
    private readonly WDir[] positions = new WDir[2];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 && _aoes.Ref(0).Risky ? CollectionsMarshal.AsSpan(_aoes) : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.WipeWhite)
        {
            AddAOE(_arena.WhiteWalls);
        }
        else if (id == (uint)AID.WipeBlack)
        {
            AddAOE(_arena.BlackWalls, 1u);
        }

        void AddAOE(RelSimplifiedComplexPolygon wallPolygon, ulong id = default)
        {
            var center = Arena.Center;
            var pos = spell.LocXZ - center;
            positions[id] = pos;
            _aoes.Add(new(new AOEShapeCustom([new PolygonCustomRel(Visibility.ComputeVisibilityPolygon(pos, wallPolygon))]), center, default, Module.CastFinishAt(spell), risky: false, actorID: id));
        }
    }

    public void UpdateAOE(bool risky)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return;
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            var id = aoe.ActorID;
            aoe.Shape = new AOEShapeCustom([new PolygonCustomRel(Visibility.ComputeVisibilityPolygon(positions[id], id == default ? _arena.WhiteWalls : _arena.BlackWalls))]);
            aoe.Risky = risky;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WipeBlack or (uint)AID.WipeWhite)
        {
            _aoes.Clear();
        }
    }
}
