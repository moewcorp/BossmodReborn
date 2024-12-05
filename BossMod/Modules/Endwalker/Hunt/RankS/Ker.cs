namespace BossMod.Endwalker.Hunt.RankS.Ker;

public enum OID : uint
{
    Boss = 0x35CF // R8.000, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    MinaxGlare = 27635, // Boss->self, 6.0s cast, range 40 circle
    Heliovoid = 27636, // Boss->self, 6.0s cast, range 12 circle

    AncientBlizzard1 = 27637, // Boss->self, 6.0s cast, range 8-40 donut
    AncientBlizzard2 = 27652, // Boss->self, 6.0s cast, range 8-40 donut
    WhispersManifest3 = 27659, // Boss->self, 6.0s cast, range 8-40 donut (remembered skill from Whispered Incantation)

    WhisperedIncantation = 27645, // Boss->self, 5.0s cast, single-target, applies status to boss, remembers next skill

    EternalDamnation1 = 27647, // Boss->self, 6.0s cast, range 40 circle gaze, applies doom
    EternalDamnation2 = 27640, // Boss->self, 6.0s cast, range 40 circle gaze, applies doom
    WhispersManifest4 = 27654, // Boss->self, 6.0s cast, range 40 circle gaze, applies doom

    AncientFlare = 27704, // Boss->self, 6.0s cast, range 40 circle, applies pyretic
    AncientFlare2 = 27638, // Boss->self, 6.0s cast, range 40 circle, applies pyretic
    WhispersManifest1 = 27706, // Boss->self, 6.0s cast, range 40 circle, applies pyretc (remembered skill from Whispered Incantation)

    AncientHoly1 = 27646, // Boss->self, 6.0s cast, range 40 circle, circle with dmg fall off, harmless after ca. range 20 (it is hard to say because people accumulate vuln stacks which skews the damage fall off with distance from source)
    AncientHoly2 = 27639, // Boss->self, 6.0s cast, range 40 circle, circle with dmg fall off, harmless after around range 20
    WhispersManifest2 = 27653, // Boss->self, 6.0s cast, range 40 circle, circle with dmg fall off, harmless after around range 20

    MirroredIncantation1 = 27927, // Boss->self, 3.0s cast, single-target, mirrors the next 3 interments
    MirroredIncantation2 = 27928, // Boss->self, 3.0s cast, single-target, mirrors the next 4 interments
    MirroredRightInterment = 27663, // Boss->self, 6.0s cast, range 40 180-degree cone
    MirroredLeftInterment = 27664, // Boss->self, 6.0s cast, range 40 180-degree cone
    MirroredForeInterment = 27661, // Boss->self, 6.0s cast, range 40 180-degree cone
    MirroredRearInterment = 27662, // Boss->self, 6.0s cast, range 40 180-degree cone
    ForeInterment = 27641, // Boss->self, 6.0s cast, range 40 180-degree cone
    RearInterment = 27642, // Boss->self, 6.0s cast, range 40 180-degree cone
    RightInterment = 27643, // Boss->self, 6.0s cast, range 40 180-degree cone
    LeftInterment = 27644, // Boss->self, 6.0s cast, range 40 180-degree cone

    Visual = 25698, // Boss->player, no cast, single-target
}

public enum SID : uint
{
    WhisperedIncantation = 2846, // Boss->Boss, extra=0x0
    MirroredIncantation = 2848, // Boss->Boss, extra=0x3/0x2/0x1/0x4
    Pyretic = 960, // Boss->player, extra=0x0
    Doom = 1970, // Boss->player
    WhispersManifest = 2847, // Boss->Boss, extra=0x0
}

class MinaxGlare(BossModule module) : Components.TemporaryMisdirection(module, ActionID.MakeSpell(AID.MinaxGlare));
class Heliovoid(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Heliovoid), new AOEShapeCircle(12));

abstract class Blizzard(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeDonut(8, 40));
class AncientBlizzard1(BossModule module) : Blizzard(module, AID.AncientBlizzard1);
class AncientBlizzard2(BossModule module) : Blizzard(module, AID.AncientBlizzard2);
class AncientBlizzardWhispersManifest(BossModule module) : Blizzard(module, AID.WhispersManifest3);

abstract class AncientHoly(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(20));
class AncientHoly1(BossModule module) : AncientHoly(module, AID.AncientHoly1);
class AncientHoly2(BossModule module) : AncientHoly(module, AID.AncientHoly2);
class AncientHolyWhispersManifest(BossModule module) : AncientHoly(module, AID.WhispersManifest2);

