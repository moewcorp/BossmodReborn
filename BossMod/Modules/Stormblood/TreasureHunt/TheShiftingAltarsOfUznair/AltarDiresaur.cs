namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarDiresaur;

public enum OID : uint
{
    Boss = 0x253A, //R=6.6
    AltarDragon = 0x256F, //R=4.0
    AltarMatanga = 0x2545, // R3.42
    GoldWhisker = 0x2544, // R0.54
    FireVoidzone = 0x1EA140,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss/GoldWhisker->player, no cast, single-target
    AutoAttack2 = 872, // AltarMatanga->player, no cast, single-target
    AutoAttack3 = 6497, // AltarDragon->player, no cast, single-target

    DeadlyHold = 13217, // Boss->player, 3.0s cast, single-target
    HeatBreath = 13218, // Boss->self, 3.0s cast, range 8+R 90-degree cone
    TailSmash = 13220, // Boss->self, 3.0s cast, range 20+R 90-degree cone
    RagingInferno = 13283, // Boss->self, 3.0s cast, range 60 circle
    Comet = 13835, // BossHelper->location, 3.0s cast, range 4 circle
    HardStomp = 13743, // 256F->self, 3.0s cast, range 6+R circle
    Fireball = 13219, // Boss->location, 3.0s cast, range 6 circle

    MatangaActivate = 9636, // AltarMatanga->self, no cast, single-target
    Spin = 8599, // AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // AltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // AltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 9630 // AltarMatanga/GoldWhisker->self, no cast, single-target, bonus adds disappear
}

public enum IconID : uint
{
    Baitaway = 23 // player
}

class DeadlyHold(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.DeadlyHold));
class HeatBreath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HeatBreath), new AOEShapeCone(14.6f, 45.Degrees()));
class TailSmash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TailSmash), new AOEShapeCone(26.6f, 45.Degrees()));
class RagingInferno(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.RagingInferno));
class Comet(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Comet), 4);
class HardStomp(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HardStomp), new AOEShapeCircle(10));
class Fireball(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, ActionID.MakeSpell(AID.Fireball), m => m.Enemies(OID.FireVoidzone).Where(z => z.EventState != 7), 0.7f);

class FireballBait(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeCircle circle = new(6);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Baitaway)
            CurrentBaits.Add(new(actor, actor, circle));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Fireball)
            ++NumCasts;
        if (NumCasts == 3)
        {
            CurrentBaits.Clear();
            NumCasts = 0;
        }
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
            hints.Add("Bait away! (3 times)");
    }
}

class RaucousScritch(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RaucousScritch), new AOEShapeCone(8.42f, 30.Degrees()));
class Hurl(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Hurl), 6);
class Spin(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCone(9.42f, 60.Degrees()), (uint)OID.AltarMatanga);

class AltarDiresaurStates : StateMachineBuilder
{
    public AltarDiresaurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DeadlyHold>()
            .ActivateOnEnter<HeatBreath>()
            .ActivateOnEnter<TailSmash>()
            .ActivateOnEnter<RagingInferno>()
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<HardStomp>()
            .ActivateOnEnter<Fireball>()
            .ActivateOnEnter<FireballBait>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => Module.WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7627)]
public class AltarDiresaur(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.AltarDragon));
        Arena.Actors(Enemies(OID.AltarMatanga).Concat(Enemies(OID.GoldWhisker)), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.GoldWhisker => 3,
                OID.AltarMatanga => 2,
                OID.AltarDragon => 1,
                _ => 0
            };
        }
    }
}
