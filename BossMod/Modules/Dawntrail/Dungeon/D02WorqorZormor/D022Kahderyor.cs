namespace BossMod.Dawntrail.Dungeon.D02WorqorZormor.D022Kahderyor;

public enum OID : uint
{
    Boss = 0x415D, // R7.0
    CrystallineDebris = 0x415E, // R1.4
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    WindUnbound = 36282, // Boss->self, 5.0s cast, range 60 circle

    CrystallineCrushVisual = 36285, // Boss->location, 5.0+1.0s cast, single-target
    CrystallineCrush = 36153, // Helper->self, 6.3s cast, range 6 circle, tower

    WindShotVisual1 = 36284, // Boss->self, 5.5s cast, single-target
    WindshotVisual2 = 36300, // Helper->player, no cast, single-target
    WindShot = 36296, // Helper->players, 6.0s cast, range 5-10 donut, stack

    EarthenShotVisual1 = 36283, // Boss->self, 5.0+0.5s cast, single-target
    EarthenShotVisual2 = 36299, // Helper->player, no cast, single-target
    EarthenShot = 36295, // Helper->player, 6.0s cast, range 6 circle, spread

    CrystallineStormVisual = 36286, // Boss->self, 3.0+1.0s cast, single-target
    CrystallineStorm = 36290, // Helper->self, 4.0s cast, range 50 width 2 rect

    SeedCrystalsVisual = 36291, // Boss->self, 4.5+0.5s cast, single-target
    SeedCrystals = 36298, // Helper->player, 5.0s cast, range 6 circle, spread

    SharpenedSights = 36287, // Boss->self, 3.0s cast, single-target
    EyeOfTheFierce = 36297, // Helper->self, 5.0s cast, range 60 circle

    StalagmiteCircleVisual = 36288, // Boss->self, 5.0s cast, single-target
    StalagmiteCircle = 36293, // Helper->self, 5.0s cast, range 15 circle

    CyclonicRingVisual = 36289, // Boss->self, 5.0s cast, single-target
    CyclonicRing = 36294 // Helper->self, 5.0s cast, range 8-40 donut
}

public enum IconID : uint
{
    WindShot = 511 // player
}

class WindEarthShot(BossModule module) : Components.GenericAOEs(module)
{
    private const string Hint = "Be inside a crystal line!";
    private static readonly AOEShapeDonut donut = new(8f, 50f);
    private static readonly AOEShapeCircle circle = new(15f);
    private static readonly Angle[] angles = [-119.997f.Degrees(), -29.996f.Degrees(), 80.001f.Degrees(), -99.996f.Degrees()];
    private static readonly WPos[] positions = [new(-43, -57), new(-63, -57), new(-53, -47), new(-53, -67)];
    private const float Length = 50f;
    private const uint State = 0x00800040;
    private static readonly AOEShapeCustom ENVC21Inverted = CreateShape(positions[1], positions[2], positions[3], angles[1], angles[0], angles[2], 1, true);
    private static readonly AOEShapeCustom ENVC21 = CreateShape(positions[1], positions[2], positions[3], angles[1], angles[0], angles[2], 7);
    private static readonly AOEShapeCustom ENVC20Inverted = CreateShape(positions[0], positions[2], positions[3], angles[1], angles[3], angles[0], 1, true);
    private static readonly AOEShapeCustom ENVC20 = CreateShape(positions[0], positions[2], positions[3], angles[1], angles[3], angles[0], 7);
    public AOEInstance? AOE;
    private static readonly WDir am40 = -40f.Degrees().ToDirection(), a0 = new(0f, 1f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(AOE);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state is State or 0x00200010)
            AddAOE(index, state, state == State ? donut : circle);
        else if (state is 0x02000001 or 0x04000004 or State or 0x08000004 or 0x01000001)
            AOE = null;
    }

    private void AddAOE(byte index, uint state, AOEShape shape)
    {
        var activation = WorldState.FutureTime(5.9d);
        var color = state == State ? Colors.SafeFromAOE : 0;
        AOE = index switch
        {
            0x1E => new(shape, positions[0], default, activation),
            0x1F => new(shape, positions[1], default, activation),
            0x20 => new(state == State ? ENVC20Inverted : ENVC20, Arena.Center, default, activation, color),
            0x21 => new(state == State ? ENVC21Inverted : ENVC21, Arena.Center, default, activation, color),
            _ => AOE
        };
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (AOE == null)
            return;
        var aoeShape = AOE.Value.Shape;
        if (aoeShape != ENVC20Inverted && aoeShape != ENVC21Inverted)
            base.AddHints(slot, actor, hints);
        else if (!AOE.Value.Check(actor.Position))
            hints.Add(Hint);
        else
            hints.Add(Hint, false);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (AOE == null)
            return;
        base.AddAIHints(slot, actor, assignment, hints);

        var aoeShape = AOE.Value.Shape;
        var containsENVC20 = aoeShape == ENVC20;
        var containsENVC21 = aoeShape == ENVC21;

        if (containsENVC20 || containsENVC21)
        {
            var forbiddenZone = actor.Role != Role.Tank
                ? ShapeDistance.Rect(Arena.Center, containsENVC20 ? am40 : a0, 20f, containsENVC20 ? 1f : 10f, 20f)
                : ShapeDistance.InvertedCircle(Arena.Center, 12f);

            hints.AddForbiddenZone(forbiddenZone, AOE.Value.Activation);
        }
    }

    private static AOEShapeCustom CreateShape(WPos pos1, WPos pos2, WPos pos3, Angle angle1, Angle angle2, Angle angle3, int halfWidth, bool inverted = false)
        => new([new Rectangle(pos1, halfWidth, Length, angle1), new Rectangle(pos2, halfWidth, Length, angle2), new Rectangle(pos3, halfWidth, Length, angle3)],
        InvertForbiddenZone: inverted);
}

