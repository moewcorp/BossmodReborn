namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V11Geryon;

sealed class ColossalStrike(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ColossalStrike);
sealed class ColossalCharge(BossModule module) : Components.SimpleChargeAOEGroups(module, [(uint)AID.ColossalCharge1, (uint)AID.ColossalCharge2], 7f);

sealed class ColossalSlam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ColossalSlam, new AOEShapeCone(60f, 30f.Degrees()))
{
    private readonly Explosion _aoe = module.FindComponent<Explosion>()!;
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe.Draw ? [] : base.ActiveAOEs(slot, actor);
}
class ColossalSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ColossalSwing, new AOEShapeCone(60f, 90f.Degrees()));

sealed class SubterraneanShudderColossalLaunch(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.SubterraneanShudder, (uint)AID.ColossalLaunch]);

sealed class RunawaySludge(BossModule module) : Components.VoidzoneAtCastTarget(module, 9f, (uint)AID.RunawaySludge, GetVoidzones, 0.2d)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.SludgeVoidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
            {
                voidzones[index++] = z;
            }
        }
        return voidzones[..index];
    }
}

class Shockwave(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> _sources = [];
    private static readonly AOEShapeRect _shape = new(40f, 40f);
    private readonly RunawaySludge _aoe = module.FindComponent<RunawaySludge>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(_sources);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Shockwave)
        {
            var act = Module.CastFinishAt(spell);
            // knockback rect always happens through center, so create two sources with origin at center looking orthogonally
            _sources.Add(new(Arena.Center, 15f, act, _shape, spell.Rotation + 90f.Degrees(), Kind.DirForward));
            _sources.Add(new(Arena.Center, 15f, act, _shape, spell.Rotation - 90f.Degrees(), Kind.DirForward));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Shockwave)
        {
            _sources.Clear();
            ++NumCasts;
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return !Module.InBounds(pos);
    }
}// in new(-233.00043f, 80.99979f), new(-193.00043f, 80.99980f), new(-193.00044f, 120.99984f), new(-233.00043f, 120.99985f)
//out new(-237.97943f, 76.00000f), new(-188.00031f, 76.03250f), new(-188.03873f, 126.00000f), new(-238.00031f, 125.98683f)
[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 868, NameID = 11442, SortOrder = 1)]
public sealed class V11Geryon(WorldState ws, Actor primary) : BossModule(ws, primary, primary.Position.X < -150f ? new(-213f, 101f) : primary.Position.X > 100f ? new(-213f, 101f) : default, new ArenaBoundsSquare(20f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.PowderKegRed));
        Arena.Actors(Enemies((uint)OID.PowderKegBlue));
    }
}
