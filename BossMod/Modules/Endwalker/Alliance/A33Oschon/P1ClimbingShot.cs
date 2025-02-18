namespace BossMod.Endwalker.Alliance.A33Oschon;

class P1ClimbingShot(BossModule module) : Components.Knockback(module)
{
    private readonly P1Downhill? _downhill = module.FindComponent<P1Downhill>();
    private Source? _knockback;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_knockback);
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos) || (_downhill?.ActiveAOEs(slot, actor).Any(z => z.Check(pos)) ?? false);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ClimbingShot1 or (uint)AID.ClimbingShot2 or (uint)AID.ClimbingShot3 or (uint)AID.ClimbingShot4)
            _knockback = new(spell.LocXZ, 20f, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ClimbingShot1 or (uint)AID.ClimbingShot2 or (uint)AID.ClimbingShot3 or (uint)AID.ClimbingShot4)
            _knockback = null;
    }
}
