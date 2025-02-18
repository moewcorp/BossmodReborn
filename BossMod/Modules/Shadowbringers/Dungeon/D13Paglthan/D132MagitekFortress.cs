namespace BossMod.Shadowbringers.Dungeon.D13Paglthan.D132MagitekFortress;

public enum OID : uint
{
    Boss = 0x32FE, // R1.0
    MagitekCore = 0x31AC, // R2.3
    TemperedImperial = 0x31AD, // R0.5
    TelotekPredator = 0x31AF, // R2.1
    MagitekMissile = 0x31B2, // R1.0
    TelotekSkyArmor = 0x31B0, // R2.0
    MarkIITelotekColossus = 0x31AE, // R3.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // TemperedImperial/TelotekPredator/TelotekSkyArmor/MarkIITelotekColossus->player, no cast, single-target

    MagitekClaw = 23706, // TelotekPredator->player, 4.0s cast, single-target, mini tank buster, can be ignored
    StableCannon = 23700, // Helper->self, no cast, range 60 width 10 rect
    TwoTonzeMagitekMissile = 23701, // Helper->location, 5.0s cast, range 12 circle
    GroundToGroundBallistic = 23703, // Helper->location, 5.0s cast, range 40 circle, knockback 10, away from source
    MissileActivation = 10758, // MagitekMissile->self, no cast, single-target
    ExplosiveForce = 23704, // MagitekMissile->player, no cast, single-target
    DefensiveReaction = 23710, // MagitekCore->self, 5.0s cast, range 60 circle
    Aethershot = 23708, // TelotekSkyArmor->location, 4.0s cast, range 6 circle
    Exhaust = 23705 // MarkIITelotekColossus->self, 4.0s cast, range 40 width 7 rect
}

class TwoTonzeMagitekMissile(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TwoTonzeMagitekMissile), 12);
class Aethershot(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Aethershot), 6);
class DefensiveReaction(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DefensiveReaction));
class Exhaust(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Exhaust), new AOEShapeRect(40, 3.5f));
class GroundToGroundBallistic(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.GroundToGroundBallistic), 10)
{
    private static readonly Func<WPos, float> distance = p => Math.Max(ShapeDistance.InvertedCone(D132MagitekFortress.DefaultCenter, 20, default, 18.Degrees())(p),
    ShapeDistance.InvertedCone(D132MagitekFortress.DefaultCenter, 20, 180.Degrees(), 18.Degrees())(p));

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var source = Sources(slot, actor).FirstOrDefault();
        if (source != default)
            hints.AddForbiddenZone(distance, source.Activation);
    }
}

class StableCannon(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(60, 5);
    private readonly List<AOEInstance> _aoes = new(2);

    private static readonly Dictionary<byte, WPos> aoePositions = new()
    {
        { 0x08, new(-185, 28.3f) },
        { 0x09, new(-175, 28.3f) },
        { 0x0A, new(-165, 28.3f) }
    };

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00200010 && aoePositions.TryGetValue(index, out var value))
            _aoes.Add(new(rect, value, Angle.AnglesCardinals[1], WorldState.FutureTime(12.1f)));
        else if (index == 0x0D && state == 0x00020001)
            _aoes.Clear();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.StableCannon)
            _aoes.Clear();
    }
}

class MagitekMissile(BossModule module) : Components.GenericAOEs(module)
{
    private const int Radius = 1, Length = 10;
    private static readonly AOEShapeCapsule capsule = new(Radius, Length);
    private readonly List<Actor> _missiles = new(15);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _missiles.Count;
        if (count == 0)
            yield break;
        for (var i = 0; i < _missiles.Count; ++i)
        {
            var m = _missiles[i];
            yield return new(capsule, m.Position, m.Rotation);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.MagitekMissile)
            _missiles.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.MagitekMissile)
            _missiles.Remove(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ExplosiveForce)
            _missiles.Remove(caster);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_missiles.Count == 0)
            return;
        var forbidden = new List<Func<WPos, float>>(15);
        for (var i = 0; i < _missiles.Count; ++i)
        {
            var m = _missiles[i];
            forbidden.Add(ShapeDistance.Capsule(m.Position, m.Rotation, Length, Radius)); // merging all forbidden zones into one to make pathfinding less demanding
        }
        hints.AddForbiddenZone(ShapeDistance.Union(forbidden));
    }
}

class CorePlatform(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(2, true);
    private AOEInstance? _aoe;
    private const string Hint = "Walk into the glowing circle!";

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe != null && Arena.Bounds == D132MagitekFortress.DefaultBounds)
            yield return _aoe.Value;
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x0D)
        {
            if (state == 0x00020001)
                _aoe = new(circle, new(-175, 30), default, DateTime.MaxValue, Colors.SafeFromAOE);
            else if (state == 0x00080004)
                _aoe = null;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        if (D132MagitekFortress.CoreBounds.Contains(pc.Position - D132MagitekFortress.CoreCenter))
            SetArena(D132MagitekFortress.CoreBounds, D132MagitekFortress.CoreCenter);
        else
            SetArena(D132MagitekFortress.DefaultBounds, D132MagitekFortress.DefaultCenter);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe != null && Arena.Bounds == D132MagitekFortress.DefaultBounds)
        {
            if (!ActiveAOEs(slot, actor).Where(c => c.Color == Colors.SafeFromAOE).Any(c => c.Check(actor.Position)))
                hints.Add(Hint);
        }
    }

    private void SetArena(ArenaBounds bounds, WPos center)
    {
        Arena.Bounds = bounds;
        Arena.Center = center;
    }
}

class D132MagitekFortressStates : StateMachineBuilder
{
    public D132MagitekFortressStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CorePlatform>()
            .ActivateOnEnter<StableCannon>()
            .ActivateOnEnter<Aethershot>()
            .ActivateOnEnter<DefensiveReaction>()
            .ActivateOnEnter<Exhaust>()
            .ActivateOnEnter<TwoTonzeMagitekMissile>()
            .ActivateOnEnter<GroundToGroundBallistic>()
            .ActivateOnEnter<MagitekMissile>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 777, NameID = 10067)]
public class D132MagitekFortress(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultCenter, DefaultBounds)
{
    public static readonly WPos DefaultCenter = new(-175, 43), CoreCenter = new(-175, 8.5f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(14.5f), CoreBounds = new(7);
    private static readonly uint[] trash = [(uint)OID.TelotekPredator, (uint)OID.TemperedImperial, (uint)OID.TelotekSkyArmor, (uint)OID.MarkIITelotekColossus, (uint)OID.MagitekCore];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(trash));
    }

    protected override bool CheckPull() => Enemies(OID.TelotekPredator).Any(x => x.InCombat);
}