abstract class Interment(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(40, 90.Degrees()));
class ForeInterment(BossModule module) : Interment(module, AID.ForeInterment);
class RearInterment(BossModule module) : Interment(module, AID.RearInterment);
class RightInterment(BossModule module) : Interment(module, AID.RightInterment);
class LeftInterment(BossModule module) : Interment(module, AID.LeftInterment);
class MirroredForeInterment(BossModule module) : Interment(module, AID.MirroredForeInterment);
class MirroredRearInterment(BossModule module) : Interment(module, AID.MirroredRearInterment);
class MirroredRightInterment(BossModule module) : Interment(module, AID.MirroredRightInterment);
class MirroredLeftInterment(BossModule module) : Interment(module, AID.MirroredLeftInterment);

class EternalDamnation1(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.EternalDamnation1));
class EternalDamnation2(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.EternalDamnation2));
class EternalDamnationWhispersManifest(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.WhispersManifest4));

class WhisperedIncantation(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.WhisperedIncantation), "Remembers the next skill and uses it again when casting Whispers Manifest");

class MirroredIncantation(BossModule module) : BossComponent(module)
{
    private int Mirrorstacks;
    private enum Types { None, Mirroredx3, Mirroredx4 }
    private Types Type;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MirroredIncantation1)
            Type = Types.Mirroredx3;
        else if ((AID)spell.Action.ID == AID.MirroredIncantation2)
            Type = Types.Mirroredx4;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MirroredIncantation1 or AID.MirroredIncantation2)
            Type = Types.None;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (actor == Module.PrimaryActor)
            switch ((SID)status.ID)
            {
                case SID.MirroredIncantation:
                    var stacks = status.Extra switch
                    {
                        0x1 => 1,
                        0x2 => 2,
                        0x3 => 3,
                        0x4 => 4,
                        _ => 0
                    };
                    Mirrorstacks = stacks;
                    break;
            }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (actor == Module.PrimaryActor)
        {
            if ((SID)status.ID == SID.MirroredIncantation)
                Mirrorstacks = 0;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Mirrorstacks > 0)
            hints.Add($"Mirrored interments left: {Mirrorstacks}!");
        else if (Type == Types.Mirroredx3)
            hints.Add("The next three interments will be mirrored!");
        else if (Type == Types.Mirroredx4)
            hints.Add("The next four interments will be mirrored!");
    }
}

class AncientFlare(BossModule module) : Components.StayMove(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AncientFlare or AID.AncientFlare2 or AID.WhispersManifest1)
            foreach (var m in Raid.WithSlot())
                SetState(m.Item1, new(Requirement.Stay, Module.CastFinishAt(spell)));
    }

    public override void Update() // we don't know which players will be affected by pyretic due to 32 player cap, so we need to do a trick to clear states again...
    {
        if (PlayerStates != default && PlayerStates[0].Activation.AddSeconds(2) < WorldState.CurrentTime)
            if (Raid.WithoutSlot().All(x => x.FindStatus(SID.Pyretic) == null))
                foreach (var p in Raid.WithSlot())
                    ClearState(p.Item1);
    }
}
// TODO: wicked swipe, check if there are even more skills missing

class KerStates : StateMachineBuilder
{
    public KerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MinaxGlare>()
            .ActivateOnEnter<Heliovoid>()
            .ActivateOnEnter<AncientBlizzard1>()
            .ActivateOnEnter<AncientBlizzard2>()
            .ActivateOnEnter<AncientBlizzardWhispersManifest>()
            .ActivateOnEnter<ForeInterment>()
            .ActivateOnEnter<RearInterment>()
            .ActivateOnEnter<RightInterment>()
            .ActivateOnEnter<LeftInterment>()
            .ActivateOnEnter<MirroredForeInterment>()
            .ActivateOnEnter<MirroredRearInterment>()
            .ActivateOnEnter<MirroredRightInterment>()
            .ActivateOnEnter<MirroredLeftInterment>()
            .ActivateOnEnter<MirroredIncantation>()
            .ActivateOnEnter<EternalDamnation1>()
            .ActivateOnEnter<EternalDamnation2>()
            .ActivateOnEnter<EternalDamnationWhispersManifest>()
            .ActivateOnEnter<AncientFlare>()
            .ActivateOnEnter<AncientHoly1>()
            .ActivateOnEnter<AncientHoly2>()
            .ActivateOnEnter<AncientHolyWhispersManifest>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.SS, NameID = 10615)]
public class Ker(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
