namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V11Geryon;

sealed class Explosion(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(5);
    private static readonly AOEShapeCircle circle = new(15f);
    private static readonly AOEShapeDonut donut = new(3f, 17f);
    public bool Draw = true;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Draw ? CollectionsMarshal.AsSpan(_aoes) : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (Draw || _aoes.Count == 0)
        {
            return;
        }
        var id = spell.Action.ID;
        var delay = id switch
        {
            (uint)AID.ColossalSlam or (uint)AID.ColossalCharge1 or (uint)AID.ColossalCharge2 => 5.4d,
            (uint)AID.ColossalSwing => 5.6d,
            (uint)AID.ColossalLaunch => 6.7d,
            _ => default
        };
        if (delay != default)
        {
            Draw = true;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var len = aoes.Length;
            var act = Module.CastFinishAt(spell, delay);
            for (var i = 0; i < len; ++i)
            {
                ref var aoe = ref aoes[i];
                aoe.Activation = act;
            }
            var distance = id switch
            {
                (uint)AID.ColossalCharge1 => -9f,
                (uint)AID.ColossalCharge2 => 9f,
                _ => default
            };
            if (distance != default)
            {
                var pos = Module.PrimaryActor.Position;
                var dir = caster.Rotation.Round(90f).ToDirection();
                for (var i = 0; i < len; ++i)
                {
                    ref var aoe = ref aoes[i];
                    var origin = aoe.Origin;
                    if (origin.InRect(pos, dir, 40f, default, 7f))
                    {
                        // undo quantisation
                        var x = MathF.Round(origin.X);
                        var z = MathF.Round(origin.Z);
                        aoe.Origin = (new WPos(x, z) + distance * (caster.Rotation.Round(90f) + 90f.Degrees()).ToDirection()).Quantized();
                        return; // never seen more than one keg getting moved
                    }
                }
            }
            if (delay == 6.7d) // colossal launch, switch shapes
            {
                for (var i = 0; i < len; ++i)
                {
                    ref var aoe = ref aoes[i];
                    aoe.Shape = aoe.Shape == circle ? donut : circle;
                }
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        AOEShape? shape = actor.OID switch
        {
            (uint)OID.PowderKegRed => circle,
            (uint)OID.PowderKegBlue => donut,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, actor.Position, default, WorldState.FutureTime(14.3d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ExplosionDonut) // there is always at least one donut keg
        {
            _aoes.Clear();
            Draw = false;
        }
    }
}
