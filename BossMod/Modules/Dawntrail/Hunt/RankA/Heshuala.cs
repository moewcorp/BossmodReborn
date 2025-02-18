namespace BossMod.Dawntrail.Hunt.RankA.Heshuala;

public enum OID : uint
{
    Boss = 0x416E // R6.6
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    HigherPower1 = 39095, // Boss->self, 2.0s cast, single-target, spin x3
    HigherPower2 = 39096, // Boss->self, 2.0s cast, single-target, spin x4
    HigherPower3 = 39097, // Boss->self, 2.0s cast, single-target, spin x5
    HigherPower4 = 39098, // Boss->self, 2.0s cast, single-target, spin x6

    SpinshockFirstCCW = 39100, // Boss->self, 5.0s cast, range 50 90-degree cone
    SpinshockFirstCW = 39099, // Boss->self, 5.0s cast, range 50 90-degree cone
    SpinshockRest = 39101, // Boss->self, 0.7s cast, range 50 90-degree cone
    ShockingCross = 39102, // Boss->self, 1.7s cast, range 50 width 10 cross, last spin rotation + 45°
    XMarksTheShock = 39103, // Boss->self, 1.7s cast, range 50 width 10 cross, last spin rotation + 90°
    LightningBolt = 39104, // Boss->location, 2.0s cast, range 5 circle
    ElectricalOverload = 39105, // Boss->self, 4.0s cast, range 50 circle
}

public enum SID : uint
{
    ElectricalCharge = 3979, // Boss->Boss, extra=0x4/0x3/0x2/0x1/0x6/0x5
    ShockingCross = 3977, // Boss->Boss, extra=0x0
    XMarksTheShock = 3978, // Boss->Boss, extra=0x0
    NumbingCurrent = 3980, // Boss->player, extra=0x0
}

class SpinShock(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly Angle _increment = 90.Degrees();
    private static readonly AOEShapeCone cone = new(50, 45.Degrees());
    public int Spins;

    private static readonly Dictionary<AID, int> spinMapping = new()
    {
        { AID.HigherPower1, 3 },
        { AID.HigherPower2, 4 },
        { AID.HigherPower3, 5 },
        { AID.HigherPower4, 6 }
    };

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spinMapping.TryGetValue((AID)spell.Action.ID, out var spinCount))
            Spins = spinCount;
        switch ((AID)spell.Action.ID)
        {
            case AID.SpinshockFirstCCW:
                Sequences.Add(new(cone, Module.PrimaryActor.Position, spell.Rotation, _increment, Module.CastFinishAt(spell), 2.7f, Spins));
                break;
            case AID.SpinshockFirstCW:
                Sequences.Add(new(cone, Module.PrimaryActor.Position, spell.Rotation, -_increment, Module.CastFinishAt(spell), 2.7f, Spins));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SpinshockFirstCCW or AID.SpinshockFirstCW or AID.SpinshockRest)
        {
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
            --Spins;
        }
    }
}

class ShockingCrossXMarksTheShock(BossModule module) : Components.GenericAOEs(module)
{
    private readonly SpinShock _rotation = module.FindComponent<SpinShock>()!;
    private enum Cross { None, Cardinal, Intercardinal }
    private Cross currentCross;
    private static readonly AOEShapeCross _cross = new(50, 5);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe != null && _rotation.Spins < 3)
            yield return _aoe.Value;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.XMarksTheShock:
                currentCross = Cross.Intercardinal;
                break;
            case SID.ShockingCross:
                currentCross = Cross.Cardinal;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.XMarksTheShock or AID.ShockingCross)
        {
            currentCross = Cross.None;
            _aoe = null;
        }
    }

    public override void Update()
    {
        if (_aoe == null && currentCross != Cross.None && _rotation.Sequences.Count > 0)
        {
            var sequence = _rotation.Sequences[0];
            var rotationOffset = currentCross == Cross.Cardinal ? default : 45.Degrees();
            var activation = WorldState.FutureTime(11.5f + _rotation.Spins * 2);
            _aoe = new(_cross, sequence.Origin, sequence.Rotation + rotationOffset, activation);
        }
    }
}

class LightningBolt(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LightningBolt), 5);
class ElectricalOverload(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ElectricalOverload));

class HeshualaStates : StateMachineBuilder
{
    public HeshualaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SpinShock>()
            .ActivateOnEnter<ShockingCrossXMarksTheShock>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<ElectricalOverload>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13157)]
public class Heshuala(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
