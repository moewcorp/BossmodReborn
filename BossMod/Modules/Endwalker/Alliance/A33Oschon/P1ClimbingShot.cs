namespace BossMod.Endwalker.Alliance.A33Oschon;

class P1ClimbingShot(BossModule module) : Components.GenericKnockback(module)
{
    private P1Downhill? _downhill = module.FindComponent<P1Downhill>();
    private Knockback? _knockback;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => Utils.ZeroOrOne(ref _knockback);

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        _downhill ??= Module.FindComponent<P1Downhill>();
        if (_downhill != null)
        {
            var aoes = _downhill.ActiveAOEs(slot, actor);
            var len = aoes.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var aoe = ref aoes[i];
                if (aoe.Check(pos))
                {
                    return true;
                }
            }
        }
        return !Module.InBounds(pos);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ClimbingShot1 or (uint)AID.ClimbingShot2 or (uint)AID.ClimbingShot3 or (uint)AID.ClimbingShot4)
        {
            _knockback = new(spell.LocXZ, 20f, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ClimbingShot1 or (uint)AID.ClimbingShot2 or (uint)AID.ClimbingShot3 or (uint)AID.ClimbingShot4)
        {
            _knockback = null;
        }
    }
}
