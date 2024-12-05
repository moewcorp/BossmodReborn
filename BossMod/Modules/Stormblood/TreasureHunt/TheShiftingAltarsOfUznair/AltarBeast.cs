namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarBeast;

public enum OID : uint
{
    Boss = 0x2536, //R=4.6
    AltarKeeper = 0x2567, //R=1.25
    AltarQueen = 0x254A, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    AltarGarlic = 0x2548, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    AltarTomato = 0x2549, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    AltarOnion = 0x2546, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    AltarEgg = 0x2547, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    AltarMatanga = 0x2545, // R3.42
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // Mandragoras/AltarMatanga->player, no cast, single-target
    AutoAttack3 = 6499, // AltarKeeper->player, no cast, single-target

    RustingClaw = 13259, // Boss->self, 3.0s cast, range 8+R 120-degree cone
    WordsOfWoe = 13260, // Boss->self, 5.0s cast, range 45+R width 6 rect
    EyeOfTheFire = 13263, // Boss->self, 5.0s cast, range 50+R circle, gaze, applies hysteria
    TheSpin = 13262, // Boss->self, 4.0s cast, range 50+R circle, damage fall off with distance, estimated safety distance between 10 and 15
    VengefulSoul = 13740, // AltarKeeper->location, 3.0s cast, range 6 circle
    TailDrive = 13261, // Boss->self, 3.0s cast, range 30+R 120-degree cone

    Pollen = 6452, // AltarQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // AltarOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // AltarTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // AltarEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // AltarGarlic->self, 3.5s cast, range 6+R circle
    MatangaActivate = 9636, // AltarMatanga->self, no cast, single-target
    Spin = 8599, // AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // AltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // AltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 9630 // AltarMatanga/Mandragoras->self, no cast, single-target, bonus adds disappear
}

class RustingClaw(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RustingClaw), new AOEShapeCone(12.6f, 60.Degrees()));
class TailDrive(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TailDrive), new AOEShapeCone(34.6f, 60.Degrees()));
class TheSpin(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TheSpin), new AOEShapeCircle(15));
class WordsOfWoe(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WordsOfWoe), new AOEShapeRect(49.6f, 3));
class VengefulSoul(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.VengefulSoul), 6);
class EyeOfTheFire(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.EyeOfTheFire));

abstract class Mandragoras(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(6.84f));
class PluckAndPrune(BossModule module) : Mandragoras(module, AID.PluckAndPrune);
class TearyTwirl(BossModule module) : Mandragoras(module, AID.TearyTwirl);
class HeirloomScream(BossModule module) : Mandragoras(module, AID.HeirloomScream);
class PungentPirouette(BossModule module) : Mandragoras(module, AID.PungentPirouette);
class Pollen(BossModule module) : Mandragoras(module, AID.Pollen);

class RaucousScritch(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RaucousScritch), new AOEShapeCone(8.42f, 30.Degrees()));
class Hurl(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Hurl), 6);
class Spin(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCone(9.42f, 60.Degrees()), (uint)OID.AltarMatanga);

class AltarBeastStates : StateMachineBuilder
{
    public AltarBeastStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RustingClaw>()
            .ActivateOnEnter<TailDrive>()
            .ActivateOnEnter<TheSpin>()
            .ActivateOnEnter<WordsOfWoe>()
            .ActivateOnEnter<VengefulSoul>()
            .ActivateOnEnter<EyeOfTheFire>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => Module.WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7588)]
public class AltarBeast(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.AltarKeeper));
        Arena.Actors(Enemies(OID.AltarEgg).Concat(Enemies(OID.AltarTomato)).Concat(Enemies(OID.AltarQueen)).Concat(Enemies(OID.AltarGarlic)).Concat(Enemies(OID.AltarOnion))
        .Concat(Enemies(OID.AltarMatanga)), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.AltarOnion => 6,
                OID.AltarEgg => 5,
                OID.AltarGarlic => 4,
                OID.AltarTomato => 3,
                OID.AltarQueen or OID.AltarMatanga => 2,
                OID.AltarKeeper => 1,
                _ => 0
            };
        }
    }
}
