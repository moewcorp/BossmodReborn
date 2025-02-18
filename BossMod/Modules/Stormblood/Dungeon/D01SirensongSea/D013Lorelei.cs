namespace BossMod.Stormblood.Dungeon.D01SirensongSea.D013Lorelei;

public enum OID : uint
{
    Boss = 0x1AFE, // R3.36
    ArenaVoidzone = 0x1EA2FF, // R2.0
    Voidzone = 0x1EA300 // R0.5
}

public enum AID : uint
{
    IllWill = 8035, // Boss->player, no cast, single-target
    VirginTears = 8041, // Boss->self, 3.0s cast, single-target
    MorbidAdvance = 8037, // Boss->self, 5.0s cast, range 80+R circle
    HeadButt = 8036, // Boss->player, no cast, single-target
    SomberMelody = 8039, // Boss->self, 4.0s cast, range 80+R circle
    MorbidRetreat = 8038, // Boss->self, 5.0s cast, range 80+R circle
    VoidWaterIII = 8040 // Boss->location, 3.5s cast, range 8 circle
}

class VirginTearsArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly ArenaBoundsCircle smallerBounds = new(15.75f);
    private static readonly AOEShapeDonut donut = new(15.75f, 22);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnActorEState(Actor actor, ushort state)
    {
        if (state == 0x001)
        {
            Arena.Bounds = D013Lorelei.DefaultArena;
            Arena.Center = D013Lorelei.DefaultArena.Center;
        }
        else if (state == 0x002)
        {
            _aoe = null;
            Arena.Bounds = smallerBounds;
            Arena.Center = D013Lorelei.ArenaCenter;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.VirginTears && Arena.Bounds == D013Lorelei.DefaultArena)
        {
            if (++NumCasts > 3)
                _aoe = new(donut, D013Lorelei.ArenaCenter, default, Module.CastFinishAt(spell, 0.7f));
        }
    }
}

class MorbidAdvance(BossModule module) : Components.ActionDrivenForcedMarch(module, ActionID.MakeSpell(AID.MorbidAdvance), 3, default, 1)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<Voidzone>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation) && z.Risky) ?? false) || !Module.InBounds(pos);
}

class MorbidRetreat(BossModule module) : Components.ActionDrivenForcedMarch(module, ActionID.MakeSpell(AID.MorbidRetreat), 3, 180.Degrees(), 1)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<Voidzone>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation) && z.Risky) ?? false) || !Module.InBounds(pos);
}

class SomberMelody(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SomberMelody));
class VoidWaterIII(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.VoidWaterIII), 8);
class Voidzone(BossModule module) : Components.PersistentVoidzone(module, 7, m => m.Enemies(OID.Voidzone).Where(z => z.EventState != 7));

class D013LoreleiStates : StateMachineBuilder
{
    public D013LoreleiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VirginTearsArenaChange>()
            .ActivateOnEnter<Voidzone>()
            .ActivateOnEnter<MorbidAdvance>()
            .ActivateOnEnter<MorbidRetreat>()
            .ActivateOnEnter<SomberMelody>()
            .ActivateOnEnter<VoidWaterIII>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus), erdelf", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 238, NameID = 6074)]
public class D013Lorelei(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultArena.Center, DefaultArena)
{
    public static readonly WPos ArenaCenter = new(-44.5f, 465);
    public static readonly ArenaBoundsComplex DefaultArena = new([new Circle(ArenaCenter, 21.6f)], [new Rectangle(new(-44.5f, 443), 20, 1)]);
}
