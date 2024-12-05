namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

class Cloudsplitter(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.CloudsplitterAOE), new AOEShapeCircle(6), true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

class CriticalReaverRaidwide(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.CriticalReaverRaidwide));
class CriticalReaverEnrage(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.CriticalReaverEnrage));
class Meteor(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.Meteor));
class TachiGekko(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.TachiGekko));
class TachiKasha(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TachiKasha), new AOEShapeCircle(20));
class TachiYukikaze(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TachiYukikaze), new AOEShapeRect(50, 2.5f));
class Raiton(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Raiton));
class Utsusemi(BossModule module) : Components.StretchTetherSingle(module, (uint)TetherID.Utsusemi, 10, needToKite: true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.BossGK, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13640, SortOrder = 7)]
public class A13ArkAngels(WorldState ws, Actor primary) : BossModule(ws, primary, new(865, -820), new ArenaBoundsCircle(34.5f))
{
    public static readonly ArenaBoundsCircle DefaultBounds = new(25);
    public static readonly uint[] Bosses = [(uint)OID.BossHM, (uint)OID.BossEV, (uint)OID.BossTT, (uint)OID.BossMR, (uint)OID.BossGK];

    private Actor? _bossHM;
    private Actor? _bossEV;
    private Actor? _bossMR;
    private Actor? _bossTT;
    public Actor? BossHM() => _bossHM;
    public Actor? BossEV() => _bossEV;
    public Actor? BossMR() => _bossMR;
    public Actor? BossTT() => _bossTT;
    public Actor? BossGK() => PrimaryActor;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _bossHM ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.BossHM).FirstOrDefault() : null;
        _bossEV ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.BossEV).FirstOrDefault() : null;
        _bossMR ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.BossMR).FirstOrDefault() : null;
        _bossTT ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.BossTT).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        var comp = FindComponent<DecisiveBattle>();
        if (comp != null)
        {
            var slot = comp.AssignedBoss[pcSlot];
            if (slot != null)
                Arena.Actor(slot);
        }
        else if (Enemies(OID.ArkShield).Any(x => !x.IsDead))
            Arena.Actor(Enemies(OID.ArkShield)[0]);
        else
            Arena.Actors(Enemies(Bosses));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.FindStatus(SID.Invincibility) != null)
            {
                e.Priority = AIHints.Enemy.PriorityForbidFully;
                break;
            }
        }
    }
}
