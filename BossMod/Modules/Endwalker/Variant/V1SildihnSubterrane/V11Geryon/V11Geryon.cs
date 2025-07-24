namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V11Geryon;

sealed class ColossalStrike(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ColossalStrike);
sealed class ColossalCharge(BossModule module) : Components.SimpleChargeAOEGroups(module, [(uint)AID.ColossalCharge1, (uint)AID.ColossalCharge2], 7f);

sealed class ColossalSlam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ColossalSlam, new AOEShapeCone(60f, 30f.Degrees()))
{
    private readonly Explosion _aoe = module.FindComponent<Explosion>()!;
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe.AOEs.Count == 5 ? [] : base.ActiveAOEs(slot, actor);
}

sealed class ColossalSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ColossalSwing, new AOEShapeCone(60f, 90f.Degrees()));

sealed class SubterraneanShudderColossalLaunch(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.SubterraneanShudder, (uint)AID.ColossalLaunch]);

sealed class RunawaySludge(BossModule module) : Components.VoidzoneAtCastTarget(module, 9f, (uint)AID.RunawaySludge, GetVoidzones, 0.2d)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.SludgeVoidzone);
        var count = enemies.Count;
        if (count == 0)
        {
            return [];
        }
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

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 868, NameID = 11442, SortOrder = 1)]
public sealed class V11Geryon(WorldState ws, Actor primary) : BossModule(ws, primary, primary.Position.X < -150f ? ArenaCenter1 : primary.Position.X > 100f ? new(-213f, 101f) : default, primary.Position.X < -150f ? new ArenaBoundsSquare(24.5f) : new ArenaBoundsSquare(19.5f))
{
    public static readonly WPos ArenaCenter1 = new(-213f, 101f);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.PowderKegRed));
        Arena.Actors(Enemies((uint)OID.PowderKegBlue));
    }
}
