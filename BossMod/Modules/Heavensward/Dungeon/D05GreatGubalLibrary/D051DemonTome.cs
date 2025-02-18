namespace BossMod.Heavensward.Dungeon.D05GreatGubalLibrary.D051DemonTome;

public enum OID : uint
{
    Boss = 0xE82, // R7.84
    IceFloor = 0x1E9944, // R0.5
    Helper = 0xED6
}

public enum AID : uint
{
    Blizzard = 3522, // Boss->player, no cast, single-target
    Repel = 3519, // Boss->self, 3.0s cast, range 40+R 180-degree cone, knockback 20, source forward
    WordsOfWinterVisual = 3517, // Boss->self, 4.0s cast, single-target
    WordsOfWinter = 3961, // Helper->self, 4.5s cast, range 40+R width 22 rect

    LiquefyCenter = 3520, // Helper->self, 3.0s cast, range 50+R width 8 rect
    LiquefySides = 3521, // Helper->self, 3.0s cast, range 50+R width 7 rect

    DisclosureVisual = 3518, // Boss->self, 8.0s cast, single-target
    Disclosure = 4818, // Helper->self, 8.0s cast, range 20+R width 22 rect
    DisclosureSpin = 3989, // Helper->self, no cast, range 12+R circle, knockback 20 away from source
    BetweenTheLines = 3926, // Boss->player, no cast, single-target, didn't switch sides in time, 25k dmg per hit
}

class Repel(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Repel), 20, true, kind: Kind.DirForward, stopAtWall: true);
class LiquefyCenter(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LiquefyCenter), new AOEShapeRect(57.84f, 4));
class LiquefySides(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LiquefySides), new AOEShapeRect(57.84f, 3.5f));

class Disclosure(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(12);
    private static readonly AOEShapeRect rect = new(25, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Disclosure)
        {
            if (spell.Rotation.AlmostEqual(-90.Degrees(), Angle.DegToRad))
                _aoes.Add(new(rect, new(3.667f, 0), spell.Rotation, Module.CastFinishAt(spell)));
            else if (spell.Rotation.AlmostEqual(90.Degrees(), Angle.DegToRad))
                _aoes.Add(new(rect, new(3.3f, 0), spell.Rotation, Module.CastFinishAt(spell)));
            Arena.Bounds = D051DemonTome.SpinArena;
            _aoes.Add(new(circle, Module.PrimaryActor.Position, default, Module.CastFinishAt(spell, 2.4f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.Disclosure or AID.DisclosureSpin)
        {
            _aoes.RemoveAt(0);
            Arena.Bounds = D051DemonTome.DefaultArena;
        }
    }
}

class ThinIce(BossModule module) : Components.ThinIce(module, 15, stopAtWall: true);

class D051DemonTomeStates : StateMachineBuilder
{
    public D051DemonTomeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ThinIce>()
            .ActivateOnEnter<LiquefyCenter>()
            .ActivateOnEnter<LiquefySides>()
            .ActivateOnEnter<Repel>()
            .ActivateOnEnter<Disclosure>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 31, NameID = 3923)]
public class D051DemonTome(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultArena.Center, DefaultArena)
{
    private static readonly WPos[] vertices = [new(19.49f, -9.58f), new(19.57f, -9.02f), new(19.57f, -8.51f), new(19.38f, -7.99f), new(19.23f, 7.30f),
    new(19.54f, 7.76f), new(19.56f, 8.86f), new(19.62f, 9.37f), new(-14.67f, 9.60f), new(-18.81f, 9.59f),
    new(-19.47f, 9.58f), new(-19.58f, 9.00f), new(-19.57f, 8.46f), new(-19.44f, 7.89f), new(-19.33f, -7.38f),
    new(-19.57f, -7.85f), new(-19.59f, -8.96f), new(-19.68f, -9.47f), new(-17.27f, -9.59f), new(-3.43f, -9.60f)];
    public static readonly ArenaBoundsComplex DefaultArena = new([new PolygonCustom(vertices)], [new RectangleSE(new(-3.666f, 0), new(3.29f, 0), 20)]);
    public static readonly ArenaBoundsComplex SpinArena = new([new PolygonCustom(vertices)], [new RectangleSE(new(-3.666f, 0), new(3.29f, 0), 4.5f)]);
}
