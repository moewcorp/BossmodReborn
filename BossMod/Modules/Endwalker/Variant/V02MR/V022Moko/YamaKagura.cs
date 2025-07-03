namespace BossMod.Endwalker.VariantCriterion.V02MR.V022Moko;

sealed class YamaKagura(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.YamaKagura, 33f, shape: new AOEShapeRect(40f, 2.5f), kind: Kind.DirForward)
{
    private readonly GhastlyGrasp _aoe = module.FindComponent<GhastlyGrasp>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Casters.Count;
        if (count != 0)
        {
            var length = Arena.Bounds.Radius * 2;
            var forbidden = new Func<WPos, float>[count];
            for (var i = 0; i < count; ++i)
            {
                var c = Casters[i];
                forbidden[i] = ShapeDistance.Rect(c.Position, c.Rotation, length, Distance - length, 2.5f);
            }
            hints.AddForbiddenZone(ShapeDistance.Union(forbidden), Module.CastFinishAt(Casters[0].CastInfo));
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return !Module.InBounds(pos);
    }
}
