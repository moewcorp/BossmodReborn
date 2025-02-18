namespace BossMod.Stormblood.Ultimate.UWU;

class P4Freefire(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.FreefireIntermission))
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShape _shape = new AOEShapeCircle(15); // TODO: verify falloff

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.Helper && id == 0x0449)
        {
            _aoes.Add(new(_shape, actor.Position, default, WorldState.FutureTime(5.9f)));
        }
    }
}
