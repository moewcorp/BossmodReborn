namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarChimera;

public enum OID : uint
{
    Boss = 0x2539, //R=5.92
    AltarAhriman = 0x256A, //R=2.07
    IceVoidzone = 0x1E8D9C,
    AltarMatanga = 0x2545, // R3.42
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // AltarMatanga->player, no cast, single-target
    AutoAttack3 = 6499, // AltarAhriman->player, no cast, single-target

    TheScorpionsSting = 13393, // Boss->self, 3.5s cast, range 6+R 90-degree cone
    TheRamsVoice = 13394, // Boss->self, 5.0s cast, range 4+R circle, interruptible, deep freeze + frostbite
    TheLionsBreath = 13392, // Boss->self, 3.5s cast, range 6+R 120-degree cone, burn
    LanguorousGaze = 13742, // AltarAhriman->self, 3.0s cast, range 6+R 90-degree cone
    TheRamsKeeper = 13396, // Boss->location, 3.5s cast, range 6 circle, voidzone
    TheDragonsVoice = 13395, // Boss->self, 5.0s cast, range 8-30 donut, interruptible, paralaysis

    MatangaActivate = 9636, // AltarMatanga->self, no cast, single-target
    Spin = 8599, // AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // AltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // AltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 9630 // AltarMatanga->self, no cast, single-target, bonus add disappear
}

public enum IconID : uint
{
    Baitaway = 23 // player
}

class TheScorpionsSting(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TheScorpionsSting), new AOEShapeCone(11.92f, 45.Degrees()));
class TheRamsVoice(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TheRamsVoice), new AOEShapeCircle(9.92f));
class TheRamsVoiceHint(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.TheRamsVoice));
class TheLionsBreath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TheLionsBreath), new AOEShapeCone(11.92f, 60.Degrees()));
class LanguorousGaze(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LanguorousGaze), new AOEShapeCone(8.07f, 45.Degrees()));
class TheDragonsVoice(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TheDragonsVoice), new AOEShapeDonut(8, 30));
class TheDragonsVoiceHint(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.TheDragonsVoice));
class TheRamsKeeper(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, ActionID.MakeSpell(AID.TheRamsKeeper), m => m.Enemies(OID.IceVoidzone).Where(z => z.EventState != 7), 0.9f);

class TheRamsKeeperBait(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeCircle circle = new(6);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Baitaway)
            CurrentBaits.Add(new(actor, actor, circle));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TheRamsKeeper)
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
            hints.Add("Bait away!");
    }
}

class RaucousScritch(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RaucousScritch), new AOEShapeCone(8.42f, 30.Degrees()));
class Hurl(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Hurl), 6);
class Spin(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCone(9.42f, 60.Degrees()), (uint)OID.AltarMatanga);

class AltarChimeraStates : StateMachineBuilder
{
    public AltarChimeraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheScorpionsSting>()
            .ActivateOnEnter<TheRamsVoice>()
            .ActivateOnEnter<TheRamsVoiceHint>()
            .ActivateOnEnter<TheDragonsVoice>()
            .ActivateOnEnter<TheDragonsVoiceHint>()
            .ActivateOnEnter<TheLionsBreath>()
            .ActivateOnEnter<LanguorousGaze>()
            .ActivateOnEnter<TheRamsKeeper>()
            .ActivateOnEnter<TheRamsKeeperBait>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => Module.WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7591)]
public class AltarChimera(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.AltarAhriman));
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
                OID.AltarAhriman => 1,
                _ => 0
            };
        }
    }
}
