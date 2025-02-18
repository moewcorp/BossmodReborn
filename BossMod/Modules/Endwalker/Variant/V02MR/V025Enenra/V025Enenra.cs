namespace BossMod.Endwalker.VariantCriterion.V02MR.V025Enenra;

class PipeCleaner(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(60, 5), (uint)TetherID.PipeCleaner);
class Uplift(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Uplift), 6);
class Snuff(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.Snuff), new AOEShapeCircle(6), true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

class Smoldering(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Smoldering), 8, 8);
class FlagrantCombustion(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FlagrantCombustion));
class SmokeRings(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SmokeRings), 16);
class ClearingSmoke(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.ClearingSmoke), 16, stopAfterWall: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var source = Sources(slot, actor).FirstOrDefault();
        var component = Module.FindComponent<Smoldering>()?.ActiveAOEs(slot, actor)?.ToList();
        if (component != null && source != default)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 4), source.Activation);
    }
}

class StringRock(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(6), new AOEShapeDonut(6, 12), new AOEShapeDonut(12, 18), new AOEShapeDonut(18, 24)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.KiseruClamor)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.KiseruClamor => 0,
                AID.BedrockUplift1 => 1,
                AID.BedrockUplift2 => 2,
                AID.BedrockUplift3 => 3,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2));
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12393, SortOrder = 7)]
public class V025Enenra(WorldState ws, Actor primary) : BossModule(ws, primary, new(900, -900), StartingBounds)
{
    public static readonly ArenaBoundsCircle StartingBounds = new(20.5f);
    public static readonly ArenaBoundsCircle DefaultBounds = new(20);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.EnenraClone));
    }
}
