namespace BossMod.Dawntrail.Alliance.A11Prishe;

sealed class AuroralUppercut(BossModule module) : Components.GenericKnockback(module, ignoreImmunes: true)
{
    private Knockback? _kb;
    private RelSimplifiedComplexPolygon poly = new();

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb != null && actor.FindStatus(SID.Knockback) == null ? Utils.ZeroOrOne(ref _kb) : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var distance = spell.Action.ID switch
        {
            (uint)AID.AuroralUppercut1 => 12f,
            (uint)AID.AuroralUppercut2 => 25f,
            (uint)AID.AuroralUppercut3 => 38f,
            _ => default
        };
        if (distance != default)
        {
            _kb = new(Arena.Center, distance, Module.CastFinishAt(spell, 1.6d));
            if (Arena.Bounds is ArenaBoundsComplex arena)
            {
                poly = arena.poly.Offset(-1f); // pretend polygon is 1y smaller than real for less suspect knockbacks
            }
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (_kb != null && status.ID == (uint)SID.Knockback)
        {
            NumCasts = 1;
            _kb = null;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb is Knockback kb)
        {
            var center = Arena.Center;
            var distance = kb.Distance;
            var polygon = poly;
            hints.AddForbiddenZone(p =>
            {
                var offset = p - center;
                if (polygon.Contains(offset + distance * offset.Normalized()))
                    return 1f;
                return default;
            }, kb.Activation);
        }
    }
}

sealed class AuroralUppercutHint(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly Angle a45 = 45f.Degrees(), a135 = 135f.Degrees(), a44 = 44f.Degrees(), a13 = 12.5f.Degrees(), a59 = 59f.Degrees();
    private static readonly WPos center = A11Prishe.ArenaCenter;
    private AOEInstance? _aoe;
    private static readonly AOEShapeCustom hintENVC00020001KB25 = new([new DonutSegmentV(center, 4f, 10f, -144f.Degrees(), a44, 8), new DonutSegmentV(center, 4f, 10f, 36f.Degrees(), a44, 8)],
    [new ConeV(center, 10f, -a135, a13, 8), new ConeV(center, 10f, a45, a13, 8)]);
    private static readonly AOEShapeCustom hintENVC02000100KB25 = new([new DonutSegmentV(center, 4f, 10f, 126f.Degrees(), a44, 8), new DonutSegmentV(center, 4f, 10f, -54f.Degrees(), a44, 8)],
    [new ConeV(center, 10f, a135, a13, 8), new ConeV(center, 10f, -a45, a13, 8)]);
    private static readonly AOEShapeCustom hintENVC00020001KB38 = new([new ConeV(center, 5f, -a135, a13, 8), new ConeV(center, 5f, a45, a13, 8)]);
    private static readonly AOEShapeCustom hintENVC02000100KB38 = new([new ConeV(center, 5f, a135, a13, 8), new ConeV(center, 5f, -a45, a13, 8)]);
    private static readonly AOEShapeCustom hintENVC00020001KB12 = new([new ConeV(center, 5f, a135, a59, 8), new ConeV(center, 5f, -a45, a59, 8)]);
    private static readonly AOEShapeCustom hintENVC02000100KB12 = new([new ConeV(center, 5f, -a135, a59, 8), new ConeV(center, 5f, a45, a59, 8)]);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var distance = spell.Action.ID switch
        {
            (uint)AID.AuroralUppercut1 => 12f,
            (uint)AID.AuroralUppercut2 => 25f,
            (uint)AID.AuroralUppercut3 => 38f,
            _ => 0f
        };
        switch (distance)
        {
            case 12f:
                if (Arena.Bounds == ArenaChanges.ArenaENVC00020001)
                    SetAOE(hintENVC00020001KB12);
                else if (Arena.Bounds == ArenaChanges.ArenaENVC02000100)
                    SetAOE(hintENVC02000100KB12);
                break;
            case 25f:
                if (Arena.Bounds == ArenaChanges.ArenaENVC00020001)
                    SetAOE(hintENVC00020001KB25);
                else if (Arena.Bounds == ArenaChanges.ArenaENVC02000100)
                    SetAOE(hintENVC02000100KB25);
                break;
            case 38f:
                if (Arena.Bounds == ArenaChanges.ArenaENVC00020001)
                    SetAOE(hintENVC00020001KB38);
                else if (Arena.Bounds == ArenaChanges.ArenaENVC02000100)
                    SetAOE(hintENVC02000100KB38);
                break;
        }
        void SetAOE(AOEShapeCustom shape) => _aoe = new(shape, Arena.Center, default, Module.CastFinishAt(spell, 1.6d), Colors.SafeFromAOE);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (_aoe != null && status.ID == (uint)SID.Knockback)
            _aoe = null;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) { }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }
}
