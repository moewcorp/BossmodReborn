namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarTotem;

public enum OID : uint
{
    Boss = 0x2534, //R=5.06
    TotemsHead = 0x2566, //R=2.2
    FireVoidzone = 0x1EA8BB,
    AltarMatanga = 0x2545, // R3.42
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // AltarMatanga->player, no cast, single-target
    AutoAttack3 = 6499, // TotemsHead->player, no cast, single-target

    FlurryOfRage = 13451, // Boss->self, 3.0s cast, range 8+R 120-degree cone
    WhorlOfFrenzy = 13453, // Boss->self, 3.0s cast, range 6+R circle
    WaveOfMalice = 13454, // Boss->location, 2.5s cast, range 5 circle
    TheWardensVerdict = 13739, // TotemsHead->self, 3.0s cast, range 40+R width 4 rect
    FlamesOfFury = 13452, // Boss->location, 4.0s cast, range 10 circle

    MatangaActivate = 9636, // AltarMatanga->self, no cast, single-target
    Spin = 8599, // AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // AltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // AltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 9630 // AltarMatanga->self, no cast, single-target, bonus adds disappear
}

public enum IconID : uint
{
    Baitaway = 23 // player
}

class FlurryOfRage(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FlurryOfRage), new AOEShapeCone(13.06f, 60.Degrees()));
class WaveOfMalice(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.WaveOfMalice), 5);
class WhorlOfFrenzy(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WhorlOfFrenzy), new AOEShapeCircle(11.06f));
class TheWardensVerdict(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TheWardensVerdict), new AOEShapeRect(45.06f, 2));
class FlamesOfFury(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 10, ActionID.MakeSpell(AID.FlamesOfFury), m => m.Enemies(OID.FireVoidzone).Where(z => z.EventState != 7), 1.2f);

class FlamesOfFuryBait(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeCircle circle = new(10);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Baitaway)
            CurrentBaits.Add(new(actor, actor, circle));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FlamesOfFury)
            ++NumCasts;
        if (NumCasts == 3)
        {
            CurrentBaits.Clear();
            NumCasts = 0;
        }
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
            hints.Add("Bait away! (3 times)");
    }
}

class RaucousScritch(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RaucousScritch), new AOEShapeCone(8.42f, 30.Degrees()));
class Hurl(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Hurl), 6);
class Spin(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCone(9.42f, 60.Degrees()), (uint)OID.AltarMatanga);

class AltarTotemStates : StateMachineBuilder
{
    public AltarTotemStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FlurryOfRage>()
            .ActivateOnEnter<WaveOfMalice>()
            .ActivateOnEnter<WhorlOfFrenzy>()
            .ActivateOnEnter<TheWardensVerdict>()
            .ActivateOnEnter<FlamesOfFury>()
            .ActivateOnEnter<FlamesOfFuryBait>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => Module.WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7586)]
public class AltarTotem(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.TotemsHead));
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
                OID.TotemsHead => 1,
                _ => 0
            };
        }
    }
}
