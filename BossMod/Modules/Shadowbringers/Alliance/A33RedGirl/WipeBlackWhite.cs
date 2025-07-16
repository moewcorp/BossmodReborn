namespace BossMod.Shadowbringers.Alliance.A33RedGirl;

sealed class WipeBlackWhite(BossModule module) : Components.GenericAOEs(module)
{
    private readonly ArenaChanges _arena = module.FindComponent<ArenaChanges>()!;
    private readonly List<AOEInstance> _aoes = new(2);
    private readonly WDir[] positions = new WDir[2];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 && _aoes.Ref(0).Risky ? CollectionsMarshal.AsSpan(_aoes) : [];
    private int lastCountBlack, lastCountWhite;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.WipeWhite)
        {
            AddAOE();
        }
        else if (id == (uint)AID.WipeBlack)
        {
            AddAOE(1u);
        }

        void AddAOE(ulong id = default)
        {
            var center = Arena.Center;
            var pos = spell.LocXZ - center;
            positions[id] = pos;
            _aoes.Add(new(new AOEShapeCircle(default), center, default, Module.CastFinishAt(spell), risky: false, actorID: id));
        }
    }

    public void UpdateAOE(int lastWhite, int lastBlack, bool risky)
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
            if (id == default && lastWhite != lastCountWhite)
            {
                aoe.Shape = GetAOEShapeCustom(_arena.WhiteWalls);
                lastCountWhite = lastWhite;
            }
            else if (id == 1u && lastBlack != lastCountBlack)
            {
                aoe.Shape = GetAOEShapeCustom(_arena.BlackWalls);
                lastCountBlack = lastBlack;
            }
            aoe.Risky = risky;
            AOEShapeCustom GetAOEShapeCustom(RelSimplifiedComplexPolygon poly) => new([new PolygonCustomRel(Visibility.Compute(positions[id], poly))]);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WipeBlack or (uint)AID.WipeWhite)
        {
            _aoes.Clear();
            lastCountWhite = lastCountWhite = 0;
        }
    }
}
