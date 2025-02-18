namespace BossMod.Dawntrail.Dungeon.D05Origenics.D051Herpekaris;

public enum OID : uint
{
    Boss = 0x4185, // R8.4
    PoisonVoidzone = 0x1E9E3C, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 36520, // Boss->location, no cast, single-target

    StridentShriek = 36519, // Boss->self, 5.0s cast, range 60 circle  raidwide

    Vasoconstrictor = 36459, // Boss->self, 3.0+1.2s cast, single-target, spawns poison
    PoisonHeartVoidzone = 36460, // Helper->location, 4.2s cast, range 2 circle
    PoisonHeartSpread = 37921, // Helper->player, 8.0s cast, range 5 circle

    PodBurst1 = 38518, // Helper->location, 5.0s cast, range 6 circle
    PodBurst2 = 38519, // Helper->location, 4.0s cast, range 6 circle

    Venomspill1 = 37451, // Boss->self, 5.0s cast, single-target
    Venomspill2 = 36452, // Boss->self, 5.0s cast, single-target
    Venomspill3 = 36453, // Boss->self, 4.0s cast, single-target
    Venomspill4 = 36454, // Boss->self, 4.0s cast, single-target

    WrithingRiotVisual = 36463, // Boss->self, 9.0s cast, single-target
    RightSweepTelegraph = 36465, // Helper->self, 2.0s cast, range 25 210-degree cone
    LeftSweepTelegraph = 36466, // Helper->self, 2.0s cast, range 25 210-degree cone
    RearSweepTelegraph = 36467, // Helper->self, 2.0s cast, range 25 90-degree cone
    RightSweep = 36469, // Boss->self, no cast, range 25 210-degree cone
    LeftSweep = 36470, // Boss->self, no cast, range 25 210-degree cone
    RearSweep = 36471, // Boss->self, no cast, range 25 90-degree cone

    CollectiveAgonyMarker = 36474, // Helper->player, no cast, single-target
    CollectiveAgony = 36473, // Boss->self/players, 5.5s cast, range 50 width 8 rect
    ConvulsiveCrush = 36518, // Boss->player, 5.0s cast, single-target, tb
}

class CollectiveAgony(BossModule module) : Components.LineStack(module, ActionID.MakeSpell(AID.CollectiveAgonyMarker), ActionID.MakeSpell(AID.CollectiveAgony), 5.6f);
class StridentShriek(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.StridentShriek));
class ConvulsiveCrush(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.ConvulsiveCrush));
class PoisonHeartSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.PoisonHeartSpread), 5f);
class PoisonHeartVoidzone(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 2f, ActionID.MakeSpell(AID.PoisonHeartVoidzone), GetVoidzones, 0.9f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.PoisonVoidzone);
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
}

abstract class PodBurst(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 6f);
class PodBurst1(BossModule module) : PodBurst(module, AID.PodBurst1);
class PodBurst2(BossModule module) : PodBurst(module, AID.PodBurst2);

class WrithingRiot(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);
    private static readonly AOEShapeCone coneLeftRight = new(25f, 105f.Degrees());
    private static readonly AOEShapeCone coneRear = new(25f, 45f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = new AOEInstance[max];
        {
            for (var i = 0; i < max; ++i)
            {
                var aoe = _aoes[i];
                if (i == 0)
                    aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
                else
                    aoes[i] = aoe with { Risky = aoes[0].Shape != aoe.Shape };
            }
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape) => _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, WorldState.FutureTime(7.3d - _aoes.Count * 1d)));
        switch (spell.Action.ID)
        {
            case (uint)AID.RightSweepTelegraph:
            case (uint)AID.LeftSweepTelegraph:
                AddAOE(coneLeftRight);
                break;
            case (uint)AID.RearSweepTelegraph:
                AddAOE(coneRear);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.RearSweep:
                case (uint)AID.LeftSweep:
                case (uint)AID.RightSweep:
                    _aoes.RemoveAt(0);
                    break;
            }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        // stay close to the middle if there is next imminent aoe
        if (_aoes.Count > 1)
        {
            var aoe = _aoes[0];
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(aoe.Origin, 3f), aoe.Activation);
        }
    }
}

class D051HerpekarisStates : StateMachineBuilder
{
    public D051HerpekarisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CollectiveAgony>()
            .ActivateOnEnter<StridentShriek>()
            .ActivateOnEnter<PoisonHeartSpread>()
            .ActivateOnEnter<PoisonHeartVoidzone>()
            .ActivateOnEnter<PodBurst1>()
            .ActivateOnEnter<PodBurst2>()
            .ActivateOnEnter<WrithingRiot>()
            .ActivateOnEnter<ConvulsiveCrush>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 825, NameID = 12741, SortOrder = 1)]
public class D051Herpekaris(WorldState ws, Actor primary) : BossModule(ws, primary, new(-88f, -180f), new ArenaBoundsSquare(17.5f));
