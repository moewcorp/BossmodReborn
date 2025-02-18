namespace BossMod.Dawntrail.TreasureHunt.CenoteJaJaGural.Room4;

public enum OID : uint
{
    Boss = 0x1EBAA7, // R0.5
    CenoteQeziigural = 0x4325, // R3.0
    CenoteTohsoq = 0x430C, // R2.0
    CenoteMegamaguey = 0x4327, // R3.2
    CenoteTyaitya = 0x4328, // R2.0
    CenoteTulichu = 0x4326, // R3.6
    CenoteTomaton = 0x430D, // R2.04
    CenoteNecrosis = 0x430F, // R2.88

    UolonOfFortune = 0x42FF, // R3.5
    AlpacaOfFortune = 0x42FE, // R1.8
    TuraliOnion = 0x4300, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    TuraliEggplant = 0x4301, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    TuraliGarlic = 0x4302, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    TuraliTomato = 0x4303, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    TuligoraQueen = 0x4304, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
}

public enum AID : uint
{
    AutoAttack1 = 870, // CenoteMegamaguey/CenoteTyaitya/CenoteQeziigural/CenoteTohsoq->player, no cast, single-target
    AutoAttack2 = 872, // UolonOfFortune/CenoteTulichu/CenoteTomaton->player, no cast, single-target
    AutoAttack3 = 871, // CenoteNecrosis->player, no cast, single-target

    Lumisphere = 38286, // CenoteQeziigural->location, 3.0s cast, range 6 circle
    AetherialBlast = 38287, // CenoteTohsoq/CenoteQeziigural->self, 3.0s cast, range 20 width 4 rect
    SerratedSpin = 38317, // CenoteMegamaguey->self, 3.0s cast, range 8 circle
    RootsOfAtopy = 38314, // CenoteTulichu->location, 3.0s cast, range 6 circle
    SyrupSpout = 38288, // CenoteTomaton->self, 3.0s cast, range 10 120-degree cone
    SpinningAttack = 38289, // CenoteNecrosis->self, 3.0s cast, range 10 width 4 rect
    RightWingblade = 38318, // CenoteTyaitya->self, 3.0s cast, range 8 180-degree cone
    LeftWingblade = 38319, // CenoteTyaitya->self, 3.0s cast, range 8 180-degree cone
    Tornado = 38316, // CenoteMegamaguey->location, 3.0s cast, range 6 circle
    OdiousAir = 38315, // CenoteTulichu->self, 3.0s cast, range 12 120-degree cone

    Inhale = 38280, // UolonOfFortune->self, 0.5s cast, range 27 120-degree cone
    Spin = 38279, // UolonOfFortune->self, 3.0s cast, range 11 circle
    RottenSpores = 38277, // UolonOfFortune->location, 3.0s cast, range 6 circle
    TearyTwirl = 32301, // TuraliOnion->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // TuraliTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // TuraliGarlic->self, 3.5s cast, range 7 circle
    PluckAndPrune = 32302, // TuraliEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // TuligoraQueen->self, 3.5s cast, range 7 circle
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus adds disappear
}

abstract class CircleLoc6(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 6f);
class Lumisphere(BossModule module) : CircleLoc6(module, AID.Lumisphere);
class Tornado(BossModule module) : CircleLoc6(module, AID.Tornado);
class RootsOfAtopy(BossModule module) : CircleLoc6(module, AID.RootsOfAtopy);
class RottenSpores(BossModule module) : CircleLoc6(module, AID.RottenSpores);

class AetherialBlast(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AetherialBlast), new AOEShapeRect(20f, 2f));
class SerratedSpin(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SerratedSpin), 8f);
class SyrupSpout(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SyrupSpout), new AOEShapeCone(10f, 60f.Degrees()));
class SpinningAttack(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SpinningAttack), new AOEShapeRect(10f, 2f));
class OdiousAir(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.OdiousAir), new AOEShapeCone(12f, 60f.Degrees()));

