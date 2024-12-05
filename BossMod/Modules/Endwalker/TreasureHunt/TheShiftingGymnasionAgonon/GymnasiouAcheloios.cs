namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouAcheloios;

public enum OID : uint
{
    Boss = 0x3D3E, //R=4.0
    GymnasiouSouchos = 0x3D3F, //R=2.7
    GymnasticGarlic = 0x3D51, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticQueen = 0x3D53, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticEggplant = 0x3D50, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticOnion = 0x3D4F, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticTomato = 0x3D52, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasiouLampas = 0x3D4D, //R=2.001, bonus loot adds
    GymnasiouLyssa = 0x3D4E, //R=3.75, bonus loot adds
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/GymnasiouSouchos/GymnasiouLyssa->player, no cast, single-target

    DoubleHammerVisualLR = 32284, // Boss->self, 4.2s cast, single-target
    DoubleHammerVisualRL = 32281, // Boss->self, 4.2s cast, single-target
    RightHammerVisual = 32282, // Boss->self, 0.5s cast, single-target
    LeftHammerVisual = 32285, // Boss->self, 0.5s cast, single-target
    QuadrupleHammerVisualCCW = 32280, // Boss->self, 4.2s cast, single-target
    QuadrupleHammerVisualCW = 32283, // Boss->self, 4.2s cast, single-target
    DoubleHammerFirst = 32859, // Helper->self, 5.0s cast, range 30 180-degree cone
    QuadrupleHammerFirst = 32858, // Helper->self, 5.0s cast, range 30 180-degree cone
    RightHammer = 32860, // Helper->self, 1.0s cast, range 30 180-degree cone
    LeftHammer = 32861, // BossHelper->self, 1.0s cast, range 30 180-degree cone

    TailSwing = 32279, // Boss->self, 3.5s cast, range 13 circle
    CriticalBite = 32286, // GymnasiouSouchos->self, 3.0s cast, range 10 120-degree cone
    DeadlyHold = 32275, // Boss->player, 5.0s cast, single-target
    EarthbreakVisual = 32277, // Boss->self, 2.1s cast, single-target
    Earthbreak = 32278, // Helper->location, 3.0s cast, range 5 circle
    VolcanicHowl = 32276, // Boss->self, 5.0s cast, range 40 circle

    PluckAndPrune = 32302, // GymnasticEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // GymnasticQueen->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // GymnasticTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // GymnasticGarlic->self, 3.5s cast, range 7 circle
    TearyTwirl = 32301, // GymnasticOnion->self, 3.5s cast, range 7 circle
    HeavySmash = 32317, // GymnasiouLyssa->location, 3.0s cast, range 6 circle
    Telega = 9630, // Mandragoras/GymnasiouLyssa->self, no cast, single-target, bonus add disappear
}

class DoubleHammer(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public static readonly AOEShapeCone Cone = new(30, 90.Degrees());
    private static readonly Angle[] angles = [89.999f.Degrees(), -90.004f.Degrees()];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count > 0)
            yield return _aoes[0] with { Color = Colors.Danger };
        if (count > 1)
            yield return _aoes[1] with { Risky = false };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DoubleHammerVisualLR:
                AddAOEs(caster, spell, angles[1], angles[0]);
                break;
            case AID.DoubleHammerVisualRL:
                AddAOEs(caster, spell, angles[0], angles[1]);
                break;
        }
    }

    private void AddAOEs(Actor caster, ActorCastInfo spell, Angle first, Angle second)
    {
        _aoes.Add(new(Cone, caster.Position, first, Module.CastFinishAt(spell, 0.8f)));
        _aoes.Add(new(Cone, caster.Position, second, Module.CastFinishAt(spell, 3.7f)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && (AID)spell.Action.ID is AID.LeftHammer or AID.RightHammer or AID.DoubleHammerFirst)
            _aoes.RemoveAt(0);
    }
}

class QuadrupleHammer(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;
    private Angle _rotation;
    private DateTime _activation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.QuadrupleHammerVisualCW: // note that boss switches hands, so CW rotation means CCW aoe sequence and vice versa
                _increment = 90.Degrees();
                break;
            case AID.QuadrupleHammerVisualCCW:
                _increment = -90.Degrees();
                break;
            case AID.QuadrupleHammerFirst:
                _rotation = spell.Rotation;
                break;
        }
        _activation = Module.CastFinishAt(spell);
        InitIfReady(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.QuadrupleHammerFirst or AID.LeftHammer or AID.RightHammer)
            AdvanceSequence(0, WorldState.CurrentTime);
    }

    private void InitIfReady(Actor source)
    {
        if (_rotation != default && _increment != default)
        {
            Sequences.Add(new(DoubleHammer.Cone, source.Position, _rotation, _increment, _activation.AddSeconds(0.8f), 3.3f, 4));
            _rotation = default;
            _increment = default;
        }
    }
}

class VolcanicHowl(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.VolcanicHowl));
class Earthbreak(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Earthbreak), 5);
class DeadlyHold(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.DeadlyHold));
class TailSwing(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TailSwing), new AOEShapeCircle(13));
class CriticalBite(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CriticalBite), new AOEShapeCone(10, 60.Degrees()));

abstract class Mandragoras(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(7));
class PluckAndPrune(BossModule module) : Mandragoras(module, AID.PluckAndPrune);
class TearyTwirl(BossModule module) : Mandragoras(module, AID.TearyTwirl);
class HeirloomScream(BossModule module) : Mandragoras(module, AID.HeirloomScream);
class PungentPirouette(BossModule module) : Mandragoras(module, AID.PungentPirouette);
class Pollen(BossModule module) : Mandragoras(module, AID.Pollen);

class HeavySmash(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HeavySmash), 6);

class GymnasiouAcheloiosStates : StateMachineBuilder
{
    public GymnasiouAcheloiosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DoubleHammer>()
            .ActivateOnEnter<QuadrupleHammer>()
            .ActivateOnEnter<TailSwing>()
            .ActivateOnEnter<CriticalBite>()
            .ActivateOnEnter<DeadlyHold>()
            .ActivateOnEnter<Earthbreak>()
            .ActivateOnEnter<VolcanicHowl>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<HeavySmash>()
            .Raw.Update = () => module.Enemies(GymnasiouAcheloios.All).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12019)]
public class GymnasiouAcheloios(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GymnasticEggplant, (uint)OID.GymnasticGarlic, (uint)OID.GymnasticOnion, (uint)OID.GymnasticTomato,
    (uint)OID.GymnasticQueen, (uint)OID.GymnasiouLyssa, (uint)OID.GymnasiouLampas];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.GymnasiouSouchos, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.GymnasiouSouchos));
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
                OID.GymnasticQueen or OID.GymnasiouLyssa or OID.GymnasiouLampas => 2,
                OID.GymnasiouSouchos => 1,
                _ => 0
            };
        }
    }
}
