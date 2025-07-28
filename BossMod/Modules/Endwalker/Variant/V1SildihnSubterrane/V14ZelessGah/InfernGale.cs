namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V14ZelessGah;

sealed class InfernGale(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback? _kb;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => Utils.ZeroOrOne(ref _kb);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.MechanicStatus && status.Extra == 0x1CA)
        {
            _kb = new(actor.Position.Quantized(), 20f, WorldState.FutureTime(5.6d));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.InfernGale)
        {
            _kb = null;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb is Knockback knockback)
        {
            var center = Arena.Center;
            ref var kb = ref knockback;
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var origin = kb.Origin;
                // rect intentionally slightly smaller to prevent sus knockbacks
                hints.AddForbiddenZone(p =>
                {
                    if ((p + 20f * (p - origin).Normalized()).InRect(center, 14f, 19f))
                    {
                        return 1f;
                    }
                    return default;
                }, kb.Activation);
            }
        }
    }
}
