using BossMod.Endwalker.Quest.MSQ.AsTheHeavensBurn.P2TerminusLacerator;

namespace BossMod.Endwalker.Quest.MSQ.AsTheHeavensBurn.P3TerminusVanquisher;

public enum OID : uint
{
    Boss = 0x35EE, // R6.0
    TerminusVanquisher = 0x35EF, // R4.2
    SparkSphere = 0x35F0, // R1.3
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 872, // Boss->tank, no cast, single-target
    AutoAttack2 = 27028, // TerminusVanquisher->tank, no cast, single-target
    TheBlackDeath = 27010, // Boss->self, no cast, range 25 120-degree cone
    Teleport = 27033, // TerminusVanquisher->location, no cast, single-target

    BlackStarVisual = 27011, // Boss->self, 5.0s cast, single-target
    BlackStar = 27012, // Helper->location, 6.0s cast, range 40 circle

    DeadlyImpactVisual = 27013, // Boss->self, 4.0s cast, single-target
    DeadlyImpact = 27014, // Helper->location, 7.0s cast, range 10 circle
    ForcefulImpactAOE = 26239, // TerminusVanquisher->location, 5.0s cast, range 7 circle
    ForcefulImpactKB = 27030, // Helper->self, 5.6s cast, range 20 circle

    MutableLawsVisual = 27039, // TerminusVanquisher->self, 4.0s cast, single-target
    MutableLawsBig = 27041, // Helper->location, 10.0s cast, range 6 circle
    MutableLawsSmall = 27040, // Helper->location, 10.0s cast, range 6 circle
    AccursedTongueVisual = 27037, // TerminusVanquisher->self, 4.0s cast, single-target
    AccursedTongue = 27038, // Helper->all, 5.0s cast, range 6 circle, spread

    Thundercall = 27034, // TerminusVanquisher->self, 4.0s cast, single-target
    Shock = 27035, // SparkSphere->self, 5.0s cast, range 10 circle
    Depress = 27036, // TerminusVanquisher->tank, 5.0s cast, range 7 circle
    ForcefulImpact = 27029, // TerminusVanquisher->location, 5.0s cast, range 7 circle

    WaveOfLoathing = 27032, // TerminusVanquisher->self, 5.0s cast, range 40 circle
    ForceOfLoathing = 27031 // TerminusVanquisher->self, no cast, range 10 120-degree cone
}

class TheBlackDeath(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.TheBlackDeath), new AOEShapeCone(25, 60.Degrees()), [(uint)OID.Boss], activeWhileCasting: false);
class ForceOfLoathing(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.ForceOfLoathing), new AOEShapeCone(10, 60.Degrees()), [(uint)OID.TerminusVanquisher], activeWhileCasting: false);
class DeadlyImpact(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.DeadlyImpact), 10, 6);
class BlackStar(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BlackStar));

class ForcefulImpact(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ForcefulImpactAOE), 7);
class ForcefulImpactKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.ForcefulImpactKB), 10, stopAtWall: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0 && Casters[0] is Actor c)
            hints.PredictedDamage.Add((WorldState.Party.WithSlot(false, true).Mask(), Module.CastFinishAt(c.CastInfo)));
    }
}
class MutableLaws1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MutableLawsBig), 15);
class MutableLaws2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MutableLawsSmall), 6);
class AccursedTongue(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.AccursedTongue), 6);
class ForcefulImpact2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ForcefulImpact), 7);
class Shock(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Shock), 10, 6);
class Depress(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.Depress), 7);

class TerminusVanquisherStates : StateMachineBuilder
{
    private readonly TerminusVanquisher _module;

    public TerminusVanquisherStates(TerminusVanquisher module) : base(module)
    {
        _module = module;

        TrivialPhase()
            .ActivateOnEnter<TheBlackDeath>()
            .ActivateOnEnter<DeadlyImpact>()
            .ActivateOnEnter<BlackStar>();
        TrivialPhase(1)
            .ActivateOnEnter<ForceOfLoathing>()
            .ActivateOnEnter<ForcefulImpact>()
            .ActivateOnEnter<ForcefulImpactKB>()
            .ActivateOnEnter<MutableLaws1>()
            .ActivateOnEnter<MutableLaws2>()
            .ActivateOnEnter<AccursedTongue>()
            .ActivateOnEnter<ForcefulImpact2>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<Depress>()
            .Raw.Update = () => _module.BossP2?.IsDeadOrDestroyed ?? false;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 804, NameID = 10935)]
public class TerminusVanquisher(WorldState ws, Actor primary) : BossModule(ws, primary, TerminusLacerator.ArenaBounds.Center, TerminusLacerator.ArenaBounds)
{
    public Actor? BossP2 => Enemies(OID.TerminusVanquisher)[0];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(BossP2);
    }
}

