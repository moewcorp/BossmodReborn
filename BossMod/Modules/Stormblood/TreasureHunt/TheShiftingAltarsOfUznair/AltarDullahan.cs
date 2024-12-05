namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarDullahan;

public enum OID : uint
{
    Boss = 0x2533, //R=3.8
    AltarVodoriga = 0x2563, //R=1.8
    AltarQueen = 0x254A, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    AltarGarlic = 0x2548, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    AltarTomato = 0x2549, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    AltarOnion = 0x2546, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    AltarEgg = 0x2547, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    AltarMatanga = 0x2545, // R3.420
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // AltarMatanga/Mandragoras->player, no cast, single-target
    AutoAttack3 = 6497, // AltarVodoriga->player, no cast, single-target

    IronJustice = 13316, // Boss->self, 3.0s cast, range 8+R 120-degree cone
    Cloudcover = 13477, // Boss->location, 3.0s cast, range 6 circle
    TerrorEye = 13644, // AltarVodoriga->location, 3.5s cast, range 6 circle
    StygianRelease = 13314, // Boss->self, 3.5s cast, range 50+R circle, small raidwide dmg, knockback 20 from source
    VillainousRebuke = 13315, // Boss->players, 4.5s cast, range 6 circle

    PluckAndPrune = 6449, // AltarEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // AltarGarlic->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // AltarOnion->self, 3.5s cast, range 6+R circle
    Pollen = 6452, // AltarQueen->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // AltarTomato->self, 3.5s cast, range 6+R circle
    MatangaActivate = 9636, // AltarMatanga->self, no cast, single-target
    Spin = 8599, // AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // AltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // AltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 9630 // Mandragoras/AltarMatanga->self, no cast, single-target, bonus add disappear
}

class IronJustice(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.IronJustice), new AOEShapeCone(11.8f, 60.Degrees()));
class Cloudcover(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Cloudcover), 6);
class TerrorEye(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.TerrorEye), 6);
class VillainousRebuke(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.VillainousRebuke), 6, 8, 8);
class StygianRelease(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.StygianRelease));
class StygianReleaseKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.StygianRelease), 20, stopAtWall: true)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Module.FindComponent<TerrorEye>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;
}

class RaucousScritch(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RaucousScritch), new AOEShapeCone(8.42f, 30.Degrees()));
class Hurl(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Hurl), 6);
class Spin(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCone(9.42f, 60.Degrees()), (uint)OID.AltarMatanga);

abstract class Mandragoras(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(6.84f));
class PluckAndPrune(BossModule module) : Mandragoras(module, AID.PluckAndPrune);
class TearyTwirl(BossModule module) : Mandragoras(module, AID.TearyTwirl);
class HeirloomScream(BossModule module) : Mandragoras(module, AID.HeirloomScream);
class PungentPirouette(BossModule module) : Mandragoras(module, AID.PungentPirouette);
class Pollen(BossModule module) : Mandragoras(module, AID.Pollen);

class AltarDullahanStates : StateMachineBuilder
{
    public AltarDullahanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IronJustice>()
            .ActivateOnEnter<Cloudcover>()
            .ActivateOnEnter<TerrorEye>()
            .ActivateOnEnter<VillainousRebuke>()
            .ActivateOnEnter<StygianRelease>()
            .ActivateOnEnter<StygianReleaseKB>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => Module.WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7585)]
public class AltarDullahan(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.AltarVodoriga));
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
                OID.AltarVodoriga => 1,
                _ => 0
            };
        }
    }
}
