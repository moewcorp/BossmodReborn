namespace BossMod.Dawntrail.Hunt.RankA.Yehehetoauapyo;

public enum OID : uint
{
    Boss = 0x43DB // R6.250, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    WhirlingOmen1 = 38627, // Boss->self, 5.0s cast, single-target
    DactailToTurnspit = 38633, // Boss->self, 5.0s cast, single-target
    Dactail = 38637, // Boss->self, 0.8s cast, range 40 150.000-degree cone
    Pteraspit = 38638, // Boss->self, 0.8s cast, range 40 150.000-degree cone
    WhirlingOmen2 = 38628, // Boss->self, 5.0s cast, single-target
    TurntailToPteraspit = 38635, // Boss->self, 5.0s cast, single-target
    TurnspitToDactail = 38634, // Boss->self, 5.0s cast, single-target
    Dactail2 = 38639, // Boss->self, 0.8s cast, range 40 150.000-degree cone
    Pteraspit2 = 38636, // Boss->self, 0.8s cast, range 40 150.000-degree cone
    PteraspitToTurntail = 38632, // Boss->self, 5.0s cast, single-target
    Dactail3 = 38641, // Boss->self, 0.8s cast, range 40 150.000-degree cone
    WhirlingOmen3 = 39878, // Boss->self, 5.0s cast, range 50 circle
    WhirlingOmen4 = 38631, // Boss->self, 5.0s cast, single-target
    Pteraspit3 = 38640 // Boss->self, 0.8s cast, range 40 150.000-degree cone
}

public enum SID : uint
{
    RightWindup = 4030,
    LeftWindup = 4029,
    RightWindup2 = 4032,
    LeftWindup2 = 4031
}

class WhirlingOmenRaidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.WhirlingOmen3), "Raidwide, no turn buffs this time!");

class TailSpit(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Angle> _windupDirections = [];
    private readonly List<AOEInstance> _aoes = [];
    private int _castCount;

    private static readonly AOEShapeCone _cone = new(40, 75.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.RightWindup:
            case (uint)SID.RightWindup2:
                _windupDirections.Add(-90f.Degrees());
                break;
            case (uint)SID.LeftWindup:
            case (uint)SID.LeftWindup2:
                _windupDirections.Add(90f.Degrees());
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (_windupDirections.Count != 0)
            switch (status.ID)
            {
                case (uint)SID.RightWindup:
                case (uint)SID.RightWindup2:
                case (uint)SID.LeftWindup:
                case (uint)SID.LeftWindup2:
                    _windupDirections.RemoveAt(0);
                    break;
            }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var facingDirection = Module.PrimaryActor.Rotation;

        switch (spell.Action.ID)
        {
            case (uint)AID.TurnspitToDactail:
                _aoes.Clear();
                // Turnspit (uses windup, frontal) then Dactail (back, no windup)
                facingDirection = HandleWindup(facingDirection, caster.Position, true, true); // Front cone with windup
                HandleWindup(facingDirection, caster.Position, false, false, true); // Back cone without windup
                break;
            case (uint)AID.DactailToTurnspit:
                _aoes.Clear();
                // Dactail (back, no windup) then Turnspit (uses windup, frontal)
                facingDirection = HandleWindup(facingDirection, caster.Position, false, false); // Back cone without windup
                HandleWindup(facingDirection, caster.Position, true, true, true); // Front cone with second turn using windup
                break;
            case (uint)AID.PteraspitToTurntail:
                _aoes.Clear();
                // Pteraspit (front, no windup) then Turntail (uses windup, back)
                facingDirection = HandleWindup(facingDirection, caster.Position, true, false); // Front cone with no windup
                HandleWindup(facingDirection, caster.Position, false, true, true); // Back cone with windup
                break;
            case (uint)AID.TurntailToPteraspit:
                _aoes.Clear();
                // Turntail (uses windup, back) then Pteraspit (front, no windup)
                facingDirection = HandleWindup(facingDirection, caster.Position, false, true); // Back cone with windup
                HandleWindup(facingDirection, caster.Position, true, false, true); // Front cone with no windup
                break;
        }
    }

    private Angle HandleWindup(Angle facingDirection, WPos casterPosition, bool isFront, bool turn = false, bool second = false)
    {
        if (turn && _windupDirections.Count > 0)
        {
            facingDirection += _windupDirections[0];
        }

        if (isFront)
        {
            _aoes.Add(new(_cone, casterPosition, facingDirection, WorldState.FutureTime(5.8d), Colors.Danger));
        }
        else
        {
            _aoes.Add(new(_cone, casterPosition, facingDirection + 180f.Degrees(), WorldState.FutureTime(5.8d), Colors.Danger));
        }

        if (second)
        {
            var currentAOE = _aoes[1];
            _aoes[1] = new(currentAOE.Shape, currentAOE.Origin, currentAOE.Rotation, WorldState.FutureTime(6.6d));
        }

        return facingDirection;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Dactail or (uint)AID.Dactail2 or (uint)AID.Dactail3
            or (uint)AID.Pteraspit or (uint)AID.Pteraspit2 or (uint)AID.Pteraspit3)
        {
            _castCount++;

            if (_aoes.Count > 0)
            {
                _aoes.RemoveAt(0);
            }

            if (_aoes.Count > 0)
            {
                var currentAOE = _aoes[0];
                _aoes[0] = new(currentAOE.Shape, currentAOE.Origin, currentAOE.Rotation, currentAOE.Activation, Colors.Danger);
            }

            if (_castCount >= 2)
            {
                _aoes.Clear();
                _castCount = 0;
            }
        }
    }
}

class YehehetoauapyoStates : StateMachineBuilder
{
    public YehehetoauapyoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WhirlingOmenRaidwide>()
            .ActivateOnEnter<TailSpit>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13400)]
public class Yehehetoauapyo(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
