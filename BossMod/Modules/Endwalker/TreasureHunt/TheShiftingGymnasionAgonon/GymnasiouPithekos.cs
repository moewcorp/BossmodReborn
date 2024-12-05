namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouPithekos;

public enum OID : uint
{
    Boss = 0x3D2B, //R=6
    BallOfLevin = 0x3E90, //R=1.7
    GymnasiouPithekosMikros = 0x3D2C, //R=4.2
    GymnasiouLyssa = 0x3D4E, //R=3.75
    GymnasticGarlic = 0x3D51, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticQueen = 0x3D53, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticEggplant = 0x3D50, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticOnion = 0x3D4F, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticTomato = 0x3D52, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 872, // Boss->player, no cast, single-target
    AutoAttack2 = 870, // GymnasiouPithekosMikros->player, no cast, single-target

    Thundercall = 32212, // Boss->location, 2.5s cast, range 3 circle
    LightningBolt = 32214, // Boss->self, 3.0s cast, single-target
    LightningBolt2 = 32215, // Helper->location, 3.0s cast, range 6 circle
    ThunderIV = 32213, // BallOfLevin->self, 7.0s cast, range 18 circle
    Spark = 32216, // Boss->self, 4.0s cast, range 14-30 donut

    RockThrow = 32217, // GymnasiouPithekosMikros->location, 3.0s cast, range 6 circle
    SweepingGouge = 32211, // Boss->player, 5.0s cast, single-target

    HeavySmash = 32317, // GymnasiouLyssa -> location 3.0s cast, range 6 circle
    PluckAndPrune = 32302, // GymnasticEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // GymnasticQueen->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // GymnasticTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // GymnasticGarlic->self, 3.5s cast, range 7 circle
    TearyTwirl = 32301, // GymnasticOnion->self, 3.5s cast, range 7 circle
    Telega = 9630 // Mandragoras/GymnasiouLyssa->self, no cast, single-target, bonus add disappear
}

public enum IconID : uint
{
    Thundercall = 111 // Thundercall marker
}

class Spark(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Spark), new AOEShapeDonut(14, 30));
class SweepingGouge(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.SweepingGouge));
class Thundercall(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Thundercall), 3);

class Thundercall2(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeCircle circle = new(18);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Thundercall)
            CurrentBaits.Add(new(actor, actor, circle));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Thundercall)
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
            hints.Add("Bait levinorb away!");
    }
}

class RockThrow(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.RockThrow), 6);
class LightningBolt2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.LightningBolt2), 6);
class ThunderIV(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThunderIV), new AOEShapeCircle(18));

abstract class Mandragoras(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(7));
class PluckAndPrune(BossModule module) : Mandragoras(module, AID.PluckAndPrune);
class TearyTwirl(BossModule module) : Mandragoras(module, AID.TearyTwirl);
class HeirloomScream(BossModule module) : Mandragoras(module, AID.HeirloomScream);
class PungentPirouette(BossModule module) : Mandragoras(module, AID.PungentPirouette);
class Pollen(BossModule module) : Mandragoras(module, AID.Pollen);

class HeavySmash(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HeavySmash), 6);

class GymnasiouPithekosStates : StateMachineBuilder
{
    public GymnasiouPithekosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Spark>()
            .ActivateOnEnter<Thundercall>()
            .ActivateOnEnter<Thundercall2>()
            .ActivateOnEnter<RockThrow>()
            .ActivateOnEnter<LightningBolt2>()
            .ActivateOnEnter<SweepingGouge>()
            .ActivateOnEnter<ThunderIV>()
            .ActivateOnEnter<HeavySmash>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .Raw.Update = () => module.Enemies(GymnasiouPithekos.All).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12001)]
public class GymnasiouPithekos(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GymnasticEggplant, (uint)OID.GymnasticGarlic, (uint)OID.GymnasticOnion, (uint)OID.GymnasticTomato,
    (uint)OID.GymnasticQueen, (uint)OID.GymnasiouLyssa];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.GymnasiouPithekosMikros, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.GymnasiouPithekosMikros));
        Arena.Actors(Enemies(bonusAdds), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.GymnasticOnion => 6,
                OID.GymnasticEggplant => 5,
                OID.GymnasticGarlic => 4,
                OID.GymnasticTomato => 3,
                OID.GymnasticQueen or OID.GymnasiouLyssa => 2,
                OID.GymnasiouPithekosMikros => 1,
                _ => 0
            };
        }
    }
}
