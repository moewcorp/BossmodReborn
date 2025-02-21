namespace BossMod.Endwalker.VariantCriterion.V02MR.V025Enenra;

class PipeCleaner(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(60f, 5f), (uint)TetherID.PipeCleaner);
class Uplift(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Uplift), 6f);
class Snuff(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.Snuff), new AOEShapeCircle(6f), true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count != 0)
            hints.Add("Tankbuster cleave");
    }
}

class Smoldering(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Smoldering), 8f, 8);
class FlagrantCombustion(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FlagrantCombustion));
class SmokeRings(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SmokeRings), 16f);
class ClearingSmoke(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.ClearingSmoke), 16f, stopAfterWall: true)
{
    private readonly Smoldering _aoe = module.FindComponent<Smoldering>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var source = Casters.Count != 0 ? Casters[0] : null;
        if (_aoe.Casters.Count != 0 && source != null)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 4f), Module.CastFinishAt(source.CastInfo));
    }
}

class StringRock(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(6), new AOEShapeDonut(6, 12), new AOEShapeDonut(12, 18), new AOEShapeDonut(18, 24)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.KiseruClamor)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.KiseruClamor => 0,
                (uint)AID.BedrockUplift1 => 1,
                (uint)AID.BedrockUplift2 => 2,
                (uint)AID.BedrockUplift3 => 3,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12393, SortOrder = 7)]
public class V025Enenra(WorldState ws, Actor primary) : BossModule(ws, primary, new(900f, -900f), StartingBounds)
{
    public static readonly ArenaBoundsCircle StartingBounds = new(20.5f);
    public static readonly ArenaBoundsCircle DefaultBounds = new(20f);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.EnenraClone));
    }
}
