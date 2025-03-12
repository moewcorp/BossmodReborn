namespace BossMod.Shadowbringers.Hunt.RankS.Tyger;

public enum OID : uint
{
    Boss = 0x288E // R=5.92
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    TheLionsBreath = 16957, // Boss->self, 4.0s cast, range 30 120-degree cone
    TheScorpionsSting = 16961, // Boss->self, no cast, range 18 90-degree cone, 2-4s after a voice attack, timing seems to vary, maybe depends if voice was interrupted and how fast?
    TheDragonsBreath = 16959, // Boss->self, 4.0s cast, range 30 120-degree cone
    TheRamsBreath = 16958, // Boss->self, 4.0s cast, range 30 120-degree cone
    TheRamsEmbrace = 16960, // Boss->location, 3.0s cast, range 9 circle
    TheDragonsVoice = 16963, // Boss->self, 4.0s cast, range 8-30 donut, interruptible raidwide donut
    TheRamsVoice = 16962 // Boss->self, 4.0s cast, range 9 circle
}

abstract class Breath(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(30f, 60f.Degrees()));
class TheDragonsBreath(BossModule module) : Breath(module, AID.TheDragonsBreath);
class TheRamsBreath(BossModule module) : Breath(module, AID.TheRamsBreath);
class TheLionsBreath(BossModule module) : Breath(module, AID.TheLionsBreath);

class TheScorpionsSting(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCone cone = new(18f, 45f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.TheRamsVoice or (uint)AID.TheDragonsVoice) // timing varies, just used the lowest I could find, probably depends on interrupt status
            _aoe = new(cone, spell.LocXZ, spell.Rotation + 180f.Degrees(), Module.CastFinishAt(spell, 2.3f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TheScorpionsSting)
            _aoe = null;
    }
}

class TheRamsEmbrace(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TheRamsEmbrace), 9f);
class TheRamsVoice(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TheRamsVoice), 9f);
class TheRamsVoiceHint(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.TheRamsVoice));
class TheDragonsVoice(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TheDragonsVoice), new AOEShapeDonut(8f, 30f));
class TheDragonsVoiceHint(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.TheDragonsVoice), hintExtra: "Donut Raidwide");

class TygerStates : StateMachineBuilder
{
    public TygerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheDragonsBreath>()
            .ActivateOnEnter<TheScorpionsSting>()
            .ActivateOnEnter<TheDragonsVoice>()
            .ActivateOnEnter<TheDragonsVoiceHint>()
            .ActivateOnEnter<TheLionsBreath>()
            .ActivateOnEnter<TheRamsBreath>()
            .ActivateOnEnter<TheRamsEmbrace>()
            .ActivateOnEnter<TheRamsVoice>()
            .ActivateOnEnter<TheRamsVoiceHint>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 8905)]
public class Tyger(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
