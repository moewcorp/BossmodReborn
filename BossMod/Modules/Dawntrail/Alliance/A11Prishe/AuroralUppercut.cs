namespace BossMod.Dawntrail.Alliance.A11Prishe;

class AuroralUppercut(BossModule module) : Components.Knockback(module, ignoreImmunes: true)
{
    private Source? _source;
    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_source);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var distance = (AID)spell.Action.ID switch
        {
            AID.AuroralUppercut1 => 12,
            AID.AuroralUppercut2 => 25,
            AID.AuroralUppercut3 => 38,
            _ => 0
        };
        if (distance > 0)
            _source = new(Arena.Center, distance, Module.CastFinishAt(spell, 1.6f));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (_source != null && (SID)status.ID == SID.Knockback)
        {
            NumCasts = 1;
            _source = null;
        }
    }
}

class AuroralUppercutHint(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly Angle a45 = 45.Degrees(), a135 = 135.Degrees(), a44 = 44.Degrees(), a10 = 10.Degrees(), a59 = 59.Degrees();
    private static readonly WPos center = A11Prishe.ArenaCenter;
    private AOEInstance? _aoe;
    private static readonly AOEShapeCustom hintENVC00020001KB25 = new([new DonutSegmentHA(center, 4, 10, -144.Degrees(), a44), new DonutSegmentHA(center, 4, 10, 36.Degrees(), a44)],
    [new ConeHA(center, 10, -a135, a10), new ConeHA(center, 10, a45, a10)], InvertForbiddenZone: true);
    private static readonly AOEShapeCustom hintENVC02000100KB25 = new([new DonutSegmentHA(center, 4, 10, 126.Degrees(), a44), new DonutSegmentHA(center, 4, 10, -54.Degrees(), a44)],
    [new ConeHA(center, 10, a135, a10), new ConeHA(center, 10, -a45, a10)], InvertForbiddenZone: true);
    private static readonly AOEShapeCustom hintENVC00020001KB38 = new([new ConeHA(center, 5, -a135, a10), new ConeHA(center, 5, a45, a10)], InvertForbiddenZone: true);
    private static readonly AOEShapeCustom hintENVC02000100KB38 = new([new ConeHA(center, 5, a135, a10), new ConeHA(center, 5, -a45, a10)], InvertForbiddenZone: true);
    private static readonly AOEShapeCustom hintENVC00020001KB12 = new([new ConeHA(center, 5, a135, a59), new ConeHA(center, 5, -a45, a59),
    new ConeV(ArenaChanges.MiddleENVC00020001[0].Center + new WDir(-9, -9), 3, -a135, a45, 3),
    new ConeV(ArenaChanges.MiddleENVC00020001[1].Center + new WDir(9, 9), 3, a45, a45, 3),
    new Rectangle(center + new WDir(-3, -15), 5, 1), new Rectangle(center + new WDir(3, 15), 5, 1)], InvertForbiddenZone: true);
    private static readonly AOEShapeCustom hintENVC02000100KB12 = new([new ConeHA(center, 5, -a135, a59), new ConeHA(center, 5, a45, a59),
    new ConeV(ArenaChanges.MiddleENVC02000100[0].Center + new WDir(9, -9), 3, -a45, a45, 3),
    new ConeV(ArenaChanges.MiddleENVC02000100[1].Center + new WDir(-9, 9), 3, a135, a45, 3),
    new Rectangle(center + new WDir(-15, 3), 1, 5), new Rectangle(center + new WDir(15, -3), 1, 5)], InvertForbiddenZone: true);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var distance = (AID)spell.Action.ID switch
        {
            AID.AuroralUppercut1 => 12,
            AID.AuroralUppercut2 => 25,
            AID.AuroralUppercut3 => 38,
            _ => 0
        };
        switch (distance)
        {
            case 12:
                if (Arena.Bounds == ArenaChanges.ArenaENVC00020001)
                    SetAOE(hintENVC00020001KB12, spell);
                else if (Arena.Bounds == ArenaChanges.ArenaENVC02000100)
                    SetAOE(hintENVC02000100KB12, spell);
                break;
            case 25:
                if (Arena.Bounds == ArenaChanges.ArenaENVC00020001)
                    SetAOE(hintENVC00020001KB25, spell);
                else if (Arena.Bounds == ArenaChanges.ArenaENVC02000100)
                    SetAOE(hintENVC02000100KB25, spell);
                break;
            case 38:
                if (Arena.Bounds == ArenaChanges.ArenaENVC00020001)
                    SetAOE(hintENVC00020001KB38, spell);
                else if (Arena.Bounds == ArenaChanges.ArenaENVC02000100)
                    SetAOE(hintENVC02000100KB38, spell);
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (_aoe != null && (SID)status.ID == SID.Knockback)
            _aoe = null;
    }

    private void SetAOE(AOEShapeCustom shape, ActorCastInfo spell)
    {
        _aoe = new(shape, Arena.Center, default, Module.CastFinishAt(spell, 1.6f), Colors.SafeFromAOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) { }
}
