namespace BossMod.Stormblood.Dungeon.D03BardamsMettle.D032HunterOfBardam;

public enum OID : uint
{
    Boss = 0x1AA5, // R2.2
    Bardam = 0x1AA3, // R15.0
    ThrowingSpear = 0x1F49, // R1.25
    StarShard = 0x1F4A, // R2.4
    LoomingShadow = 0x1F4D, // R1.0
    WarriorOfBardam = 0x1AA4, // R1.1
    Helper = 0x18D6
}

public enum AID : uint
{
    Visual1 = 9450, // Helper->player, no cast, single-target
    Visual2 = 9451, // Helper->player, no cast, single-target
    Visual3 = 9611, // StarShard->self, no cast, single-target

    Magnetism = 7944, // Boss->self, no cast, range 40+R circle, pull 40 between hitboxes
    EmptyGaze = 7940, // Boss->self, 6.5s cast, range 40+R circle
    Travail = 7935, // Bardam->self, no cast, single-target
    Charge = 9599, // ThrowingSpear->self, 2.5s cast, range 45+R width 5 rect

    Sacrifice = 7937, // Helper->location, 7.0s cast, range 3 circle, tower
    DivinePunishment = 7938, // Helper->self, no cast, range 40+R circle

    BardamsRing = 9601, // Helper->self, no cast, range 10-20 donut, stack donuts, 5.3s delay
    CometFirst = 9597, // Helper->location, 4.0s cast, range 4 circle
    CometRest = 9598, // Helper->location, 1.5s cast, range 4 circle

    HeavyStrike = 9591, // Boss/WarriorOfBardam->self, 4.0s cast, single-target
    HeavyStrike1 = 9592, // Helper->self, 4.0s cast, range 6+R 270-degree cone
    HeavyStrike2 = 9593, // Helper->self, 4.0s cast, range 6+R-12+R 270-degree donut segment
    HeavyStrike3 = 9594, // Helper->self, 4.0s cast, range 12+R-18+R 270-degree donut segment

    CometImpact = 9600, // StarShard->self, 4.0s cast, range 9 circle
    ReconstructVisual = 7933, // Bardam->self, no cast, single-target
    Reconstruct = 7934, // Helper->location, 4.0s cast, range 5 circle

    Tremblor = 9605, // Boss->self, 3.5s cast, single-target
    Tremblor1 = 9596, // Helper->self, 4.0s cast, range 10 circle
    Tremblor2 = 9595, // Helper->self, 4.0s cast, range 10-20 donut
    MeteorImpact = 9602 // LoomingShadow->self, 30.0s cast, ???
}

public enum IconID : uint
{
    BardamsRing = 58, // player
    ChasingAOE = 197 // player
}

class CometChase(BossModule module) : Components.StandardChasingAOEs(module, 4f, (uint)AID.CometFirst, (uint)AID.CometRest, 10f, 1.5f, 9, true, (uint)IconID.ChasingAOE)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell) { }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID is var id && id == ActionFirst || id == ActionRest)
        {
            Advance(spell.LocXZ, MoveDistance, WorldState.CurrentTime);
            if (Chasers.Count == 0)
            {
                ExcludedTargets = default;
                NumCasts = 0;
            }
        }
    }
}

class CometAOE(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.CometFirst, (uint)AID.CometRest], 4f);

class MeteorImpact(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.MeteorImpact, 50f, safeInsideHitbox: false)
{
    private DateTime activation;

    public override ReadOnlySpan<Actor> BlockerActors()
    {
        var boulders = Module.Enemies((uint)OID.StarShard);
        var count = boulders.Count;
        if (count == 0)
            return [];
        var actors = new List<Actor>();
        for (var i = 0; i < count; ++i)
        {
            var b = boulders[i];
            if (!b.IsDead)
                actors.Add(b);
        }
        return actors.Count == 1 ? CollectionsMarshal.AsSpan(actors) : [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Casters.Add(caster);
            activation = Module.CastFinishAt(spell);
        }
    }

    public override void Update()
    {
        if (BlockerActors().Length != 0 && Safezones.Count == 0)
        {
            Refresh();
            AddSafezone(activation);
        }
    }
}

class Charge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Charge, new AOEShapeRect(46.25f, 2.5f));
class EmptyGaze(BossModule module) : Components.CastGaze(module, (uint)AID.EmptyGaze);
class Sacrifice(BossModule module) : Components.CastTowers(module, (uint)AID.Sacrifice, 3f);
class Reconstruct(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Reconstruct, 5f);
class CometImpact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CometImpact, 9f);
class BardamsRing(BossModule module) : Components.DonutStack(module, (uint)AID.BardamsRing, (uint)IconID.BardamsRing, 10f, 20f, 3.5f, 4, 4);

