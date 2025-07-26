namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V13Gladiator;

sealed class SunderedRemains(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SunderedRemains, 10f, 8);
sealed class Landing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Landing, 20f);
sealed class GoldenFlame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GoldenFlame, new AOEShapeRect(60f, 5f));
sealed class SculptorsPassion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SculptorsPassion, new AOEShapeRect(60f, 4f));
sealed class RackAndRuin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RackAndRuin, new AOEShapeRect(40f, 2.5f), 8);
sealed class MightySmite(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.MightySmite);

sealed class FlashOfSteel(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.FlashOfSteel1, (uint)AID.FlashOfSteel2]);

sealed class ShatteringSteelMeteor(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.ShatteringSteel, 60f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction && Module.Enemies((uint)OID.WhirlwindUpdraft).Count != 0) // depending on path this can cast can happen with other mechs
        {
            base.OnCastStarted(caster, spell);
        }
    }

    public override ReadOnlySpan<Actor> BlockerActors()
    {
        var boulders = Module.Enemies((uint)OID.AntiqueBoulder);
        var count = boulders.Count;
        if (count == 0)
        {
            return [];
        }
        var actors = new List<Actor>(1);
        for (var i = 0; i < count; ++i)
        {
            var b = boulders[i];
            if (b.ModelState.AnimState2 != 1)
            {
                actors.Add(b);
                break;
            }
        }
        return CollectionsMarshal.AsSpan(actors);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", PrimaryActorOID = (uint)OID.Boss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 868, NameID = 11387, SortOrder = 3)]
public sealed class V13Gladiator(WorldState ws, Actor primary) : BossModule(ws, primary, new(-35f, -271f), new ArenaBoundsSquare(19.5f));
