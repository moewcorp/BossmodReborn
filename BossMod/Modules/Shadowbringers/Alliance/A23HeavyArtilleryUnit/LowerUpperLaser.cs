namespace BossMod.Shadowbringers.Alliance.A23HeavyArtilleryUnit;

sealed class UpperLaser(BossModule module) : Components.GenericAOEs(module)
{
    private readonly LowerLaser _aoe = module.FindComponent<LowerLaser>()!;
    private readonly List<AOEInstance> _aoes = new(9);
    private static readonly Angle a30 = 30f.Degrees();
    private static readonly AOEShapeDonutSector[] donutsectors = [new(6f, 16f, a30), new(16f, 23f, a30), new(23f, 30f, a30)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > 6 ? 6 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);

        var color = Colors.Danger;
        var max2 = _aoe.AOEs.Count != 0 ? 3 : count;
        for (var i = 0; i < max2; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i < 3 && count > 3)
            {
                aoe.Color = color;
            }
            aoe.Risky = true;
        }
        return aoes[..max];
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.ArenaFeatures && state == 0x00800100u)
        {
            var center = Arena.Center;
            var angle = Angle.FromDirection(actor.Position - center);
            // the lasers are placed poorly for some reason and can be off by 1-2°
            angle = (MathF.Round(angle.Deg / 60f) * 60f).Degrees();
            var loc = WPos.ClampToGrid(center);
            var act = WorldState.FutureTime(9.7d);
            for (var i = 0; i < 3; ++i)
            {
                _aoes.Add(new(donutsectors[i], center, angle, act.AddSeconds(i * 3d), risky: false));
            }
            if (_aoes.Count == 9)
            {
                _aoes.Sort((a, b) => a.Activation.CompareTo(b.Activation));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is >= (uint)AID.UpperLaserFirstSector1 and <= (uint)AID.UpperLaserFirstSector3 or >= (uint)AID.UpperLaserRepeatSector1 and <= (uint)AID.UpperLaserRepeatSector3)
        {
            if (++NumCasts % 15 == 0)
            {
                _aoes.RemoveRange(0, 3);
                NumCasts = 0;
            }
        }
    }
}

sealed class LowerLaser(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(3);
    private static readonly AOEShapeCone cone = new(40f, 30f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.ArenaFeatures && state == 0x00100020u)
        {
            var center = Arena.Center;
            var angle = Angle.FromDirection(actor.Position - center);
            // the lasers are placed poorly for some reason and can be off by 1-2°
            angle = (MathF.Round(angle.Deg / 60f) * 60f).Degrees();
            AOEs.Add(new(cone, WPos.ClampToGrid(center), angle, WorldState.FutureTime(9.7d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LowerLaserRepeatCone)
        {
            if (++NumCasts == 18)
            {
                AOEs.Clear();
                NumCasts = 0;
            }
        }
    }
}