class Tremblor(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Tremblor1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.Tremblor1 => 0,
                (uint)AID.Tremblor2 => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(1.5f));
        }
    }
}

class TremblorFinal(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Tremblor2, new AOEShapeDonut(10f, 20f))
{
    private readonly Tremblor _aoe = module.FindComponent<Tremblor>()!;

    public override void Update()
    {
        MaxCasts = _aoe.Sequences.Count != 0 ? 0 : 1;
    }
}

class HeavyStrike(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly Angle a135 = 135f.Degrees();
    private static readonly AOEShape[] _shapes = [new AOEShapeCone(6.5f, a135), new AOEShapeDonutSector(6.5f, 12.5f, a135), new AOEShapeDonutSector(12.5f, 18.5f, a135)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HeavyStrike)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell, 1), spell.Rotation);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count > 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.HeavyStrike1 => 0,
                (uint)AID.HeavyStrike2 => 1,
                (uint)AID.HeavyStrike3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(1.3d), spell.Rotation);
        }
    }
}

class D032HunterOfBardamStates : StateMachineBuilder
{
    public D032HunterOfBardamStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CometChase>()
            .ActivateOnEnter<CometAOE>()
            .ActivateOnEnter<Tremblor>()
            .ActivateOnEnter<TremblorFinal>()
            .ActivateOnEnter<HeavyStrike>()
            .ActivateOnEnter<Charge>()
            .ActivateOnEnter<EmptyGaze>()
            .ActivateOnEnter<BardamsRing>()
            .ActivateOnEnter<Sacrifice>()
            .ActivateOnEnter<CometImpact>()
            .ActivateOnEnter<Reconstruct>()
            .ActivateOnEnter<MeteorImpact>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 240, NameID = 6180)]
public class D032HunterOfBardam(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-25.46f, -32.82f), new(-25.08f, -32.37f), new(-24.46f, -32.23f), new(-23.34f, -32.04f), new(-20.11f, -31.35f),
    new(-19.58f, -31.16f), new(-17.25f, -29.93f), new(-16.76f, -29.55f), new(-16.38f, -29.2f), new(-16.05f, -28.81f),
    new(-15.52f, -27.91f), new(-15.18f, -27.5f), new(-14.23f, -26.97f), new(-13.8f, -26.6f), new(-12.47f, -25),
    new(-12.16f, -24.57f), new(-12.18f, -24.04f), new(-12.17f, -23.54f), new(-11.75f, -22.49f), new(-11.15f, -21.71f),
    new(-10.65f, -21.42f), new(-10.43f, -20.94f), new(-9.04f, -16.72f), new(-9.2f, -16.14f), new(-9.37f, -15.58f),
    new(-9.34f, -14.99f), new(-8.99f, -12.61f), new(-9.05f, -12.02f), new(-9.84f, -7.81f), new(-10, -7.32f),
    new(-10.53f, -6.4f), new(-10.75f, -5.89f), new(-11.13f, -4.87f), new(-12.64f, -1.95f), new(-13.03f, -1.47f),
    new(-13.77f, -0.63f), new(-14.16f, -0.31f), new(-20.94f, 4.33f), new(-35.25f, 4.44f), new(-36.01f, 4.45f),
    new(-36.43f, 4.14f), new(-37.4f, 3.32f), new(-37.88f, 3.1f), new(-39.43f, 2.55f), new(-39.87f, 2.21f),
    new(-41.98f, 0.15f), new(-42.6f, -0.52f), new(-43.06f, -0.92f), new(-44.19f, -2.02f), new(-44.54f, -2.45f),
    new(-46.02f, -4.79f), new(-46.26f, -5.32f), new(-46.88f, -6.85f), new(-47.05f, -7.33f), new(-47.14f, -7.84f),
    new(-47.18f, -8.98f), new(-47.49f, -9.5f), new(-47.59f, -10.08f), new(-47.73f, -11.34f), new(-47.78f, -11.94f),
    new(-47.57f, -13), new(-47.66f, -13.55f), new(-48.09f, -15.17f), new(-48.1f, -15.76f), new(-48.07f, -16.97f),
    new(-47.21f, -19.5f), new(-46.98f, -20.07f), new(-44.27f, -25.31f), new(-42.1f, -27.62f), new(-41.09f, -28.98f),
    new(-40.7f, -29.32f), new(-36.5f, -31.89f), new(-36.03f, -32.14f), new(-32.68f, -32.83f)];
    private static readonly ArenaBoundsComplex arena = new([new PolygonCustom(vertices)]);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = AIHints.Enemy.PriorityForbidden;
        }
    }
}
