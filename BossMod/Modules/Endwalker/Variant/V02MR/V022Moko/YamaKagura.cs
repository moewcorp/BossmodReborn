namespace BossMod.Endwalker.VariantCriterion.V02MR.V022Moko;

class YamaKagura(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.YamaKagura), 33, shape: new AOEShapeRect(40, 2.5f), kind: Kind.DirForward)
{

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var source = Sources(slot, actor).FirstOrDefault();
        if (source != default)
        {
            var length = Arena.Bounds.Radius * 2;
            var forbidden = new List<Func<WPos, float>>();
            foreach (var d in Sources(slot, actor))
                forbidden.Add(ShapeDistance.Rect(d.Origin, d.Direction, length, Distance - length, 2.5f));
            if (forbidden.Count > 0)
                hints.AddForbiddenZone(ShapeDistance.Union(forbidden), source.Activation);
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<GhastlyGrasp>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.InBounds(pos);
}
