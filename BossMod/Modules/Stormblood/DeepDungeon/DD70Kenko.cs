namespace BossMod.Stormblood.DeepDungeon.DD70Kenko;

public enum OID : uint
{
    Boss = 0x23EB, // R6.000, x1
    Puddle = 0x1E9829
}

public enum AID : uint
{
    PredatorClaws = 12205, // Boss->self, 3.0s cast, range 9+R ?-degree cone
    Slabber = 12203, // Boss->location, 3.0s cast, range 8 circle
    Innerspace = 12207, // Boss->player, 3.0s cast, single-target
    Ululation = 12208, // Boss->self, 3.0s cast, range 80+R circle
    HoundOutOfHell = 12206, // Boss->player, 5.0s cast, width 14 rect charge
    Devour = 12204 // Boss->location, no cast, range 4+R ?-degree cone
}

class PredatorClaws(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.PredatorClaws), new AOEShapeCone(15f, 60f.Degrees()));
class Slabber(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Slabber), 8f);
class InnerspacePredict(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? Next;
    private static readonly AOEShapeCircle circle = new(3);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(Next);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Innerspace)
            Next = new(circle, WPos.ClampToGrid(WorldState.Actors.Find(spell.MainTargetID)!.Position), default, WorldState.FutureTime(1.6d));
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Puddle)
            Next = null;
    }
}

// TODO only apply invert logic to marked player, will probably need to rewrite component
class Innerspace(BossModule module) : Components.PersistentInvertibleVoidzone(module, 3f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Puddle);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HoundOutOfHell)
            InvertResolveAt = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Devour)
            InvertResolveAt = default;
    }
}
class Ululation(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Ululation));
class HoundOutOfHell(BossModule module) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(AID.HoundOutOfHell), 7f);

class KenkoStates : StateMachineBuilder
{
    public KenkoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PredatorClaws>()
            .ActivateOnEnter<Slabber>()
            .ActivateOnEnter<Innerspace>()
            .ActivateOnEnter<InnerspacePredict>()
            .ActivateOnEnter<Ululation>()
            .ActivateOnEnter<HoundOutOfHell>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 546, NameID = 7489)]
public class Kenko(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300f, -300f), new ArenaBoundsCircle(24f));

