namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretCladoselache;

public enum OID : uint
{
    Boss = 0x3027, //R=2.47
    SecretShark = 0x3028, //R=3.0 
    KeeperOfKeys = 0x3034, // R3.23
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // SecretShark->player, no cast, single-target
    AutoAttack2 = 872, // Boss->player, no cast, single-target

    TidalGuillotine = 21704, // Boss->self, 4.0s cast, range 13 circle
    ProtolithicPuncture = 21703, // Boss->player, 4.0s cast, single-target

    PelagicCleaver = 21705, // Boss->self, 3.5s cast, range 40 60-degree cone
    PelagicCleaverFirst = 21706, // Boss->self, 5.0s cast, range 40 60-degree cone
    PelagicCleaverRest = 21707, // Boss->self, no cast, range 40 60-degree cone

    BiteAndRun = 21709, // SecretShark->player, 5.0s cast, width 5 rect charge
    AquaticLance = 21708, // Boss->player, 5.0s cast, range 8 circle

    Telega = 9630, // KeeperOfKeys->self, no cast, single-target, bonus adds disappear
    Mash = 21767, // KeeperOfKeys->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // KeeperOfKeys->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // KeeperOfKeys->self, 4.0s cast, range 11 circle
    Scoop = 21768 // KeeperOfKeys->self, 4.0s cast, range 15 120-degree cone
}

public enum IconID : uint
{
    spreadmarker = 135, // player
    RotateCCW = 168, // Boss
    RotateCW = 167 // Boss
}

class PelagicCleaverRotation(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;
    private Angle _rotation;
    private DateTime _activation;
    private static readonly AOEShapeCone _shape = new(40, 30.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var increment = (IconID)iconID switch
        {
            IconID.RotateCW => -60.Degrees(),
            IconID.RotateCCW => 60.Degrees(),
            _ => default
        };
        if (increment != default)
        {
            _increment = increment;
            InitIfReady(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.PelagicCleaverFirst)
        {
            _rotation = spell.Rotation;
            _activation = Module.CastFinishAt(spell);
        }
        if (_rotation != default)
            InitIfReady(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PelagicCleaverFirst or AID.PelagicCleaverRest)
            AdvanceSequence(0, WorldState.CurrentTime);
    }

    private void InitIfReady(Actor source)
    {
        if (_rotation != default && _increment != default)
        {
            Sequences.Add(new(_shape, source.Position, _rotation, _increment, _activation, 2.1f, 6));
            _rotation = default;
            _increment = default;
        }
    }
}

class PelagicCleaver(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PelagicCleaver), new AOEShapeCone(40, 30.Degrees()));
class TidalGuillotine(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TidalGuillotine), new AOEShapeCircle(13));
class ProtolithicPuncture(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.ProtolithicPuncture));
class BiteAndRun(BossModule module) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(AID.BiteAndRun), 2.5f);
class AquaticLance(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.AquaticLance), 8);

class Spin(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCircle(11));
class Mash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Mash), new AOEShapeRect(13, 2));
class Scoop(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Scoop), new AOEShapeCone(15, 60.Degrees()));

class SecretCladoselacheStates : StateMachineBuilder
{
    public SecretCladoselacheStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PelagicCleaver>()
            .ActivateOnEnter<PelagicCleaverRotation>()
            .ActivateOnEnter<TidalGuillotine>()
            .ActivateOnEnter<ProtolithicPuncture>()
            .ActivateOnEnter<BiteAndRun>()
            .ActivateOnEnter<AquaticLance>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .Raw.Update = () => Module.WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9778)]
public class SecretCladoselache(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.SecretShark));
        Arena.Actors(Enemies(OID.KeeperOfKeys), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.KeeperOfKeys => 2,
                OID.SecretShark => 1,
                _ => 0
            };
        }
    }
}
