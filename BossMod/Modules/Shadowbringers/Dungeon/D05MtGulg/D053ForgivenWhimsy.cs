namespace BossMod.Shadowbringers.Dungeon.D05MtGulg.D053ForgivenWhimsy;

public enum OID : uint
{
    Boss = 0x27CC, //R=20.00
    Brightsphere = 0x27CD, //R=1.0
    Towers = 0x1EAACF, //R=0.5
    Helper2 = 0x2E8, //R=0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 15624, // Boss->player, no cast, single-target

    SacramentOfPenanceVisual = 15627, // Boss->self, 4.0s cast, single-target
    SacramentOfPenance = 15628, // Helper->self, no cast, range 50 circle
    Reformation = 15620, // Boss->self, no cast, single-target, boss changes pattern
    ExegesisA = 16989, // Boss->self, 5.0s cast, single-target
    ExegesisB = 16987, // Boss->self, 5.0s cast, single-target
    ExegesisC = 15622, // Boss->self, 5.0s cast, single-target
    ExegesisD = 16988, // Boss->self, 5.0s cast, single-target
    Exegesis = 15623, // Helper->self, no cast, range 10 width 10 rect
    Catechism = 15625, // Boss->self, 4.0s cast, single-target
    Catechism2 = 15626, // Helper->player, no cast, single-target
    JudgmentDay = 15631, // Boss->self, 3.0s cast, single-target, tower circle 5
    Judged = 15633, // Helper->self, no cast, range 5 circle, tower success
    FoundWanting = 15632, // Helper->self, no cast, range 40 circle, tower fail
    RiteOfTheSacrament = 15629, // Boss->self, no cast, single-target
    PerfectContrition = 15630 // Brightsphere->self, 6.0s cast, range 5-15 donut
}

class PerfectContrition(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(5, 15);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Brightsphere)
            _aoes.Add(new(donut, actor.Position, default, WorldState.FutureTime(10.6f)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.PerfectContrition)
            _aoes.Clear();
    }
}

class Catechism(BossModule module) : Components.SingleTargetCastDelay(module, ActionID.MakeSpell(AID.Catechism), ActionID.MakeSpell(AID.Catechism2), 0.5f);
class SacramentOfPenance(BossModule module) : Components.RaidwideCastDelay(module, ActionID.MakeSpell(AID.SacramentOfPenanceVisual), ActionID.MakeSpell(AID.SacramentOfPenance), 0.5f);

class JudgmentDay(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Towers)
            Towers.Add(new(actor.Position, 5, activation: WorldState.FutureTime(7.6f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Towers.Count != 0 && (AID)spell.Action.ID is AID.Judged or AID.FoundWanting)
            Towers.RemoveAt(0);
    }
}

class Exegesis(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(5);
    private static readonly AOEShapeRect rect = new(5, 5, 5);
    private static readonly AOEShapeCross cross = new(15, 5);
    private static readonly WPos[] diagonalPositions = [new(-240, -50), new(-250, -40), new(-230, -40), new(-250, -60), new(-230, -60)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var _activation = Module.CastFinishAt(spell, 0.4f);
        switch ((AID)spell.Action.ID)
        {
            case AID.ExegesisA: //diagonal
                for (var i = 0; i < diagonalPositions.Length; ++i)
                    _aoes.Add(new(rect, diagonalPositions[i], default, _activation));
                break;
            case AID.ExegesisB: //east+west
                _aoes.Add(new(rect, new(-250, -50), default, _activation));
                _aoes.Add(new(rect, new(-230, -50), default, _activation));
                break;
            case AID.ExegesisC: //north+south
                _aoes.Add(new(rect, new(-240, -60), default, _activation));
                _aoes.Add(new(rect, new(-240, -40), default, _activation));
                break;
            case AID.ExegesisD: //cross
                _aoes.Add(new(cross, new(-240, -50), default, _activation));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Exegesis)
            _aoes.Clear();
    }
}

class D053ForgivenWhimsyStates : StateMachineBuilder
{
    public D053ForgivenWhimsyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Catechism>()
            .ActivateOnEnter<SacramentOfPenance>()
            .ActivateOnEnter<PerfectContrition>()
            .ActivateOnEnter<JudgmentDay>()
            .ActivateOnEnter<Exegesis>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 659, NameID = 8261)]
public class D053ForgivenWhimsy(WorldState ws, Actor primary) : BossModule(ws, primary, new(-240, -50), new ArenaBoundsSquare(15)); //actually walkable arena size is 14.5, but then the tiny safespots in the corner are no longer visible
