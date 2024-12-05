namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.TheGreatGoldWhisker;

public enum OID : uint
{
    Boss = 0x2541, //R=2.4
    GoldWhisker = 0x2544, // R0.54
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/GoldWhisker->player, no cast, single-target

    TripleTrident = 13364, // Boss->players, 3.0s cast, single-target
    Tingle = 13365, // Boss->self, 4.0s cast, range 10+R circle
    FishOutOfWater = 13366, // Boss->self, 3.0s cast, single-target
    Telega = 9630 // GoldWhisker->self, no cast, single-target
}

class TripleTrident(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.TripleTrident));
class FishOutOfWater(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.FishOutOfWater), "Spawns adds");
class Tingle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Tingle), new AOEShapeCircle(12.4f));

class TheGreatGoldWhiskerStates : StateMachineBuilder
{
    public TheGreatGoldWhiskerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TripleTrident>()
            .ActivateOnEnter<FishOutOfWater>()
            .ActivateOnEnter<Tingle>()
            .Raw.Update = () => Module.WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7599)]
public class TheGreatGoldWhisker(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.GoldWhisker), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.GoldWhisker => 1,
                _ => 0
            };
        }
    }
}
