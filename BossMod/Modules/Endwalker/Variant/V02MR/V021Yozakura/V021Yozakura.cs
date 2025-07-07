namespace BossMod.Endwalker.VariantCriterion.V02MR.V021Yozakura;

sealed class GloryNeverlasting(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.GloryNeverlasting);
sealed class ArtOfTheFireblossom(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArtOfTheFireblossom, 9f);
sealed class ArtOfTheWindblossom(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArtOfTheWindblossom, new AOEShapeDonut(5f, 60f));
sealed class KugeRantsuiOkaRanman(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.KugeRantsui, (uint)AID.OkaRanman]);
sealed class LevinblossomStrike(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LevinblossomStrike, 3f);

sealed class DriftingPetals(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.DriftingPetals, 15f, ignoreImmunes: true)
{
    private readonly Mudrain _aoe1 = module.FindComponent<Mudrain>()!;
    private readonly Witherwind _aoe2 = module.FindComponent<Witherwind>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var source = Casters[0];
            var component = _aoe1.Sources(Module).ToList();
            var forbidden = new List<Func<WPos, float>>
            {
                ShapeDistance.InvertedCircle(source.Position, 5f)
            };
            var count = component.Count;
            for (var i = 0; i < count; ++i)
            {
                forbidden.Add(ShapeDistance.Cone(source.Position, 20f, Angle.FromDirection(component[i].Position - Arena.Center), 20f.Degrees()));
            }
            hints.AddForbiddenZone(ShapeDistance.Union(forbidden), Module.CastFinishAt(source.CastInfo));
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe1.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        var aoes2 = _aoe2.ActiveAOEs(slot, actor);
        var len2 = aoes2.Length;
        for (var i = 0; i < len2; ++i)
        {
            ref readonly var aoe = ref aoes2[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }
}

sealed class Mudrain(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, (uint)AID.Mudrain, GetVoidzones, 0.7d)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.MudVoidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7u)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}
sealed class Icebloom(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Icebloom, 6);
sealed class Shadowflight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shadowflight, new AOEShapeRect(10f, 3f));
sealed class MudPie(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MudPie, new AOEShapeRect(60f, 3f));
sealed class FireblossomFlare(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FireblossomFlare, 6f);
sealed class ArtOfTheFluff(BossModule module) : Components.CastGazes(module, [(uint)AID.ArtOfTheFluff1, (uint)AID.ArtOfTheFluff2]);
sealed class TatamiGaeshi(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TatamiGaeshi, new AOEShapeRect(40f, 5f));
sealed class AccursedSeedling(BossModule module) : Components.Voidzone(module, 4f, GetSeedlings)
{
    private static List<Actor> GetSeedlings(BossModule module) => module.Enemies((uint)OID.AccursedSeedling);
}

sealed class RootArrangement(BossModule module) : Components.StandardChasingAOEs(module, 4f, (uint)AID.RockRootArrangementFirst, (uint)AID.RockRootArrangementRest, 4, 1, 4, true, (uint)IconID.ChasingAOE)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Targets[slot])
        {
            hints.AddForbiddenZone(ShapeDistance.Rect(Arena.Center + new WDir(19f, default), Arena.Center + new WDir(-19f, default), 20f), Activation);
        }
    }
}

sealed class Witherwind(BossModule module) : Components.Voidzone(module, 3f, GetWhirlwind, 20f)
{
    private static List<Actor> GetWhirlwind(BossModule module) => module.Enemies((uint)OID.AutumnalTempest);
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12325, SortOrder = 1)]
public sealed class V021Yozakura(WorldState ws, Actor primary) : BossModule(ws, primary, primary.Position.X < -700f ? ArenaCenter1 : primary.Position.X > 700f ? ArenaCenter2 : ArenaCenter3, primary.Position.X < -700f ? StartingBounds : primary.Position.X > 700f ? DefaultBounds2 : StartingBounds)
{
    public static readonly WPos ArenaCenter1 = new(-775f, 16f);
    public static readonly WPos ArenaCenter2 = new(737f, 220f);
    public static readonly WPos ArenaCenter3 = new(47f, 93f);
    public static readonly ArenaBoundsSquare StartingBounds = new(22.5f);
    public static readonly ArenaBoundsSquare DefaultBounds1 = new(20f);
    public static readonly ArenaBoundsSquare DefaultBounds2 = new(19.5f);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.LivingGaol));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.LivingGaol => 1,
                _ => 0
            };
        }
    }
}
