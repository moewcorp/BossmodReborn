namespace BossMod.Dawntrail.Dungeon.D09YuweyawataFieldStation.D092OverseerKanilokka;

public enum OID : uint
{
    Boss = 0x464A, // R9.0
    RawElectrope = 0x4642, // R1.0
    PreservedSoul = 0x464B, // R2.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 40659, // Boss->player, no cast, single-target

    DarkSouls = 40658, // Boss->player, 5.0s cast, single-target

    FreeSpiritsVisual = 40639, // Boss->self, 4.0+1,0s cast, single-target
    FreeSpirits = 40640, // Helper->self, 5.0s cast, range 20 circle

    Soulweave1 = 40642, // PreservedSoul->self, 2.5s cast, range ?-32 donut
    Soulweave2 = 40641, // PreservedSoul->self, 2.5s cast, range ?-32 donut

    PhantomFloodVisual = 40643, // Boss->self, 3.7+1,3s cast, single-target
    PhantomFlood = 40644, // Helper->self, 5.0s cast, range 5-20 donut

    DarkIIVisual1 = 40654, // Boss->self, 4.5+0,5s cast, single-target
    DarkIIVisual2 = 40655, // Boss->self, no cast, single-target
    DarkII1 = 40656, // Helper->self, 5.0s cast, range 35 30-degree cone
    DarkII2 = 40657, // Helper->self, 7.5s cast, range 35 30-degree cone

    TelltaleTears = 40649, // Helper->players, 5.0s cast, range 5 circle, spread
    LostHope = 40645, // Boss->self, 3.0s cast, range 20 circle
    Necrohazard = 40646, // Boss->self, 15.0s cast, range 20 circle, damage fall off AOE, very extreme damage if not almost at the border
    Bloodburst = 40647, // Boss->self, 5.0s cast, range 45 circle
    SoulDouse = 40651, // Helper->players, 5.0s cast, range 6 circle
}

class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donutSmall = new(5, 15), donutBig = new(15, 20);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FreeSpirits)
            _aoe = new(donutBig, Arena.Center, default, Module.CastFinishAt(spell));
        else if ((AID)spell.Action.ID == AID.PhantomFlood)
            _aoe = new(donutSmall, Arena.Center, default, Module.CastFinishAt(spell));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index != 0x07)
            return;
        switch (state)
        {
            case 0x00020001:
                SetArena(D092OverseerKanilokka.DefaultArena, D092OverseerKanilokka.DefaultArena.Center);
                break;
            case 0x00200010:
                SetArena(D092OverseerKanilokka.TinyArena, D092OverseerKanilokka.TinyArena.Center);
                break;
            case 0x00800040:
                SetArena(D092OverseerKanilokka.ArenaENVC00800040, D092OverseerKanilokka.ArenaENVC00800040.Center);
                break;
            case 0x02000100:
                SetArena(D092OverseerKanilokka.ArenaENVC02000100, D092OverseerKanilokka.ArenaENVC02000100.Center);
                break;
            case 0x00080004:
                SetArena(D092OverseerKanilokka.StartingBounds, D092OverseerKanilokka.StartingBounds.Center);
                break;
        }
        _aoe = null;
    }

    private void SetArena(ArenaBounds bounds, WPos center)
    {
        Arena.Bounds = bounds;
        Arena.Center = center;
    }
}

class Soulweave(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(28, 32);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count > 0)
        {
            for (var i = 0; i < count; ++i)
            {
                var aoe = _aoes[i];
                yield return (aoe.Activation - _aoes[0].Activation).TotalSeconds <= 1.2 ? aoe with { Color = Colors.Danger } : aoe with { Risky = false };
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Soulweave1 or AID.Soulweave2)
            _aoes.Add(new(donut, spell.LocXZ, default, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && (AID)spell.Action.ID is AID.Soulweave1 or AID.Soulweave2)
            _aoes.RemoveAt(0);
    }
}

