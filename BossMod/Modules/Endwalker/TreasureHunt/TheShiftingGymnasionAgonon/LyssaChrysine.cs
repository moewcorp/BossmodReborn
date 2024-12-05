namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.LyssaChrysine;

public enum OID : uint
{
    Boss = 0x3D43, //R=5.0
    IcePillar = 0x3D44, //R=2.0
    GymnasiouLyssa = 0x3D4E, //R=3.75, bonus loot adds
    GymnasiouLampas = 0x3D4D, //R=2.001, bonus loot adds
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/GymnasiouLyssa->player, no cast, single-target

    Icicall = 32307, // Boss->self, 2.5s cast, single-target, spawns ice pillars
    IcePillar = 32315, // IcePillar->self, 3.0s cast, range 6 circle
    SkullDasher = 32306, // Boss->player, 5.0s cast, single-target
    PillarPierce = 32316, // IcePillar->self, 3.0s cast, range 80 width 4 rect
    HeavySmash = 32314, // Boss->players, 5.0s cast, range 6 circle, stack
    Howl = 32296, // Boss->self, 2.5s cast, single-target, calls adds

    FrigidNeedleVisual = 32310, // Boss->self, 3.5s cast, single-target --> combo start FrigidNeedle --> CircleofIce (out-->in)
    FrigidNeedle = 32311, // Helper->self, 4.0s cast, range 10 circle
    CircleOfIceVisual = 32312, // Boss->self, 3.5s cast, single-target --> combo start CircleofIce --> FrigidNeedle (in-->out)
    CircleOfIce = 32313, // Helper->self, 4.0s cast, range 10-20 donut

    FrigidStoneVisual = 32308, // Boss->self, 2.5s cast, single-target
    FrigidStone = 32309, // Helper->location, 3.0s cast, range 5 circle

    HeavySmash2 = 32317, // GymnasiouLyssa->location, 3.0s cast, range 6 circle
    Telega = 9630 // GymnasiouLyssa/Lampas->self, no cast, single-target, bonus add disappear
}

class HeavySmash2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HeavySmash2), 6);
class FrigidStone(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.FrigidStone), 5);

class FrigidNeedle(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10), new AOEShapeDonut(10, 20)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FrigidNeedleVisual)
            AddSequence(Module.Center, Module.CastFinishAt(spell, 0.5f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Sequences.Count > 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.FrigidNeedle => 0,
                AID.CircleOfIce => 1,
                _ => -1
            };
            AdvanceSequence(order, Arena.Center, WorldState.FutureTime(2));
        }
    }
}

class CircleOfIce(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeDonut(10, 20), new AOEShapeCircle(10)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CircleOfIceVisual)
            AddSequence(Module.Center, Module.CastFinishAt(spell, 0.5f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Sequences.Count > 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.CircleOfIce => 0,
                AID.FrigidNeedle => 1,
                _ => -1
            };
            AdvanceSequence(order, Arena.Center, WorldState.FutureTime(2));
        }
    }
}

class PillarPierce(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PillarPierce), new AOEShapeRect(80, 2));
class SkullDasher(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.SkullDasher));
class HeavySmash(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.HeavySmash), 6, 8, 8);

class IcePillar(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.IcePillar)
            _aoes.Add(new(circle, actor.Position, default, WorldState.FutureTime(3.7f)));
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.IcePillar)
            _aoes.Clear();
    }
}

class Howl(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.Howl), "Calls adds");

class LyssaChrysineStates : StateMachineBuilder
{
    public LyssaChrysineStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IcePillar>()
            .ActivateOnEnter<HeavySmash>()
            .ActivateOnEnter<SkullDasher>()
            .ActivateOnEnter<Howl>()
            .ActivateOnEnter<FrigidNeedle>()
            .ActivateOnEnter<CircleOfIce>()
            .ActivateOnEnter<FrigidStone>()
            .ActivateOnEnter<HeavySmash2>()
            .ActivateOnEnter<PillarPierce>()
            .Raw.Update = () => module.Enemies(LyssaChrysine.All).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12024)]
public class LyssaChrysine(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GymnasiouLampas, (uint)OID.GymnasiouLyssa];
    public static readonly uint[] All = [(uint)OID.Boss, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(bonusAdds), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.GymnasiouLampas => 2,
                OID.GymnasiouLyssa => 1,
                _ => 0
            };
        }
    }
}
