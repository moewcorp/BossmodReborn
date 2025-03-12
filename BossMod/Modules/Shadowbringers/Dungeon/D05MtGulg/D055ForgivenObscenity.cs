namespace BossMod.Shadowbringers.Dungeon.D05MtGulg.D055ForgivenObscenity;

public enum OID : uint
{
    Boss = 0x27CE, //R=5.0
    BossClones = 0x27CF, //R=5.0
    Orbs = 0x27D0, //R=1.0
    Rings = 0x1EAB62,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    VauthrysBlessing = 15639, // Boss->self, no cast, single-target
    OrisonFortissimo = 15636, // Boss->self, 4.0s cast, single-target
    OrisonFortissimo2 = 15637, // Helper->self, no cast, range 50 circle

    DivineDiminuendoCircle1 = 15638, // Boss->self, 4.0s cast, range 8 circle
    DivineDiminuendoCircle2 = 15640, // Boss->self, 4.0s cast, range 8 circle
    DivineDiminuendoCircle3 = 15649, // BossClones->self, 4.0s cast, range 8 circle
    DivineDiminuendoDonut1 = 15641, // Helper->self, 4.0s cast, range 10-16 donut
    DivineDiminuendoDonut2 = 18025, // Helper->self, 4.0s cast, range 18-32 donut

    ConvictionMarcato1 = 15642, // Boss->self, 4.0s cast, range 40 width 5 rect
    ConvictionMarcato2 = 15643, // Helper->self, 4.0s cast, range 40 width 5 rect
    ConvictionMarcato3 = 15648, // BossClones->self, 4.0s cast, range 40 width 5 rect
    unknown = 16846, // Helper->self, 4.0s cast, single-target
    PenancePianissimo = 15644, // Boss->self, 3.0s cast, single-target, inverted circle voidzone appears
    FeatherMarionette = 15645, // Boss->self, 3.0s cast, single-target
    SolitaireRing = 17066, // Boss->self, 3.5s cast, single-target
    Ringsmith = 15652, // Boss->self, no cast, single-target
    GoldChaser = 15653, // Boss->self, 4.0s cast, single-target
    VenaAmoris = 15655, // Orbs->self, no cast, range 40 width 5 rect
    SacramentSforzando = 15634, // Boss->self, 4.0s cast, single-target
    SacramentSforzando2 = 15635, // Helper->player, no cast, single-target
    SanctifiedStaccato = 15654 // Helper->self, no cast, range 3 circle, sort of a voidzone around the light orbs, only triggers if you get too close
}

class Orbs(BossModule module) : Components.GenericAOEs(module, default, "GTFO from voidzone!")
{
    private readonly List<Actor> _orbs = new(6);
    private const float Radius = 3f;
    private static readonly AOEShapeCapsule capsule = new(Radius, default);
    private static readonly AOEShapeCircle circle = new(Radius);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _orbs.Count;
        if (count == 0)
            return [];
        var rings = Module.Enemies((uint)OID.Rings);
        var countR = rings.Count;
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var o = _orbs[i];
            for (var j = 0; j < countR; ++i)
            {
                var ring = rings[j];
                if (ring.Position.InRect(o.Position, 20f * o.Rotation.ToDirection(), Radius))
                {
                    aoes[i] = new(capsule with { Length = (ring.Position - o.Position).Length() }, o.Position, o.Rotation);
                    break;
                }
                else if (j == countR)
                    aoes[i] = new(circle, o.Position);
            }
        }
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Orbs)
            _orbs.Add(actor);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008)
            _orbs.RemoveAll(t => t.Position.AlmostEqual(actor.Position, 1f));
    }
}