class FreeSpirits(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FreeSpirits));
class Bloodburst(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Bloodburst));
class DarkSouls(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.DarkSouls));
class TelltaleTears(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.TelltaleTears), 5);
class SoulDouse(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.SoulDouse), 6, 4, 4);
class LostHope(BossModule module) : Components.TemporaryMisdirection(module, ActionID.MakeSpell(AID.LostHope));
class Necrohazard(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Necrohazard), new AOEShapeCircle(18));

class DarkII(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(35, 15.Degrees());
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(6);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DarkII1 or AID.DarkII2)
        {
            _aoes.Add(new(cone, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            if (_aoes.Count == 12)
                _aoes.SortBy(x => x.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && (AID)spell.Action.ID is AID.DarkII1 or AID.DarkII2)
            _aoes.RemoveAt(0);
    }
}

class D092OverseerKanilokkaStates : StateMachineBuilder
{
    public D092OverseerKanilokkaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<FreeSpirits>()
            .ActivateOnEnter<Soulweave>()
            .ActivateOnEnter<DarkSouls>()
            .ActivateOnEnter<DarkII>()
            .ActivateOnEnter<TelltaleTears>()
            .ActivateOnEnter<SoulDouse>()
            .ActivateOnEnter<Bloodburst>()
            .ActivateOnEnter<LostHope>()
            .ActivateOnEnter<Necrohazard>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1008, NameID = 13634, SortOrder = 6)]