abstract class Wingblade(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(8f, 90f.Degrees()));
class LeftWingblade(BossModule module) : Wingblade(module, AID.LeftWingblade);
class RightWingblade(BossModule module) : Wingblade(module, AID.RightWingblade);

class Spin(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Spin), 11f);

abstract class Mandragoras(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 7f);
class PluckAndPrune(BossModule module) : Mandragoras(module, AID.PluckAndPrune);
class TearyTwirl(BossModule module) : Mandragoras(module, AID.TearyTwirl);
class HeirloomScream(BossModule module) : Mandragoras(module, AID.HeirloomScream);
class PungentPirouette(BossModule module) : Mandragoras(module, AID.PungentPirouette);
class Pollen(BossModule module) : Mandragoras(module, AID.Pollen);

class Room4States : StateMachineBuilder
{
    public Room4States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Lumisphere>()
            .ActivateOnEnter<AetherialBlast>()
            .ActivateOnEnter<SerratedSpin>()
            .ActivateOnEnter<RootsOfAtopy>()
            .ActivateOnEnter<SyrupSpout>()
            .ActivateOnEnter<SpinningAttack>()
            .ActivateOnEnter<Tornado>()
            .ActivateOnEnter<LeftWingblade>()
            .ActivateOnEnter<RightWingblade>()
            .ActivateOnEnter<OdiousAir>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<RottenSpores>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(Room4.All);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (!enemies[i].IsDeadOrDestroyed)
                        return false;
                }
                return module.PrimaryActor.EventState == 7;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 993, NameID = 5057)]
public class Room4(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaBounds.Center, ArenaBounds)
{
    private static readonly WPos arenaCenter = new(-197f, -145f);
    private static readonly Angle a135 = 135.Degrees();
    private static readonly WDir dir135 = a135.ToDirection(), dirM135 = (-a135).ToDirection();
    public static readonly ArenaBoundsComplex ArenaBounds = new([new Polygon(arenaCenter, 19.5f * CosPI.Pi36th, 36), new Rectangle(arenaCenter + 8.65f * dir135, 20f, 6.15f, a135),
    new Rectangle(arenaCenter + 8.65f * dirM135, 20f, 6.15f, -a135), new Rectangle(arenaCenter + 12f * dir135, 20f, 4.3f, a135), new Rectangle(arenaCenter + 12f * dirM135, 20f, 4.3f, -a135),
    new Rectangle(arenaCenter + 14.3f * dir135, 20f, 3.5f, a135), new Rectangle(arenaCenter + 14.3f * dirM135, 20f, 3.5f, -a135)], [new Rectangle(new(-197f, -125f), 20f, 1.7f)]);
    private static readonly uint[] bonusAdds = [(uint)OID.TuligoraQueen, (uint)OID.TuraliTomato, (uint)OID.TuraliOnion, (uint)OID.TuraliEggplant,
    (uint)OID.TuraliGarlic, (uint)OID.UolonOfFortune, (uint)OID.AlpacaOfFortune];
    private static readonly uint[] trash = [(uint)OID.CenoteQeziigural, (uint)OID.CenoteTohsoq, (uint)OID.CenoteMegamaguey, (uint)OID.CenoteTyaitya, (uint)OID.CenoteTulichu,
    (uint)OID.CenoteTomaton, (uint)OID.CenoteNecrosis];
    public static readonly uint[] All = [(uint)OID.Boss, .. trash, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, Colors.Object);
        Arena.Actors(Enemies(trash));
        Arena.Actors(Enemies(bonusAdds), Colors.Vulnerable);
    }

    protected override bool CheckPull() => !PrimaryActor.IsTargetable;

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.TuraliOnion => 6,
                (uint)OID.TuraliEggplant => 5,
                (uint)OID.TuraliGarlic => 4,
                (uint)OID.TuraliTomato => 3,
                (uint)OID.TuligoraQueen or (uint)OID.AlpacaOfFortune => 2,
                (uint)OID.UolonOfFortune => 1,
                _ => 0
            };
        }
    }
}
