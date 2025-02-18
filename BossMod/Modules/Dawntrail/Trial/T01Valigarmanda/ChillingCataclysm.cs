namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

class ChillingCataclysm(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeCross _shape = new(40, 2.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.ArcaneSphere2)
        {
            var activation = WorldState.FutureTime(7.1f);
            AOEs.Add(new(_shape, actor.Position, -0.003f.Degrees(), activation));
            AOEs.Add(new(_shape, actor.Position, 44.998f.Degrees(), activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ChillingCataclysm)
            AOEs.Clear();
    }
}
