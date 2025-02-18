namespace BossMod.Shadowbringers.Dungeon.D02DohnMheg.D021AencThon;

public enum OID : uint
{
    Boss = 0x3F2, // R=2.0
    GeyserHelper1 = 0x1EAAA1, // controls animations for 2 geysers
    GeyserHelper2 = 0x1EAAA2, // controls animations for 3 geysers
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    CandyCane = 8857, // Boss->player, 4.0s cast, single-target
    HydrofallVisual = 8871, // Boss->self, 3.0s cast, single-target
    Hydrofall = 8893, // Helper->location, 3.0s cast, range 6 circle
    LaughingLeapAOE = 8852, // Boss->location, 4.0s cast, range 4 circle
    LaughingLeapStack = 8840, // Boss->players, no cast, range 4 circle
    LandsbloodFirst = 7822, // Boss->self, 3.0s cast, range 40 circle
    LandsbloodRepeat = 7899, // Boss->self, no cast, range 40 circle
    Geyser = 8800 // Helper->self, no cast, range 6 circle
}

public enum IconID : uint
{
    Stackmarker = 62 // player
}

class Landsblood(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.LandsbloodFirst), "Raidwides + Geysers");
class CandyCane(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.CandyCane));
class Hydrofall(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Hydrofall), 6f);
class LaughingLeap(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LaughingLeapAOE), 4f);
class LaughingLeapStack(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stackmarker, ActionID.MakeSpell(AID.LaughingLeapStack), 4f, 5.1f, 4, 4);

class Geyser(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(6f);

    private static readonly Dictionary<uint, Dictionary<Angle, WPos[]>> GeyserPositions = new()
    {
        {
            (uint)OID.GeyserHelper1, new Dictionary<Angle, WPos[]>
            {
                { 0.Degrees(), [new(0f, 14.16f), new(-9f, 45.16f)] },
                { 180.Degrees(), [new(9f, 15.16f), new(0f, 46.16f)] },
                { -90.Degrees(), [new(-15f, 21.16f), new(16f, 30.16f)] },
                { 90.Degrees(), [new(-16f, 30.16f), new(15f, 39.16f)] }
            }
        },
        {
            (uint)OID.GeyserHelper2, new Dictionary<Angle, WPos[]>
            {
                { 0.Degrees(), [new(0f, 35.16f), new(-9f, 15.16f), new(7f, 23.16f)] },
                { 90.Degrees(),  [new(-15f, 39.16f), new(-7f, 23.16f), new(5f, 30.16f)] },
                { 180.Degrees(), [new(9f, 45.16f), new(-7f, 37.16f), new(0f, 25.16f)] },
                { -90.Degrees(), [new(7f, 37.16f), new(15f, 21.16f), new(-5f, 30.16f)] }
            }
        }
    };

    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        var act0 = _aoes[0].Activation;
        var color = Colors.Danger;
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            var act = aoe.Activation;
            aoes[i] = aoe with { Color = act != _aoes[count - 1].Activation ? color : 0, Risky = act == act0 };
        }
        return aoes;
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00100020)
        {
            if (GeyserPositions.TryGetValue(actor.OID, out var positionsByRotation))
            {
                var activation = WorldState.FutureTime(5.1f);
                foreach (var (rotation, positions) in positionsByRotation)
                    if (actor.Rotation.AlmostEqual(rotation, Angle.DegToRad))
                    {
                        for (var i = 0; i < positions.Length; ++i)
                            _aoes.Add(new(circle, WPos.ClampToGrid(positions[i]), default, activation));
                        break;
                    }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.Geyser)
            _aoes.RemoveAt(0);
    }
}

class D021AencThonStates : StateMachineBuilder
{
    public D021AencThonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CandyCane>()
            .ActivateOnEnter<Landsblood>()
            .ActivateOnEnter<Hydrofall>()
            .ActivateOnEnter<LaughingLeap>()
            .ActivateOnEnter<LaughingLeapStack>()
            .ActivateOnEnter<Geyser>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 649, NameID = 8141)]
public class D021AencThon(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(0f, 30f), 19.5f * CosPI.Pi32th, 32)], [new Rectangle(new(0f, 50f), 20f, 1f), new Rectangle(new(0f, 10f), 20f, 1.4f)]);
}
