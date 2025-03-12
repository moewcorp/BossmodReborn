namespace BossMod.Dawntrail.FATE.Ttokrrone;

class Landswallow(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect1 = new(38f, 13.5f);
    private static readonly AOEShapeRect rect2 = new(50f, 13.5f);
    private static readonly AOEShapeRect rect3 = new(68f, 13.5f);
    private static readonly AOEShapeRect rect4 = new(63f, 13.5f);
    private readonly List<AOEInstance> _aoes = new(6);
    private DateTime activation;

    private static readonly Dictionary<(Angle, Angle), (WPos, Angle, AOEShape)[]> chargeConfigs = new()
    {
        { (default, 135f.Degrees()), new (WPos, Angle, AOEShape)[]
            {
                (new(52.995f, -820.005f), -0.003f.Degrees(), rect1),
                (new(52.995f, -790.524f), 157.5f.Degrees(), rect4),
                (new(73.747f, -840.75f), -90.004f.Degrees(), rect2),
                (new(32.242f, -840.757f), 67.498f.Degrees(), rect4),
                (new(82.475f, -820.005f), -67.504f.Degrees(), rect4),
                (new(32.242f, -799.252f), 134.999f.Degrees(), rect3)
            }
        },
        { (default, -135f.Degrees()), new (WPos, Angle, AOEShape)[]
            {
                (new(52.995f, -820.005f), -0.003f.Degrees(), rect1),
                (new(52.995f, -790.524f), -180.Degrees(), rect3),
                (new(52.995f, -849.516f), -45.003f.Degrees(), rect2),
                (new(23.484f, -820.005f), 112.449f.Degrees(), rect4),
                (new(73.747f, -840.757f), -0.003f.Degrees(), rect2),
                (new(73.747f, -799.252f), -135.005f.Degrees(), rect3)
            }
        },
        { (-90f.Degrees(), -135f.Degrees()), new (WPos, Angle, AOEShape)[]
            {
                (new(52.995f, -820.005f), -90.004f.Degrees(), rect1),
                (new(23.484f, -820.005f), 134.999f.Degrees(), rect2),
                (new(52.995f, -849.516f), -0.003f.Degrees(), rect3),
                (new(52.995f, -790.524f), 157.5f.Degrees(), rect4),
                (new(73.747f, -840.757f), -0.003f.Degrees(), rect2),
                (new(73.747f, -799.252f), -135.005f.Degrees(), rect3)
            }
        },
        { (-90f.Degrees(), 135f.Degrees()), new (WPos, Angle, AOEShape)[]
            {
                (new(52.995f, -820.005f), -90.004f.Degrees(), rect1),
                (new(23.484f, -820.005f), 112.499f.Degrees(), rect4),
                (new(73.747f, -840.757f), -90.004f.Degrees(), rect2),
                (new(32.242f, -840.757f), 67.498f.Degrees(), rect4),
                (new(82.475f, -820.005f), -67.504f.Degrees(), rect4),
                (new(32.242f, -799.252f), 134.999f.Degrees(), rect3)
            }},
        { (90f.Degrees(), -45f.Degrees()), new (WPos, Angle, AOEShape)[]
            {
                (new(52.995f, -820.005f), 89.999f.Degrees(), rect1),
                (new(82.475f, -820.005f), -89.982f.Degrees(), rect3),
                (new(23.484f, -820.005f), 44.998f.Degrees(), rect2),
                (new(52.995f, -790.524f), -157.505f.Degrees(), rect4),
                (new(32.242f, -840.757f), 89.999f.Degrees(), rect2),
                (new(73.747f, -840.757f), -45.003f.Degrees(), rect3)
            }},
        { (90f.Degrees(), 45f.Degrees()), new (WPos, Angle, AOEShape)[]
            {
                (new(52.995f, -820.005f), 89.999f.Degrees(), rect1),
                (new(82.475f, -820.005f), -45.003f.Degrees(), rect2),
                (new(52.995f, -790.524f), -180.Degrees(), rect3),
                (new(52.995f, -849.516f), -22.503f.Degrees(), rect4),
                (new(32.242f, -799.252f), -180.Degrees(), rect2),
                (new(32.242f, -840.757f), 44.998f.Degrees(), rect3)
            }},
        { (180f.Degrees(), -45f.Degrees()), new (WPos, Angle, AOEShape)[]
            {
                (new(52.995f, -820.005f), 180.Degrees(), rect1),
                (new(52.995f, -849.516f), -22.503f.Degrees(), rect4),
                (new(32.242f, -799.252f), 89.999f.Degrees(), rect2),
                (new(73.747f, -799.252f), -112.504f.Degrees(), rect4),
                (new(23.484f, -820.005f), 112.499f.Degrees(), rect4),
                (new(73.747f, -840.757f), -45.003f.Degrees(), rect3)
            }},
        { (180f.Degrees(), 45f.Degrees()), new (WPos, Angle, AOEShape)[]
            {
                (new(52.995f, -820.005f), 179.995f.Degrees(), rect1),
                (new(52.995f, -849.516f), 22.498f.Degrees(), rect4),
                (new(73.747f, -799.252f), 180.Degrees(), rect2),
                (new(73.747f, -840.757f), -22.503f.Degrees(), rect4),
                (new(52.995f, -790.524f), -157.505f.Degrees(), rect4),
                (new(32.242f, -840.757f), 44.998f.Degrees(), rect3)
            }}
    };

    private Angle startRotation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Landswallow1)
        {
            startRotation = spell.Rotation;
            activation = Module.CastFinishAt(spell);
        }
        else if (spell.Action.ID == (uint)AID.LandSwallowTelegraph6)
        {
            var maxError = Angle.DegToRad;
            foreach (var config in chargeConfigs)
            {
                if (startRotation.AlmostEqual(config.Key.Item1, maxError) && spell.Rotation.AlmostEqual(config.Key.Item2, maxError))
                {
                    AddAOEs(config.Value);
                    break;
                }
            }
        }
    }

    private void AddAOEs((WPos, Angle, AOEShape)[] array)
    {
        var len = array.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var c = ref array[i];
            _aoes.Add(new(c.Item3, c.Item1, c.Item2, activation.AddSeconds(1.3d * _aoes.Count)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count == 0)
            return;
        switch (spell.Action.ID)
        {
            case (uint)AID.Landswallow1:
            case (uint)AID.Landswallow2:
            case (uint)AID.Landswallow3:
            case (uint)AID.Landswallow4:
                _aoes.RemoveAt(0);
                break;
        }
    }
}
