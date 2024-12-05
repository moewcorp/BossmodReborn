namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarKelpie;

public enum OID : uint
{
    Boss = 0x2537, //R=5.4
    Hydrosphere = 0x255B, //R=1.2
    AltarMatanga = 0x2545, // R3.42
    GoldWhisker = 0x2544, // R0.54
    AltarQueen = 0x254A, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    AltarGarlic = 0x2548, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    AltarTomato = 0x2549, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    AltarOnion = 0x2546, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    AltarEgg = 0x2547, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // GoldWhisker->player, no cast, single-target
    AutoAttack2 = 872, // Boss/AltarMatanga->player, no cast, single-target

    Torpedo = 13438, // Boss->player, 3.0s cast, single-target
    Innocence = 13439, // Boss->location, 3.0s cast, range 5 circle
    Gallop = 13441, // Boss->location, no cast, ???, movement ability
    RisingSeas = 13440, // Boss->self, 5.0s cast, range 50+R circle, knockback 20, away from source
    BloodyPuddle = 13443, // Hydrosphere->self, 4.0s cast, range 10+R circle
    HydroPush = 13442, // Boss->self, 6.0s cast, range 44+R width 44 rect, knockback 20, dir forward

    MatangaActivate = 9636, // AltarMatanga->self, no cast, single-target
    Spin = 8599, // AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // AltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // AltarMatanga->location, 3.0s cast, range 6 circle
    PluckAndPrune = 6449, // AltarEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // AltarGarlic->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // AltarOnion->self, 3.5s cast, range 6+R circle
    Pollen = 6452, // AltarQueen->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // AltarTomato->self, 3.5s cast, range 6+R circle
    Telega = 9630 // AltarMatanga/Mandragoras->self, no cast, single-target, bonus adds disappear
}

class Innocence(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Innocence), 5);
class HydroPush(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HydroPush), new AOEShapeRect(49.4f, 22, 5));

class BloodyPuddle(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(11.2f);
    private readonly List<Actor> _spheres = [];
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var s in _spheres)
            yield return new(circle, s.Position, default, _activation);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Hydrosphere)
        {
            _spheres.Add(actor);
            _activation = WorldState.FutureTime(8.6f);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BloodyPuddle)
            _spheres.Clear();
    }
}

class Torpedo(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.Torpedo));
class RisingSeas(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.RisingSeas));
class HydroPushKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.HydroPush), 20, shape: new AOEShapeRect(49.4f, 22, 5), kind: Kind.DirForward, stopAtWall: true);

class RisingSeasKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.RisingSeas), 20, stopAtWall: true)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Module.FindComponent<BloodyPuddle>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;
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

class AltarKelpieStates : StateMachineBuilder
{
    public AltarKelpieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Innocence>()
            .ActivateOnEnter<HydroPush>()
            .ActivateOnEnter<BloodyPuddle>()
            .ActivateOnEnter<Torpedo>()
            .ActivateOnEnter<RisingSeas>()
            .ActivateOnEnter<RisingSeasKB>()
            .ActivateOnEnter<HydroPushKB>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .Raw.Update = () => Module.WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7589)]
public class AltarKelpie(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.AltarEgg).Concat(Enemies(OID.AltarTomato)).Concat(Enemies(OID.AltarQueen)).Concat(Enemies(OID.AltarGarlic)).Concat(Enemies(OID.AltarOnion))
        .Concat(Enemies(OID.AltarMatanga).Concat(Enemies(OID.GoldWhisker))), Colors.Vulnerable);
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
                OID.AltarQueen or OID.GoldWhisker => 2,
                OID.AltarMatanga => 1,
                _ => 0
            };
        }
    }
}
