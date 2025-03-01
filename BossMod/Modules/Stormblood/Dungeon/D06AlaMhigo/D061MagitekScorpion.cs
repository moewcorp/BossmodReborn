namespace BossMod.Stormblood.Dungeon.D06AlaMhigo.D061MagitekScorpion;

public enum OID : uint
{
    Boss = 0x1BA4, // R6.0
    Target = 0x1BA5,
    FireVoidzone = 0x1EA66D,
    Helper = 0x18D6
}

public enum AID : uint
{
    AutoAttack = 9303, // Boss->player, no cast, single-target

    ElectromagneticField = 8269, // Boss->self, 3.0s cast, range 40 circle
    TargetSearch = 8262, // Boss->self, 3.0s cast, single-target
    LockOn = 8263, // Helper->self, no cast, range 5 circle
    TailLaserVisual = 8264, // Boss->self, 2.0s cast, single-target
    TailLaserFrontFirst = 8265, // Helper->self, 3.0s cast, range 20+R width 10 rect
    TailLaserBackFirst = 8266, // Helper->self, 3.0s cast, range 20+R width 10 rect
    TailLaserFrontRest = 8267, // Helper->self, no cast, range 20+R width 10 rect
    TailLaserBackRest = 8268, // Helper->self, no cast, range 20+R width 10 rect
}

class TailLaser(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(20.5f, 5, 20.5f);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TailLaserVisual)
            _aoe = new(rect, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 1));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TailLaserBackFirst or AID.TailLaserFrontFirst or AID.TailLaserFrontRest or AID.TailLaserBackRest)
        {
            if (++NumCasts == 14)
            {
                NumCasts = 0;
                _aoe = null;
            }
        }
    }
}

class TargetSearch(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var t in Module.Enemies(OID.Target).Where(x => x.EventState == 0))
            yield return new(circle, t.Position);
    }
}

class LockOn(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, ActionID.MakeSpell(AID.LockOn), m => m.Enemies(OID.FireVoidzone).Where(z => z.EventState != 7), 0.7f);

class ElectromagneticField(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ElectromagneticField));

class D061MagitekScorpionStates : StateMachineBuilder
{
    public D061MagitekScorpionStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TailLaser>()
            .ActivateOnEnter<ElectromagneticField>()
            .ActivateOnEnter<LockOn>()
            .ActivateOnEnter<TargetSearch>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 247, NameID = 6037)]
public class D061MagitekScorpion(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Circle(new(-191, 72), 19.75f)], [new Rectangle(new(-210, 72), 1f, 20f), new Rectangle(new(-172.2f, 72), 1f, 20f)]);
}
