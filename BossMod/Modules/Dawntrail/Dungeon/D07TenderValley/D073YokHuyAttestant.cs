namespace BossMod.Dawntrail.Dungeon.D07TenderValley.D073YokHuyAttestant;

public enum OID : uint
{
    Boss = 0x4253, // R4.0
    BlackenedStatue = 0x4254, // R0.5
    // trash that can be pulled into boss room
    YokHuyOrb = 0x424F, // R1.5
    YokHuyAltar = 0x448D, // R1.7
    YokHuyAltar2 = 0x4251 // R1.7
}

public enum AID : uint
{
    AutoAttack = 872, // YokHuyAltar2/YokHuyOrb/YokHuyAltar->player, no cast, single-target

    TectonicShift = 39221, // YokHuyAltar->self, 3.0s cast, range 8 circle
    AncientWrathVisual = 38538, // Boss->self, 6.0s cast, single-target
    AncientWrathLong = 39825, // BlackenedStatue->self, no cast, range 35 width 8 rect
    AncientWrathShort = 39823, // BlackenedStatue->self, no cast, range 12 width 8 rect
    AncientWrathMedium = 39824, // BlackenedStatue->self, no cast, range 22 width 8 rect
    BoulderToss = 38540, // Boss->player, 1.0s cast, single-target
    SunToss = 38539 // Boss->location, 3.0s cast, range 6 circle
}

public enum TetherID : uint
{
    StatueActivate = 37 // 28E8->Boss
}

class TectonicShift(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TectonicShift), 8f);
class BoulderToss(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.BoulderToss));
class SunToss(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SunToss), 6);

class AncientWrath(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(6);
    private static readonly AOEShapeRect rectShort = new(12f, 4f);
    private static readonly AOEShapeRect rectMedium = new(22f, 4f);
    private static readonly AOEShapeRect rectLong = new(35f, 4f);

    private static readonly (WPos Position, AOEShapeRect Shape)[] AOEMap =
        [(new(-112.5f, -486.5f), rectMedium), (new(-147.5f, -471.5f), rectMedium),
        (new(-147.5f, -486.5f), rectShort), (new(-112.5f, -471.5f), rectShort)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.StatueActivate)
        {
            var aoeShape = GetAOEShape(source.Position) ?? rectLong;
            _aoes.Add(new(aoeShape, source.Position, source.Rotation, WorldState.FutureTime(8.1d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AncientWrathShort or (uint)AID.AncientWrathMedium or (uint)AID.AncientWrathLong)
            _aoes.Clear();
    }

    private static AOEShapeRect? GetAOEShape(WPos position)
    {
        foreach (var (pos, shape) in AOEMap)
            if (position.AlmostEqual(pos, 1))
                return shape;
        return null;
    }
}

class D073YokHuyAttestantStates : StateMachineBuilder
{
    public D073YokHuyAttestantStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AncientWrath>()
            .ActivateOnEnter<BoulderToss>()
            .ActivateOnEnter<SunToss>()
            .ActivateOnEnter<TectonicShift>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 834, NameID = 12801)]
public class D073YokHuyAttestant(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Rectangle(new(-130f, -475f), 17.5f, 22f)], [new Rectangle(new(-135.05f, -486.25f), 1.35f, 3.5f), new Rectangle(new(-124.4f, -471.4f), 1.3f, 3.75f)]);
    private static readonly uint[] Trash = [(uint)OID.YokHuyAltar, (uint)OID.YokHuyAltar2, (uint)OID.YokHuyOrb];
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(Trash));
    }
}
