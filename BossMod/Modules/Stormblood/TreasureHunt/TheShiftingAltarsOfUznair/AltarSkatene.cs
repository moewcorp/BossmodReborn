namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarSkatene;

public enum OID : uint
{
    Boss = 0x2535, //R=4.48
    AltarDeepeye = 0x255E, //R=0.9
    AltarMatanga = 0x2545, // R3.42
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // AltarDeepeye->player, no cast, single-target
    AutoAttack3 = 6499, // BossAdd->player, no cast, single-target

    RecklessAbandon = 13311, // Boss->player, 3.0s cast, single-target
    Tornado = 13309, // Boss->location, 3.0s cast, range 6 circle
    VoidCall = 13312, // Boss->self, 3.5s cast, single-target
    Chirp = 13310, // Boss->self, 3.5s cast, range 8+R circle

    MatangaActivate = 9636, // AltarMatanga->self, no cast, single-target
    Spin = 8599, // AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // AltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // AltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 963, // AltarMatanga->self, no cast, single-target, bonus adds disappear
}

class Chirp(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Chirp), new AOEShapeCircle(12.48f));
class Tornado(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Tornado), 6);
class VoidCall(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.VoidCall), "Calls adds");
class RecklessAbandon(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.RecklessAbandon));

class Hurl(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Hurl), 6);
class Spin(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCone(9.42f, 60.Degrees()), (uint)OID.AltarMatanga);
class RaucousScritch(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RaucousScritch), new AOEShapeCone(8.42f, 30.Degrees()));

class AltarSkateneStates : StateMachineBuilder
{
    public AltarSkateneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Chirp>()
            .ActivateOnEnter<Tornado>()
            .ActivateOnEnter<VoidCall>()
            .ActivateOnEnter<RecklessAbandon>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => Module.WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7587)]
public class AltarSkatene(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.AltarDeepeye));
        Arena.Actors(Enemies(OID.AltarMatanga), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.AltarMatanga => 2,
                OID.AltarDeepeye => 1,
                _ => 0
            };
        }
    }
}
