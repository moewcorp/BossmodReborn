namespace BossMod.Global.MaskedCarnivale.Stage29.Act2;

public enum OID : uint
{
    Boss = 0x2C5D, //R=3.0
    FireTornado = 0x2C60, // R=4.0
    LeftHand = 0x2C5F, // R=1.3
    RightHand = 0x2C5E, // R=1.8
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 6499, // Boss->player, no cast, single-target
    AutoAttack2 = 18980, // LeftHand->player, no cast, single-target
    AutoAttack3 = 18976, // RightHand->player, no cast, single-target

    Unknown = 18975, // Boss->self, no cast, single-target
    ProteanWave1 = 18971, // Boss->self, 3.0s cast, range 39 30-degree cone, knockback 40, away from source (only first wave)
    ProteanWave2 = 18972, // Boss->self, no cast, range 39 30-degree cone, hits player position, but can be dodged. angle snapshot sometime between cast event of wave 1 and wave 2, not sure if its possible to predict accurately
    ProteanWave3 = 18984, // FireTornado->self, 3.0s cast, range 50 30-degree cone
    Throttle = 18964, // Boss->player, no cast, range 5 circle
    FerrofluidKB = 18963, // Boss->self, 5.0s cast, range 80 circle, knockback 6, away from source
    FluidConvection = 18974, // Boss->self, no cast, range 10-40 donut
    FerrofluidAttract = 18962, // Boss->self, 5.0s cast, range 80 circle, pull 6, between centers
    FluidDynamic = 18973, // Boss->self, no cast, range 6 circle
    FluidBallVisual = 18968, // Boss->self, 3.0s cast, single-target
    FluidBall = 18969, // Helper->location, 3.0s cast, range 5 circle
    WateryGrasp = 19028, // Boss->self, 5.0s cast, single-target, calls left/right hand, happens at 70% max hp
    FluidStrike = 18981, // LeftHand->self, no cast, range 12 90-degree cone
    FluidStrike2 = 18977, // RightHand->self, no cast, range 12 90-degree cone
    BigSplashFirst = 18965, // Boss->self, 8.0s cast, range 80 circle, knockback 25, away from source, use diamondback to survive
    BigSplashRepeat = 18966, // Boss->self, no cast, range 80 circle, knockback 25, away from source
    Cascade = 19022, // Boss->self, 4.0s cast, range 80 circle, raidwide, summons water tornados
    Unwind = 18985, // FireTornado->self, 5.0s cast, range 80 circle, damage fall of AOE, about 10 distance is fine
    FluidSwing = 18961, // Boss->player, 4.0s cast, range 11 30-degree cone, interruptible, knockback 50, source forward
    Palmistry = 18982, // LeftHand->player, 3.0s cast, single-target, drains 5000 MP
    WashAwayFirst = 18978, // RightHand->self, 5.0s cast, range 80 circle, multiple raidwides, add should be killed before cast finishes
    WashAwayRest = 18979 // RightHand->self, no cast, range 80 circle
}

public enum SID : uint
{
    Throttle = 700, // Boss->player, extra=0x0, player dies when debuff runs out if not cleansed before
    Stun = 149 // Boss->player, extra=0x0
}

class BigSplash(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BigSplashFirst), "Diamondback! (Multiple raidwides + knockbacks)");
class BigSplashKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.BigSplashFirst), 25);
class Cascade(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Cascade), "Raidwide + Tornados spawn");
class WateryGrasp(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.WateryGrasp), "Spawns hands. Focus left hand first.");
class Throttle(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.Throttle), "Prepare to use Excuviation to remove debuff");
class FluidSwing(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.FluidSwing));
class FluidSwingKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.FluidSwing), 50, kind: Kind.DirForward);

abstract class ProteanWave(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(39, 15.Degrees()));
class ProteanWave1(BossModule module) : ProteanWave(module, AID.ProteanWave1);
class ProteanWave3(BossModule module) : ProteanWave(module, AID.ProteanWave3);

class KnockbackPull(BossModule module) : Components.Knockback(module)
{
    private Source? _knockback;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_knockback);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FerrofluidKB)
            _knockback = new(Module.PrimaryActor.Position, 6, Module.CastFinishAt(spell));
        else if ((AID)spell.Action.ID == AID.FerrofluidAttract)
            _knockback = new(Module.PrimaryActor.Position, 6, Module.CastFinishAt(spell), Kind: Kind.TowardsOrigin);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FerrofluidKB or AID.FerrofluidAttract)
            _knockback = null;
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<FluidConvectionDynamic>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.InBounds(pos);
}

class Unwind(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Unwind), 10);
class FluidBall(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FluidBall), 5);

class FluidConvectionDynamic(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(10, 40);
    private static readonly AOEShapeCircle circle = new(6);
    private DateTime _activation;
    private AOEShape? shape;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (shape != default)
            yield return new(shape, Module.PrimaryActor.Position, default, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // boss can move after cast started, so we can't use aoe instance, since that would cause outdated position data to be used
        if ((AID)spell.Action.ID == AID.FerrofluidKB)
        {
            shape = donut;
            _activation = Module.CastFinishAt(spell);
        }
        else if ((AID)spell.Action.ID == AID.FerrofluidAttract)
        {
            shape = circle;
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FluidConvection or AID.FluidDynamic)
            shape = null;
    }
}

class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        var lefthand = Module.Enemies(OID.LeftHand).FirstOrDefault(x => !x.IsDead);
        var righthand = Module.Enemies(OID.RightHand).FirstOrDefault(x => !x.IsDead);
        if (lefthand != null)
            hints.Add($"{lefthand.Name} will drain all your MP, kill it fast!");
        if (lefthand == null && righthand != null)
            hints.Add($"{righthand.Name} will do multiple raidwides, kill it fast!");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var doomed = actor.FindStatus(SID.Throttle); //it is called throttle, but works exactly like any cleansable doom
        if (doomed != null)
            hints.Add("You were doomed! Cleanse it with Exuviation.");
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} will cast Throttle on you which needs to be\ncleansed with Excuviation. It will also spawn two hands which need to be\nkilled asap. Focus the left hand first because it will drain all your MP.");
    }
}

class Stage29Act2States : StateMachineBuilder
{
    public Stage29Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FluidSwing>()
            .ActivateOnEnter<FluidSwingKnockback>()
            .ActivateOnEnter<BigSplash>()
            .ActivateOnEnter<BigSplashKB>()
            .ActivateOnEnter<Cascade>()
            .ActivateOnEnter<WateryGrasp>()
            .ActivateOnEnter<Throttle>()
            .ActivateOnEnter<ProteanWave1>()
            .ActivateOnEnter<ProteanWave3>()
            .ActivateOnEnter<KnockbackPull>()
            .ActivateOnEnter<FluidBall>()
            .ActivateOnEnter<Unwind>()
            .ActivateOnEnter<FluidConvectionDynamic>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 698, NameID = 9241, SortOrder = 2)]
public class Stage29Act2 : BossModule
{
    public Stage29Act2(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.LeftHand));
        Arena.Actors(Enemies(OID.RightHand));
    }
}