class GoldChaser(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    private readonly List<Actor> _casters = new(6);
    private static readonly AOEShapeRect rect = new(100f, 2.5f);
    private static readonly WPos[] positionsSet1 = [new(-227.5f, 253f), new(-232.5f, 251.5f)];
    private static readonly WPos[] positionsSet2 = [new(-252.5f, 253f), new(-247.5f, 251.5f)];
    private static readonly WPos[] positionsSet3 = [new(-242.5f, 253f), new(-237.5f, 253)];
    private static readonly WPos[] positionsSet4 = [new(-252.5f, 253f), new(-227.5f, 253)];

    private bool AreCastersInPositions(WPos[] positions)
    {
        var caster0 = _casters[0].Position;
        var caster1 = _casters[1].Position;
        return _casters.Count >= 2 &&
               (caster0 == positions[0] && caster1 == positions[1] || caster0 == positions[1] && caster1 == positions[0]);
    }
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _casters.Count;
        if (count == 0)
            return [];

        var inSet1Or2 = AreCastersInPositions(positionsSet1) || AreCastersInPositions(positionsSet2);
        var inSet3Or4 = AreCastersInPositions(positionsSet3) || AreCastersInPositions(positionsSet4);

        if (!inSet1Or2 && !inSet3Or4)
            return [];

        var aoeCount = 0;
        if (count > 2)
            aoeCount += (NumCasts == 0 ? 1 : 0) + (NumCasts is 0 or 1 ? 1 : 0);
        if (count > 4 && NumCasts is 0 or 1)
            aoeCount += 2;
        if (count > 4)
            aoeCount += (NumCasts == 2 ? 1 : 0) + (NumCasts is 2 or 3 ? 1 : 0);
        if (count == 6 && NumCasts is 2 or 3)
            aoeCount += 2;
        if (count == 6)
            aoeCount += (NumCasts == 4 ? 1 : 0) + (NumCasts is 4 or 5 ? 1 : 0);

        if (aoeCount == 0)
            return [];

        var aoes = new AOEInstance[aoeCount];
        var index = 0;

        if (count > 2)
        {
            if (NumCasts == 0)
                aoes[index++] = new(rect, _casters[0].Position, default, _activation.AddSeconds(7.1f), Colors.Danger);
            if (NumCasts is 0 or 1)
                aoes[index++] = new(rect, _casters[1].Position, default, _activation.AddSeconds(inSet1Or2 ? 7.6f : 7.1f), Colors.Danger);
        }

        if (count > 4)
        {
            if (NumCasts is 0 or 1)
            {
                aoes[index++] = new(rect, _casters[2].Position, default, _activation.AddSeconds(8.1f), Risky: false);
                aoes[index++] = new(rect, _casters[3].Position, default, _activation.AddSeconds(inSet1Or2 ? 8.6f : 8.1f), Risky: false);
            }
            if (NumCasts == 2)
                aoes[index++] = new(rect, _casters[2].Position, default, _activation.AddSeconds(8.1f), Colors.Danger);
            if (NumCasts is 2 or 3)
                aoes[index++] = new(rect, _casters[3].Position, default, _activation.AddSeconds(inSet1Or2 ? 8.6f : 8.1f), Colors.Danger);
        }

        if (count == 6)
        {
            if (NumCasts is 2 or 3)
            {
                aoes[index++] = new(rect, _casters[4].Position, default, _activation.AddSeconds(inSet1Or2 ? 9.1f : 11.1f), Risky: false);
                aoes[index++] = new(rect, _casters[5].Position, default, _activation.AddSeconds(11.1f), Risky: false);
            }
            if (NumCasts == 4)
                aoes[index++] = new(rect, _casters[4].Position, default, _activation.AddSeconds(inSet1Or2 ? 9.1f : 11.1f), Colors.Danger);
            if (NumCasts is 4 or 5)
                aoes[index++] = new(rect, _casters[5].Position, default, _activation.AddSeconds(11.1f), Colors.Danger);
        }

        for (var i = 0; i < aoeCount; ++i)
            aoes[i].Origin = WPos.ClampToGrid(aoes[i].Origin);
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Rings)
            _casters.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Ringsmith)
            _activation = WorldState.CurrentTime;
        else if (spell.Action.ID == (uint)AID.VenaAmoris)
        {
            if (++NumCasts == 6)
            {
                _casters.Clear();
                NumCasts = 0;
            }
        }
    }
}

class SacramentSforzando(BossModule module) : Components.SingleTargetCastDelay(module, ActionID.MakeSpell(AID.SacramentSforzando), ActionID.MakeSpell(AID.SacramentSforzando2), 0.8f);
class OrisonFortissimo(BossModule module) : Components.RaidwideCastDelay(module, ActionID.MakeSpell(AID.OrisonFortissimo), ActionID.MakeSpell(AID.OrisonFortissimo2), 0.8f);

abstract class DivineDiminuendoCircle(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 8f);
class DivineDiminuendoCircle1(BossModule module) : DivineDiminuendoCircle(module, AID.DivineDiminuendoCircle1);
class DivineDiminuendoCircle2(BossModule module) : DivineDiminuendoCircle(module, AID.DivineDiminuendoCircle2);
class DivineDiminuendoCircle3(BossModule module) : DivineDiminuendoCircle(module, AID.DivineDiminuendoCircle3);

class DivineDiminuendoDonut1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.DivineDiminuendoDonut1), new AOEShapeDonut(10f, 16f));
class DivineDiminuendoDonut2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.DivineDiminuendoDonut2), new AOEShapeDonut(18f, 32f));

abstract class ConvictionMarcato(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(40f, 2.5f));
class ConvictionMarcato1(BossModule module) : ConvictionMarcato(module, AID.ConvictionMarcato1);
class ConvictionMarcato2(BossModule module) : ConvictionMarcato(module, AID.ConvictionMarcato2);
class ConvictionMarcato3(BossModule module) : ConvictionMarcato(module, AID.ConvictionMarcato3);

class PenancePianissimo(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeDonut donut = new(14.5f, 30f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.PenancePianissimo)
            _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell, 0.7f));
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008)
            Arena.Bounds = D055ForgivenObscenity.ArenaRect;
        else if (state == 0x00010002)
        {
            _aoe = null;
            Arena.Bounds = D055ForgivenObscenity.ArenaCircle;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Arena.Bounds == D055ForgivenObscenity.ArenaCircle)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), actor, ActionQueue.Priority.High);
    }
}

class D055ForgivenObscenityStates : StateMachineBuilder
{
    public D055ForgivenObscenityStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SacramentSforzando>()
            .ActivateOnEnter<DivineDiminuendoCircle1>()
            .ActivateOnEnter<DivineDiminuendoCircle2>()
            .ActivateOnEnter<DivineDiminuendoCircle3>()
            .ActivateOnEnter<DivineDiminuendoDonut1>()
            .ActivateOnEnter<DivineDiminuendoDonut2>()
            .ActivateOnEnter<ConvictionMarcato1>()
            .ActivateOnEnter<ConvictionMarcato2>()
            .ActivateOnEnter<ConvictionMarcato3>()
            .ActivateOnEnter<OrisonFortissimo>()
            .ActivateOnEnter<GoldChaser>()
            .ActivateOnEnter<Orbs>()
            .ActivateOnEnter<PenancePianissimo>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 659, NameID = 8262)]
public class D055ForgivenObscenity(WorldState ws, Actor primary) : BossModule(ws, primary, new(-240, 237), ArenaRect)
{
    public static readonly ArenaBounds ArenaRect = new ArenaBoundsRect(14.5f, 19.5f);
    public static readonly ArenaBounds ArenaCircle = new ArenaBoundsCircle(15);
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, allowDeadAndUntargetable: true);
    }
}
