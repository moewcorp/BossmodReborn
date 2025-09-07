namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V11Geryon;

sealed class Intake(BossModule module) : Components.GenericKnockback(module, stopAtWall: true)
{
    private readonly List<Knockback> _kbs = new(4);
    private readonly Explosion _aoe = module.FindComponent<Explosion>()!;
    private static readonly AOEShapeRect rect = new(40f, 5f);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _aoe.AOEs.Count != 0 ? CollectionsMarshal.AsSpan(_kbs) : [];

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x09)
        {
            var originX = state switch
            {
                0x00200010u => 20f,
                0x00020001u => -20f,
                _ => default
            };
            if (originX != default)
            {
                var act = WorldState.FutureTime(14.2d);
                var angle = originX == 20f ? Angle.AnglesCardinals[0] : Angle.AnglesCardinals[3];
                for (var i = 0; i < 4; ++i)
                {
                    _kbs.Add(new(new WPos(originX, 15f - i * 10f).Quantized(), 25f, act, rect, angle, Kind.TowardsOrigin));
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Intake)
        {
            _kbs.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kbs.Count != 0)
        {
            // voidzones are treated as squares because we dont want to get knocked through them (there might be space behind them otherwise)
            ref var kb = ref _kbs.Ref(0);
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var kbs = _kbs;
                var aoes = CollectionsMarshal.AsSpan(_aoe.AOEs);
                var len = aoes.Length;
                var donutPos = new WPos[2];
                var index = 0;
                for (var i = 0; i < len; ++i)
                {
                    ref var aoe = ref aoes[i];
                    if (aoe.Shape == Explosion.Donut)
                    {
                        donutPos[index++] = aoe.Origin;
                        if (index == 2)
                        {
                            break;
                        }
                    }
                }

                var kbz = CollectionsMarshal.AsSpan(kbs);
                var lenKBz = kbz.Length;
                var rects = new (WPos origin, WDir rotation)[len];
                for (var i = 0; i < len; ++i)
                {
                    ref var aoe = ref aoes[i];
                    rects[i] = (aoe.Origin, aoe.Rotation.ToDirection());
                }
                // we don't need to clamp the distance since origin is outside of the arena
                // just need to get pulled into the general direction of the donut inner circle, still have about 4s to move into place after
                hints.AddForbiddenZone(new SDKnockbackWithWallsAwayFromOriginMultiAimIntoDonuts(rects, lenKBz, 40f, 5f, 25f, donutPos, 6f, len), act);
            }
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoe.AOEs);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Shape != Explosion.Donut && aoe.Check(pos)) // allow getting knocked into the donut since there is time to move after pull
            {
                return true;
            }
        }
        return false;
    }
}
