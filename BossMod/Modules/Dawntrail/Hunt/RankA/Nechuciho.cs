namespace BossMod.Dawntrail.Hunt.RankA.Nechuciho;

public enum OID : uint
{
    Boss = 0x452C // R3.42
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    CastWordOfTheWood = 39872, // Boss->self, 5.0s cast, range 30 180-degree cone
    WordOfTheWoodVisual = 39496, // Boss->self, 7.0s cast, single-target
    WordOfTheWood1 = 39785, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWood2 = 39786, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWood3 = 39787, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWood4 = 39788, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWood5 = 42164, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWood6 = 42165, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWood7 = 42166, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWood8 = 42167, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWood9 = 42168, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWood10 = 42169, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWood11 = 42170, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWood12 = 42171, // Boss->self, no cast, range 30 180-degree cone

    SentinelRoar = 39491, // Boss->self, 5.0s cast, range 40 circle
    Level5DeathSentence = 39492, // Boss->self, 5.0s cast, range 30 circle
    WhisperOfTheWood1 = 39493, // Boss->self, 3.0s cast, single-target
    WhisperOfTheWood2 = 39494, // Boss->self, 3.0s cast, single-target
    WhisperOfTheWood3 = 39495 // Boss->self, 3.0s cast, single-target
}

class Level5DeathSentence(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.Level5DeathSentence), true, false, "Applies Doom!");
class SentinelRoar(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SentinelRoar));
class WordOfTheWood(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CastWordOfTheWood), new AOEShapeCone(30, 90.Degrees()));

class WhispersOfTheWood(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly Angle a180 = 180.Degrees(), a90 = 90.Degrees(), a0 = 0.Degrees();
    private static readonly AOEShapeCone cone = new(30, a90);
    private readonly List<AOEInstance> _aoes = [];
    private static readonly HashSet<AID> castEnd = [AID.WordOfTheWood1, AID.WordOfTheWood2, AID.WordOfTheWood3, AID.WordOfTheWood4, AID.WordOfTheWood5,
    AID.WordOfTheWood6, AID.WordOfTheWood7, AID.WordOfTheWood8, AID.WordOfTheWood9, AID.WordOfTheWood10, AID.WordOfTheWood11, AID.WordOfTheWood12];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count != 0)
        {
            for (var i = 0; i < count; ++i)
            {
                var aoe = _aoes[i];
                if (i == 0)
                    yield return count > 1 ? aoe with { Color = Colors.Danger } : aoe;
                else if (i == 1)
                    yield return _aoes[0].Rotation.AlmostEqual(_aoes[1].Rotation + a180, Angle.DegToRad) ? aoe with { Risky = false } : aoe;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.WhisperOfTheWood1:
                AddAOEs([a0, -a90, a90, a180], spell);
                break;
            case AID.WhisperOfTheWood2:
                AddAOEs([a90, a0, a180, -a90], spell);
                break;
            case AID.WhisperOfTheWood3:
                AddAOEs([a180, a90, -a90, a0], spell);
                break;
        }
    }

    private void AddAOEs(ReadOnlySpan<Angle> angles, ActorCastInfo spell)
    {
        for (var i = 0; i < 4; ++i)
        {
            var angle = (i == 0 ? spell.Rotation : _aoes[i - 1].Rotation) + angles[i];
            _aoes.Add(new(cone, Module.PrimaryActor.Position, angle, Module.CastFinishAt(spell, 9.3f + 2 * i)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && castEnd.Contains((AID)spell.Action.ID))
            _aoes.RemoveAt(0);
    }
}

class NechucihoStates : StateMachineBuilder
{
    public NechucihoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Level5DeathSentence>()
            .ActivateOnEnter<SentinelRoar>()
            .ActivateOnEnter<WordOfTheWood>()
            .ActivateOnEnter<WhispersOfTheWood>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin, Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13362)]
public class Nechuciho(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
