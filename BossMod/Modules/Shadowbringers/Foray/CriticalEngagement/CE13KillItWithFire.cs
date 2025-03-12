namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE13KillItWithFire;

public enum OID : uint
{
    Boss = 0x2E2F, // R2.250, x1
    RottenMandragora = 0x2E30, // R1.050, spawn during fight
    Pheromones = 0x2E31, // R1.500, spawn during fight
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    Teleport = 20513, // Boss->location, no cast, single-target, teleport

    HarvestFestival = 20511, // Boss->self, 4.0s cast, single-target, visual (summon mandragoras)
    PheromonesVisual1 = 20512, // RottenMandragora->self, no cast, single-target, visual (???)
    PheromonesVisual2 = 20514, // RottenMandragora->location, no cast, single-target, visual (???)
    RancidPheromones = 20515, // RottenMandragora->self, no cast, single-target, visual (???)
    Heartbreak = 20516, // Pheromones->self, no cast, range 4 circle when pheromone is touched
    DeadLeaves = 20517, // Boss->self, 4.0s cast, range 30 circle, visual (recolors)
    TenderAnaphylaxis = 20518, // Helper->self, 4.0s cast, range 30 90-degree cone
    JealousAnaphylaxis = 20519, // Helper->self, 4.0s cast, range 30 90-degree cone
    AnaphylacticShock = 20520, // Helper->self, 4.0s cast, range 30 width 2 rect aoe (borders)
    SplashBomb = 20521, // Boss->self, 4.0s cast, single-target, visual (puddles)
    SplashBombAOE = 20522, // Helper->self, 4.0s cast, range 6 circle puddle
    SplashGrenade = 20523, // Boss->self, 5.0s cast, single-target, visual (stack)
    SplashGrenadeAOE = 20524, // Helper->players, 5.0s cast, range 6 circle stack
    PlayfulBreeze = 20525, // Boss->self, 4.0s cast, single-target, visual (raidwide)
    PlayfulBreezeAOE = 20526, // Helper->self, 4.0s cast, range 60 circle raidwide
    Budbutt = 20527 // Boss->player, 4.0s cast, single-target, tankbuster
}

public enum SID : uint
{
    TenderAnaphylaxis = 2301, // Helper->player, extra=0x0
    JealousAnaphylaxis = 2302, // Helper->player, extra=0x0
}

class Pheromones(BossModule module) : Components.PersistentVoidzone(module, 4f, GetVoidzones, 3f)
{
    private static List<Actor> GetVoidzones(BossModule module) => module.Enemies((uint)OID.Pheromones);
}

class DeadLeaves(BossModule module) : Components.GenericAOEs(module, default, "Go to different color!")
{
    private BitMask _tenderStatuses;
    private BitMask _jealousStatuses;
    private readonly List<AOEInstance> _tenderAOEs = new(2);
    private readonly List<AOEInstance> _jealousAOEs = new(2);

    private static readonly AOEShapeCone _shape = new(30f, 45f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_tenderStatuses[slot])
            return CollectionsMarshal.AsSpan(_tenderAOEs);
        else if (_jealousStatuses[slot])
            return CollectionsMarshal.AsSpan(_jealousAOEs);
        return [];
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.TenderAnaphylaxis:
                _tenderStatuses[Raid.FindSlot(actor.InstanceID)] = true;
                break;
            case (uint)SID.JealousAnaphylaxis:
                _jealousStatuses[Raid.FindSlot(actor.InstanceID)] = true;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.TenderAnaphylaxis:
                _tenderStatuses[Raid.FindSlot(actor.InstanceID)] = false;
                break;
            case (uint)SID.JealousAnaphylaxis:
                _jealousStatuses[Raid.FindSlot(actor.InstanceID)] = false;
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        CastersForAction(spell.Action)?.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        CastersForAction(spell.Action)?.Remove(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
    }

    private List<AOEInstance>? CastersForAction(ActionID action) => action.ID switch
    {
        (uint)AID.TenderAnaphylaxis => _tenderAOEs,
        (uint)AID.JealousAnaphylaxis => _jealousAOEs,
        _ => null
    };
}

class AnaphylacticShock(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AnaphylacticShock), new AOEShapeRect(30f, 1f));
class SplashBomb(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SplashBombAOE), 6f);
class SplashGrenade(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.SplashGrenadeAOE), 6f, 8, 24);
class PlayfulBreeze(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PlayfulBreeze));
class Budbutt(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Budbutt));

class CE13KillItWithFireStates : StateMachineBuilder
{
    public CE13KillItWithFireStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Pheromones>()
            .ActivateOnEnter<DeadLeaves>()
            .ActivateOnEnter<AnaphylacticShock>()
            .ActivateOnEnter<SplashBomb>()
            .ActivateOnEnter<SplashGrenade>()
            .ActivateOnEnter<PlayfulBreeze>()
            .ActivateOnEnter<Budbutt>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 735, NameID = 1)] // bnpcname=9391
public class CE13KillItWithFire(WorldState ws, Actor primary) : BossModule(ws, primary, new(-90f, 700f), new ArenaBoundsCircle(25f));
