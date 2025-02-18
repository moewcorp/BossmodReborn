namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

class DarkNebula(BossModule module) : Components.Knockback(module)
{
    private const float Length = 4f;
    private const float HalfWidth = 1.75f;

    public readonly List<Actor> Casters = new(4);

    private static readonly Angle a90 = 90f.Degrees();
    private static readonly List<(Predicate<WPos> Matcher, int[] CircleIndices, WDir Directions)> PositionMatchers =
        [
        (pos => pos == new WPos(142f, 792f), [3, 1], 45f.Degrees().ToDirection()),  // 135°
        (pos => pos == new WPos(158f, 792f), [0, 3], -135f.Degrees().ToDirection()),  // 45°
        (pos => pos == new WPos(158f, 808f), [2, 0], -45f.Degrees().ToDirection()),  // -45°
        (pos => pos.AlmostEqual(new WPos(142f, 808f), 1), [1, 2], 135f.Degrees().ToDirection())  // -135°
    ];

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var sources = new Source[max];
        for (var i = 0; i < max; ++i)
        {
            var caster = Casters[i];
            var dir = caster.CastInfo?.Rotation ?? caster.Rotation;
            var kind = dir.ToDirection().OrthoL().Dot(actor.Position - caster.Position) >= 0 ? Kind.DirLeft : Kind.DirRight;
            sources[i] = new(caster.Position, 20f, Module.CastFinishAt(caster.CastInfo), null, dir, kind);
        }
        return sources;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DarkNebulaShort or (uint)AID.DarkNebulaLong)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DarkNebulaShort or (uint)AID.DarkNebulaLong)
        {
            ++NumCasts;
            Casters.Remove(caster);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;

        var forbidden = new List<Func<WPos, float>>(2);
        var caster0 = Casters[0];
        static Func<WPos, float> CreateForbiddenZone(int circleIndex, WDir dir)
         => ShapeDistance.InvertedRect(A14ShadowLord.Circles[circleIndex].Center, dir, Length, 0f, HalfWidth);

        var mapping = PositionMatchers.FirstOrDefault(m => m.Matcher(caster0.Position));

        if (Casters.Count == 1)
        {
            for (var i = 0; i < 2; ++i)
                forbidden.Add(CreateForbiddenZone(mapping.CircleIndices[i], mapping.Directions));
        }
        else
        {
            var caster1 = Casters[1];
            var rotationMatch = caster0.Rotation.AlmostEqual(caster1.Rotation + a90, Angle.DegToRad);
            var circleIndex = rotationMatch ? mapping.CircleIndices[0] : mapping.CircleIndices[1];
            forbidden.Add(CreateForbiddenZone(circleIndex, mapping.Directions));
        }

        hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), Sources(slot, actor).FirstOrDefault().Activation);
    }
}