class WindShotStack(BossModule module) : Components.DonutStack(module, ActionID.MakeSpell(AID.WindShot), (uint)IconID.WindShot, 5f, 10f, 6f, 4, 4)
{
    private readonly WindEarthShot _aoe = module.FindComponent<WindEarthShot>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveStacks.Count == 0)
            return;

        var comp = _aoe.AOE;
        if (comp == null)
            return;

        var aoe = comp.Value;
        var forbidden = new List<Func<WPos, float>>(3);
        var party = Raid.WithoutSlot(false, true, true);

        for (var i = 0; i < party.Length; ++i)
        {
            var p = party[i];
            if (p == actor)
                continue;

            var addForbidden = false;
            if (aoe.Shape is AOEShapeDonut && !aoe.Check(p.Position) || aoe.Shape is AOEShapeCustom && aoe.Check(p.Position))
                addForbidden = true;
            if (addForbidden)
                forbidden.Add(ShapeDistance.InvertedCircle(p.Position, 1.66f));
        }

        if (forbidden.Count != 0)
            hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), ActiveStacks[0].Activation);
    }
}

class WindUnbound(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.WindUnbound));
class CrystallineCrush(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.CrystallineCrush), 6f, 4, 4);
class EarthenShot(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.EarthenShot), 6f);
class StalagmiteCircle(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.StalagmiteCircle), 15f);
class CrystallineStorm(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CrystallineStorm), new AOEShapeRect(50f, 1f));
class CyclonicRing(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CyclonicRing), new AOEShapeDonut(8f, 40f));
class EyeOfTheFierce(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.EyeOfTheFierce));
class SeedCrystals(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.SeedCrystals), 6f);

class D022KahderyorStates : StateMachineBuilder
{
    public D022KahderyorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WindEarthShot>()
            .ActivateOnEnter<WindShotStack>()
            .ActivateOnEnter<WindUnbound>()
            .ActivateOnEnter<CrystallineStorm>()
            .ActivateOnEnter<CrystallineCrush>()
            .ActivateOnEnter<EarthenShot>()
            .ActivateOnEnter<StalagmiteCircle>()
            .ActivateOnEnter<CyclonicRing>()
            .ActivateOnEnter<EyeOfTheFierce>()
            .ActivateOnEnter<SeedCrystals>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 824, NameID = 12703)]
public class D022Kahderyor(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultBounds.Center, DefaultBounds)
{
    public static readonly ArenaBoundsComplex DefaultBounds = new([new Polygon(new(-53f, -57f), 19.5f, 40)], [new Rectangle(new(-72.5f, -57f), 0.75f, 20), new Rectangle(new(-53f, -37f), 20f, 1.5f)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.CrystallineDebris), Colors.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.CrystallineDebris => 1,
                _ => 0
            };
        }
    }
}
