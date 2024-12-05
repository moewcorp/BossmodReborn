namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarAiravata;

public enum OID : uint
{
    Boss = 0x2543, //R=4.75
    AltarMatanga = 0x2545, // R3.42
    GoldWhisker = 0x2544, // R0.54
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // GoldWhisker->player, no cast, single-target
    AutoAttack2 = 872, // Boss/AltarMatanga->player, no cast, single-target

    Huff = 13371, // Boss->player, 3.0s cast, single-target
    HurlBoss = 13372, // Boss->location, 3.0s cast, range 6 circle
    Buffet = 13374, // Boss->player, 3.0s cast, single-target, knockback 20 forward
    SpinBoss = 13373, // Boss->self, 4.0s cast, range 30 120-degree cone
    BarbarousScream = 13375, // Boss->self, 3.5s cast, range 14 circle

    MatangaActivate = 9636, // AltarMatanga->self, no cast, single-target
    Spin = 8599, // AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // AltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // AltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 9630 // AltarMatanga/GoldWhisker->self, no cast, single-target, bonus adds disappear
}

public enum IconID : uint
{
    BuffetTarget = 23 // player
}

class HurlBoss(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HurlBoss), 6);
class SpinBoss(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SpinBoss), new AOEShapeCone(30, 60.Degrees()));
class BarbarousScream(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BarbarousScream), new AOEShapeCircle(13));
class Huff(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.Huff));

class Buffet(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Buffet), 20, kind: Kind.DirForward, stopAtWall: true)
{
    private bool targeted;
    private Actor? target;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BuffetTarget)
        {
            targeted = true;
            target = actor;
        }
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor) => target == actor ? base.Sources(slot, actor) : [];

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if ((AID)spell.Action.ID == AID.Buffet)
            targeted = false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (target == actor && targeted)
            hints.Add("Bait away!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (target == actor && targeted)
            hints.AddForbiddenZone(ShapeDistance.Circle(Module.Center, 17.5f));
    }
}

class Buffet2(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.Buffet), new AOEShapeCone(30, 60.Degrees()), true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var b in ActiveBaitsNotOn(actor))
            hints.AddForbiddenZone(b.Shape, b.Target.Position - (b.Target.HitboxRadius + Module.PrimaryActor.HitboxRadius) * Module.PrimaryActor.DirectionTo(b.Target), b.Rotation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var b in ActiveBaitsOn(pc))
            b.Shape.Outline(Arena, b.Target.Position - (b.Target.HitboxRadius + Module.PrimaryActor.HitboxRadius) * Module.PrimaryActor.DirectionTo(b.Target), b.Rotation);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var b in ActiveBaitsNotOn(pc))
            b.Shape.Draw(Arena, b.Target.Position - (b.Target.HitboxRadius + Module.PrimaryActor.HitboxRadius) * Module.PrimaryActor.DirectionTo(b.Target), b.Rotation);
    }
}

class RaucousScritch(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RaucousScritch), new AOEShapeCone(8.42f, 30.Degrees()));
class Hurl(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Hurl), 6);
class Spin(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCone(9.42f, 60.Degrees()), (uint)OID.AltarMatanga);

class AltarAiravataStates : StateMachineBuilder
{
    public AltarAiravataStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HurlBoss>()
            .ActivateOnEnter<SpinBoss>()
            .ActivateOnEnter<BarbarousScream>()
            .ActivateOnEnter<Huff>()
            .ActivateOnEnter<Buffet>()
            .ActivateOnEnter<Buffet2>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => Module.WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7601)]
public class AltarAiravata(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.GoldWhisker).Concat(Enemies(OID.AltarMatanga)), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.GoldWhisker => 2,
                OID.AltarMatanga => 1,
                _ => 0
            };
        }
    }
}
