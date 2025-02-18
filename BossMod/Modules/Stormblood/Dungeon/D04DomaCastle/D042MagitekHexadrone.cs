namespace BossMod.Stormblood.Dungeon.D04DomaCastle.D042MagitekHexadrone;

public enum OID : uint
{
    Boss = 0x1BD0, // R4.24
    HexadroneBit = 0x1BD2, // R0.9
    HexadroneBitHelper = 0x1BD3, // R0.5
    Helper = 0x1BD1
}

public enum AID : uint
{
    AutoAttack = 8501, // Boss->player, no cast, single-target

    CircleOfDeath = 8354, // Boss->self, 3.0s cast, range 4+R circle
    TwoTonzeMagitekMissile = 8355, // Boss->player, no cast, range 6 circle
    MagitekMissilesVisual = 8356, // Boss->self, 7.5s cast, single-target
    MagitekMissiles = 8357, // Helper->location, 8.0s cast, range 6 circle
    MagitekMissilesExplosion = 8358, // Helper->location, no cast, range 60 circle
    ChainMineVisual = 9287, // HexadroneBit->self, no cast, range 50+R width 3 rect
    ChainMine = 8359 // HexadroneBitHelper->player, no cast, single-target
}

public enum TetherID : uint
{
    HexadroneBits = 60 // HexadroneBit->HexadroneBit
}

public enum IconID : uint
{
    Stackmarker = 62 // player
}

class MagitekMissile(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.MagitekMissiles), 6);
class CircleOfDeath(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CircleOfDeath), 8.24f);
class TwoTonzeMagitekMissile(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stackmarker, ActionID.MakeSpell(AID.TwoTonzeMagitekMissile), 6, 5.1f, 4, 4);
class ChainMine(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(40, 2, 10);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;
    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.HexadroneBits)
            _aoes.Add(new(rect, source.Position, source.Rotation, WorldState.FutureTime(5.6f)));
    }
    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.HexadroneBits)
            _aoes.RemoveAll(x => x.Origin == source.Position);
    }
}

class D042MagitekHexadroneStates : StateMachineBuilder
{
    public D042MagitekHexadroneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CircleOfDeath>()
            .ActivateOnEnter<TwoTonzeMagitekMissile>()
            .ActivateOnEnter<ChainMine>()
            .ActivateOnEnter<MagitekMissile>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 241, NameID = 6203)]
public class D042MagitekHexadrone(WorldState ws, Actor primary) : BossModule(ws, primary, new(-240, 130.5f), new ArenaBoundsSquare(19.5f));
