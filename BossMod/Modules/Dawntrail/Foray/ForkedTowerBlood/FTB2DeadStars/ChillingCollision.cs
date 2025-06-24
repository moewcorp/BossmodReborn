namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB2DeadStars;

sealed class ChillingCollision(BossModule module) : Components.GenericKnockback(module, (uint)AID.ChillingCollision)
{
    private Knockback? _kb;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => Utils.ZeroOrOne(ref _kb);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ChillingCollisionVisual1)
        {
            _kb = new(spell.LocXZ, 21f, Module.CastFinishAt(spell, 1.2f));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb is Knockback kb)
        {
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(kb.Origin, 9f), act);
            }
        }
    }
}
