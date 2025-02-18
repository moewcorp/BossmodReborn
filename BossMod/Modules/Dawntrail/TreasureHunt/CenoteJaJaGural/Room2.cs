namespace BossMod.Dawntrail.TreasureHunt.CenoteJaJaGural.Room2;

public enum OID : uint
{
    Boss = 0x1EBAA5, // R0.5
    CenoteWoodsman = 0x4310, // R1.9
    CenoteBranchbearer = 0x4313, // R3.375
    CenoteSlammer = 0x4316, // R4.169
    CenoteAlligator = 0x4318, // R2.97
    CenoteMonstera = 0x4314, // R2.8
    CenotePitcherWeed = 0x4311, // R2.4
    CenoteMourner = 0x4315, // R1.56
    CenoteLeafkin = 0x4317, // R1.4

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
    AutoAttack1 = 872, // UolonOfFortune/CenoteMourner/CenoteLeafkin/CenoteMonstera/CenoteWoodsman/UolonOfFortune->player, no cast, single-target
    AutoAttack2 = 871, // CenoteBranchbearer->player, no cast, single-target
    AutoAttack3 = 870, // CenotePitcherWeed/CenoteSlammer/CenoteAlligator->player, no cast, single-target

    TornadicSerenade = 38294, // CenoteBranchbearer->location, 3.0s cast, range 6 circle
    SwiftwindSerenade = 38293, // CenoteBranchbearer->self, 3.0s cast, range 40 width 8 rect
    Ovation = 38290, // CenoteWoodsman->self, 3.0s cast, range 14 width 4 rect
    GravelShower = 38300, // CenoteSlammer->self, 3.0s cast, range 10 width 4 rect
    Flatten = 38299, // CenoteSlammer->self, 2.5s cast, range 8 90-degree cone
    CriticalBite = 38302, // CenoteAlligator->self, 3.0s cast, range 10 120-degree cone
    WaterIII = 38296, // CenoteMonstera->location, 4.0s cast, range 8 circle
    NepenthicPlunge = 38291, // CenotePitcherWeed->self, 3.0s cast, range 10 90-degree cone
    Dissever = 38295, // CenoteMonstera->self, 3.0s cast, range 10 90-degree cone
    PollenCorona = 38298, // CenoteMourner->self, 4.0s cast, range 8 circle
    Tornado = 38301, // CenoteLeafkin->location, 3.0s cast, range 6 circle
    DoubleSmash = 38297, // CenoteMourner->self, 3.0s cast, range 10 120-degree cone

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

class SwiftwindSerenade(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SwiftwindSerenade), new AOEShapeRect(40f, 4f));
class Ovation(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Ovation), new AOEShapeRect(14f, 2f));
class GravelShower(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GravelShower), new AOEShapeRect(10f, 2f));
class Flatten(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Flatten), new AOEShapeCone(8f, 45f.Degrees()));
class PollenCorona(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.PollenCorona), 8f);
class WaterIII(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WaterIII), 8f);

abstract class Cone1045(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(10f, 45f.Degrees()));
class Dissever(BossModule module) : Cone1045(module, AID.Dissever);
class NepenthicPlunge(BossModule module) : Cone1045(module, AID.NepenthicPlunge);

abstract class Cone1060(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(10f, 60f.Degrees()));
class DoubleSmash(BossModule module) : Cone1060(module, AID.DoubleSmash);
class CriticalBite(BossModule module) : Cone1060(module, AID.CriticalBite);

abstract class CircleLoc6(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 6f);
class Tornado(BossModule module) : CircleLoc6(module, AID.Tornado);
class TornadicSerenade(BossModule module) : CircleLoc6(module, AID.TornadicSerenade);
class RottenSpores(BossModule module) : CircleLoc6(module, AID.RottenSpores);

class Spin(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Spin), 11f);

abstract class Mandragoras(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 7f);
class PluckAndPrune(BossModule module) : Mandragoras(module, AID.PluckAndPrune);
class TearyTwirl(BossModule module) : Mandragoras(module, AID.TearyTwirl);
class HeirloomScream(BossModule module) : Mandragoras(module, AID.HeirloomScream);
class PungentPirouette(BossModule module) : Mandragoras(module, AID.PungentPirouette);
class Pollen(BossModule module) : Mandragoras(module, AID.Pollen);

class Room2States : StateMachineBuilder
{
    public Room2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TornadicSerenade>()
            .ActivateOnEnter<SwiftwindSerenade>()
            .ActivateOnEnter<Ovation>()
            .ActivateOnEnter<GravelShower>()
            .ActivateOnEnter<Flatten>()
            .ActivateOnEnter<CriticalBite>()
            .ActivateOnEnter<NepenthicPlunge>()
            .ActivateOnEnter<WaterIII>()
            .ActivateOnEnter<Dissever>()
            .ActivateOnEnter<Tornado>()
            .ActivateOnEnter<PollenCorona>()
            .ActivateOnEnter<DoubleSmash>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<RottenSpores>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(Room2.All);
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
public class Room2(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaBounds.Center, ArenaBounds)
{
    private static readonly WPos arenaCenter = new(default, 192f);
    private static readonly Angle a135 = 135.Degrees();
    private static readonly WDir dir135 = a135.ToDirection(), dirM135 = (-a135).ToDirection();
    public static readonly ArenaBoundsComplex ArenaBounds = new([new Polygon(arenaCenter, 19.5f * CosPI.Pi36th, 36), new Rectangle(arenaCenter + 8.65f * dir135, 20f, 6.15f, a135),
    new Rectangle(arenaCenter + 8.65f * dirM135, 20f, 6.15f, -a135), new Rectangle(arenaCenter + 12f * dir135, 20f, 4.3f, a135), new Rectangle(arenaCenter + 12f * dirM135, 20f, 4.3f, -a135),
    new Rectangle(arenaCenter + 14.3f * dir135, 20f, 3.5f, a135), new Rectangle(arenaCenter + 14.3f * dirM135, 20f, 3.5f, -a135)], [new Rectangle(new(default, 212f), 20f, 1.7f)]);
    private static readonly uint[] bonusAdds = [(uint)OID.TuligoraQueen, (uint)OID.TuraliTomato, (uint)OID.TuraliOnion, (uint)OID.TuraliEggplant,
    (uint)OID.TuraliGarlic, (uint)OID.UolonOfFortune, (uint)OID.AlpacaOfFortune];
    private static readonly uint[] trash = [(uint)OID.CenoteWoodsman, (uint)OID.CenoteBranchbearer, (uint)OID.CenoteSlammer, (uint)OID.CenoteAlligator, (uint)OID.CenoteMonstera,
    (uint)OID.CenotePitcherWeed, (uint)OID.CenoteMourner, (uint)OID.CenoteLeafkin];
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
