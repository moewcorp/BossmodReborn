namespace BossMod.Stormblood.Dungeon.D04DomaCastle.D043HypertunedGrynewaht;

public enum OID : uint
{
    Boss = 0x1BD4, // R0.6
    MagitekChakram = 0x1BD8, // R3.0
    RetunedMagitekBit = 0x1BD6, // R0.9
    RetunedMagitekBitHelper = 0x1BD7, // R0.5
    Helper = 0x1BD5
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    CleanCut = 8369, // MagitekChakram->location, 6.0s cast, width 8 rect charge
    ChainsawFirst = 8360, // Boss->self, 2.0s cast, range 4+R width 2 rect
    ChainsawRest = 8361, // Helper->self, no cast, range 4+R width 2 rect
    DelayActionChargeVisual = 8364, // Boss->self, no cast, single-target
    DelayActionCharge = 8365, // Helper->player, no cast, range 6 circle

    GunsawFirst = 8362, // Boss->self, no cast, range 60+R width 2 rect
    GunsawRest = 8363, // Boss->self, no cast, range 60+R width 2 rect

    ThermobaricChargeVisual = 8366, // Boss->self, no cast, single-target
    ThermobaricChargeVisual2 = 8368, // Boss->self, no cast, single-target
    ThermobaricCharge = 8367, // Helper->location, 10.0s cast, range 60 circle

    ChainMineVisual1 = 9287, // RetunedMagitekBit->self, no cast, range 50+R width 3 rect
    ChainMineVisual2 = 9144, // RetunedMagitekBitHelper->self, no cast, range 3 circle
    ChainMine = 8359 // RetunedMagitekBitHelper->player, no cast, single-target
}

public enum IconID : uint
{
    Spreadmarker = 99 // player
}

public enum TetherID : uint
{
    HexadroneBits = 60 // RetunedMagitekBit->RetunedMagitekBit
}

public enum SID : uint
{
    Prey = 1253 // Boss->player, extra=0x0
}

class CleanCut(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.CleanCut), 4);
class DelayActionCharge(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, ActionID.MakeSpell(AID.DelayActionCharge), 6, 4);
class ThermobaricCharge(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ThermobaricCharge), 30);

class Chainsaw(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(4.6f, 1);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe != null)
            yield return _aoe.Value with { Origin = Module.PrimaryActor.Position, Rotation = Module.PrimaryActor.Rotation };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ChainsawFirst)
            _aoe = new(rect, Module.PrimaryActor.Position, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ChainsawFirst:
            case AID.ChainsawRest:
                if (++NumCasts == 5)
                {
                    _aoe = null;
                    NumCasts = 0;
                }
                break;
        }
    }
}

class ChainMine(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(40, 2, 10);
    private readonly ThermobaricCharge _aoe = module.FindComponent<ThermobaricCharge>()!;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;
    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.HexadroneBits)
            _aoes.Add(new(rect, source.Position, source.Rotation, WorldState.FutureTime(5.6f)));
    }
    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.HexadroneBits)
            _aoes.RemoveAll(x => x.Origin == source.Position);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe.ActiveAOEs(slot, actor).Count() <= 1)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class ThermobaricChargeBait(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private static readonly AOEShapeCircle circle = new(30);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Prey)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, circle, status.ExpireAt));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Prey)
            CurrentBaits.Clear();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveBaits.Any(x => x.Target == actor))
            hints.Add("Bait away!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveBaits.Any(x => x.Target == actor))
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 23));
    }
}

class Gunsaw(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect rect = new(60.9f, 1);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.GunsawFirst)
        {
            var target = Raid.WithoutSlot(false, true, true).FirstOrDefault(x => x.Position.InRect(Module.PrimaryActor.Position, Module.PrimaryActor.Rotation, rect.LengthFront, 0, 0.02f));
            if (target != default)
                CurrentBaits.Add(new(caster, target, rect));
        }
        else if ((AID)spell.Action.ID == AID.GunsawRest)
            if (++NumCasts == 4)
            {
                CurrentBaits.Clear();
                NumCasts = 0;
            }
    }
}

class D043HypertunedGrynewahtStates : StateMachineBuilder
{
    public D043HypertunedGrynewahtStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ThermobaricChargeBait>()
            .ActivateOnEnter<ThermobaricCharge>()
            .ActivateOnEnter<CleanCut>()
            .ActivateOnEnter<Chainsaw>()
            .ActivateOnEnter<ChainMine>()
            .ActivateOnEnter<DelayActionCharge>()
            .ActivateOnEnter<Gunsaw>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 241, NameID = 6205)]
public class D043HypertunedGrynewaht(WorldState ws, Actor primary) : BossModule(ws, primary, new(-240, -198), new ArenaBoundsSquare(19.5f));
