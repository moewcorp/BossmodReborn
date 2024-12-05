namespace BossMod.Dawntrail.Alliance.A12Fafnir;

class SpikeFlail(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SpikeFlail), new AOEShapeCone(80, 135.Degrees()));
class Touchdown(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Touchdown), new AOEShapeCircle(24))
{
    private readonly DragonBreath? _aoe = module.FindComponent<DragonBreath>();

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _aoe?.AOE == null || Arena.Bounds != A12Fafnir.FireArena
            ? ActiveCasters.Select(c => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo))) : [];
    }
}

class DragonBreath(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.DragonBreath))
{
    public AOEInstance? AOE;

    private static readonly AOEShapeDonut donut = new(16, 30);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(AOE);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OffensivePostureDragonBreath)
        {
            NumCasts = 0;
            AOE = new(donut, Arena.Center, default, Module.CastFinishAt(spell, 1.2f));
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.FireVoidzone && state == 0x00040008)
            AOE = null;
    }
}
