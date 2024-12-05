namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretWorm;

public enum OID : uint
{
    Boss = 0x3029, //R=6.0
    Bubble = 0x302A, //R=1.5
    SecretQueen = 0x3021, // R0.840, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    SecretGarlic = 0x301F, // R0.840, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    SecretTomato = 0x3020, // R0.840, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    SecretOnion = 0x301D, // R0.840, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    SecretEgg = 0x301E, // R0.840, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // Mandragoras->player, no cast, single-target

    Hydroburst = 21714, // Bubble->self, 1.0s cast, range 8 circle
    Hydrocannon = 21713, // Boss->location, 3.0s cast, range 8 circle
    AquaBurst = 21715, // Bubble->self, 5.0s cast, range 50 circle, damage fall off AOE, optimal range seems to be 10
    FreshwaterCannon = 21711, // Boss->self, 2.5s cast, range 46 width 4 rect
    BrineBreath = 21710, // Boss->player, 4.0s cast, single-target

    Pollen = 6452, // SecretQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // SecretOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // SecretTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // SecretEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // SecretGarlic->self, 3.5s cast, range 6+R circle
    Telega = 9630 // Mandragoras->self, no cast, single-target, bonus adds disappear
}

public enum IconID : uint
{
    Baitaway = 23 // player
}

class Hydrocannon(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Hydrocannon), 8);
class FreshwaterCannon(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FreshwaterCannon), new AOEShapeRect(46, 2));
class AquaBurst(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AquaBurst), new AOEShapeCircle(10));
class BrineBreath(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.BrineBreath));
class Hydroburst(BossModule module) : Components.PersistentVoidzone(module, 10, m => m.Enemies(OID.Bubble).Where(x => !x.IsDead && !(x.CastInfo != null && x.CastInfo.IsSpell(AID.AquaBurst))));

class Bubble(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeCircle circle = new(10);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Baitaway)
            CurrentBaits.Add(new(actor, actor, circle));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Hydrocannon)
            CurrentBaits.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (CurrentBaits.Any(x => x.Target == actor))
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 17.5f));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (CurrentBaits.Any(x => x.Target == actor))
            hints.Add("Bait bubble away!");
    }
}

abstract class Mandragoras(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(6.84f));
class PluckAndPrune(BossModule module) : Mandragoras(module, AID.PluckAndPrune);
class TearyTwirl(BossModule module) : Mandragoras(module, AID.TearyTwirl);
class HeirloomScream(BossModule module) : Mandragoras(module, AID.HeirloomScream);
class PungentPirouette(BossModule module) : Mandragoras(module, AID.PungentPirouette);
class Pollen(BossModule module) : Mandragoras(module, AID.Pollen);

class SecretWormStates : StateMachineBuilder
{
    public SecretWormStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FreshwaterCannon>()
            .ActivateOnEnter<AquaBurst>()
            .ActivateOnEnter<BrineBreath>()
            .ActivateOnEnter<Hydrocannon>()
            .ActivateOnEnter<Bubble>()
            .ActivateOnEnter<Hydroburst>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .Raw.Update = () => Module.WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9780)]
public class SecretWorm(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.SecretEgg).Concat(Enemies(OID.SecretTomato)).Concat(Enemies(OID.SecretQueen)).Concat(Enemies(OID.SecretGarlic)).Concat(Enemies(OID.SecretOnion)), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.SecretOnion => 5,
                OID.SecretEgg => 4,
                OID.SecretGarlic => 3,
                OID.SecretTomato => 2,
                OID.SecretQueen => 1,
                _ => 0
            };
        }
    }
}