public class D092OverseerKanilokka(WorldState ws, Actor primary) : BossModule(ws, primary, StartingBounds.Center, StartingBounds)
{
    private const int Edges = 60;
    public static readonly WPos ArenaCenter = new(116, -66);
    public static readonly Polygon[] StartingPolygon = [new Polygon(ArenaCenter, 19.5f * CosPI.Pi60th, Edges)];
    public static readonly Polygon[] TinyPolygon = [new Polygon(ArenaCenter, 5, Edges)];
    private static readonly WPos[] vertices02000100West = [new(111.06f, -65.72f), new(110.67f, -65.43f), new(107.318f, -65.444f), new(105.58f, -65.14f), new(104.667f, -64.333f),
    new(103.67f, -62.384f), new(103.178f, -60.442f), new(103.86f, -59.34f), new(103.95f, -57.884f), new(102.706f, -57.1f), new(100.07f, -58.374f), new(98.053f, -57.253f),
    new(99.5f, -54.79f), new(100.76f, -55.59f), new(103.6f, -54.06f), new(105.967f, -55.71f), new(106.658f, -56.668f), new(106.886f, -59.705f), new(107.185f, -61.381f),
    new(108.828f, -62.385f), new(110.195f, -62.339f), new(111.853f, -61.94f), new(112.56f, -61.859f), new(113.26f, -61.845f)];
    private static readonly WPos[] vertices02000100North = [new(118.766f, -70.106f), new(118.784f, -70.473f), new(118.395f, -72.065f), new(114.281f, -74.929f), new(114.281f, -76.494f),
    new(114.916f, -77.026f), new(115.96f, -77.269f), new(117.036f, -77.379f), new(118.31f, -77.806f), new(119.238f, -78.268f), new(120.02f, -78.853f), new(120.581f, -80.649f),
    new(120.215f, -83.173f), new(119.498f, -83.807f), new(117.335f, -85.069f), new(117.422f, -85.957f), new(114.534f, -86.06f), new(114.917f, -84.171f), new(116, -83.361f),
    new(117.7f, -82.228f), new(117.576f, -80.921f), new(117.118f, -80.047f), new(116, -79.764f), new(114.6f, -79.644f), new(112.1f, -78.814f), new(111.198f, -77.547f),
    new(110.9f, -76.129f), new(110.936f, -74.878f), new(113.477f, -72.316f), new(113.307f, -70.185f)];
    private static readonly WPos[] vertices02000100East = [new(118.87f, -61.951f), new(119.5f, -61.2f), new(119.5f, -58.872f), new(119.8f, -57.9f), new(123.96f, -55.915f),
    new(124.767f, -55.898f), new(126.807f, -56.914f), new(127.894f, -57.773f), new(128.759f, -59.09f), new(131.165f, -58.697f), new(131.542f, -57.7f), new(131.489f, -55.889f),
    new(131.625f, -55.403f), new(131.5f, -54.809f), new(133.929f, -57.215f), new(133.676f, -57.343f), new(133.483f, -60.665f), new(131.628f, -62.529f), new(129.073f, -62.57f),
    new(126.344f, -60.348f), new(124.493f, -59.185f), new(122.206f, -60.479f), new(122.439f, -61.998f), new(121.883f, -63.458f), new(120.931f, -65.486f)];
    private static readonly WPos[] vertices00800040North = [new(119.67f, -69.1f), new(119.72f, -72.25f), new(117.4f, -76.6f), new(116, -78.4f), new(117.5f, -79.8f), new(121, -81.2f),
    new(123.3f, -85.5f), new(119.9f, -85.5f), new(117, -83.8f), new(113.667f, -80.854f), new(113, -77.5f), new(115.883f, -72.848f), new(114, -70.5f)];
    private static readonly WPos[] vertices00800040East = [new(119.89f, -63), new(122.887f, -63.63f), new(126.394f, -65.31f), new(127.887f, -67.5f), new(129.465f, -67.817f),
    new(131.81f, -64.4f), new(136.6f, -62.82f), new(135.9f, -65.945f), new(133.18f, -66.62f), new(131.69f, -70.87f), new(127.1f, -71.56f), new(124.32f, -68.3f),
     new(122.6f, -67.54f), new(120.32f, -68.186f)];
    private static readonly WPos[] vertices00800040South = [new(112.684f, -62.388f), new(112.2f, -60.07f), new(107.836f, -57.852f), new(107.12f, -53.635f), new(111.456f, -49.98f),
    new(110.765f, -46.659f), new(113.8f, -46.079f), new(115.03f, -51.19f), new(111.5f, -53.67f), new(111.4f, -55.61f), new(116.356f, -58.773f), new(117.67f, -61.434f)];
    private static readonly WPos[] vertices00800040West = [new(112.793f, -69.754f), new(110.552f, -70.2f), new(108.04f, -73.04f), new(103.3f, -72.96f), new(100.9f, -70.243f),
    new(98.644f, -70.563f), new(97.194f, -72.67f), new(96.467f, -69.863f), new(98.48f, -67.5f), new(101.8f, -67), new(104.645f, -69.163f), new(106.676f, -69.848f),
     new(108.837f, -66.57f), new(111.04f, -65.8f)];
    public static readonly ArenaBoundsComplex StartingBounds = new(StartingPolygon, [new Rectangle(new(116, -46), 20, 1.25f), new Rectangle(new(116, -86), 20, 1.25f)]);
    public static readonly ArenaBoundsComplex DefaultArena = new([new Polygon(ArenaCenter, 15, Edges)]);
    public static readonly ArenaBoundsComplex TinyArena = new(TinyPolygon, MapResolution: 0.1f);
    private static readonly DonutV[] difference = [new DonutV(ArenaCenter, 19.5f, 22, Edges)];
    public static readonly ArenaBoundsComplex ArenaENVC00800040 = new([new PolygonCustom(vertices00800040North), new PolygonCustom(vertices00800040East),
    new PolygonCustom(vertices00800040South), new PolygonCustom(vertices00800040West), ..TinyPolygon], difference);
    public static readonly ArenaBoundsComplex ArenaENVC02000100 = new([new PolygonCustom(vertices02000100East), new PolygonCustom(vertices02000100North),
    new PolygonCustom(vertices02000100West), ..TinyPolygon], difference);
}
