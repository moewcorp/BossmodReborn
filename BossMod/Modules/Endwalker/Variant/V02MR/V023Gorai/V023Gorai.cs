namespace BossMod.Endwalker.VariantCriterion.V02MR.V023Gorai;

class Unenlightenment(BossModule module) : Components.RaidwideCastDelay(module, ActionID.MakeSpell(AID.Unenlightenment), ActionID.MakeSpell(AID.UnenlightenmentAOE), 0.5f);
class SpikeOfFlameAOE(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SpikeOfFlameAOE), 5);

class StringSnap(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.StringSnap1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.StringSnap1 => 0,
                AID.StringSnap2 => 1,
                AID.StringSnap3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2));
        }
    }
}

class TorchingTorment(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.Tankbuster, ActionID.MakeSpell(AID.TorchingTorment), 5.9f, true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

class PureShock(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PureShock));
class HumbleHammer(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HumbleHammer), 3);
class FightingSpirits(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FightingSpirits));
class BiwaBreaker(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BiwaBreakerFirst), "Raidwide x5");

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12373, SortOrder = 4)]
public class V023Gorai(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChange.ArenaCenter, ArenaChange.StartingBounds)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.ShishuWhiteBaboon));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.ShishuWhiteBaboon => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
