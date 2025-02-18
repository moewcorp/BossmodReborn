namespace BossMod.Endwalker.Alliance.A34Eulogia;

class ClimbingShot(BossModule module) : Components.Knockback(module)
{
    private readonly AsAboveSoBelow? _exaflare = module.FindComponent<AsAboveSoBelow>();
    private Source? _knockback;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_knockback);
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos) || (_exaflare?.ActiveAOEs(slot, actor).Any(z => z.Check(pos)) ?? false);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ClimbingShotNald or (uint)AID.ClimbingShotThal)
            _knockback = new(spell.LocXZ, 20f, Module.CastFinishAt(spell, 0.2f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ClimbingShotAOE1 or (uint)AID.ClimbingShotAOE2 or (uint)AID.ClimbingShotAOE3 or (uint)AID.ClimbingShotAOE4)
        {
            ++NumCasts;
            _knockback = null;
        }
    }
}
