namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE15CrawlingDeath;

public enum OID : uint
{
    Boss = 0x46C4, // R3.6
    Deathwall = 0x483F, // R0.5
    PhantomClaw = 0x46C5, // R2.925
    Clawmarks1 = 0x46C6, // R1.0-1.7
    Clawmarks2 = 0x46C7, // R1.0
    Clawmarks3 = 0x46C8, // R1.0
    ClawsOnFloor = 0x1EBCF0, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target
    Deathwall = 41308, // Deathwall->self, no cast, ???

    DirtyNails = 41332, // Boss->player, 5.0s cast, single-target, tankbuster
    Visual = 41312, // PhantomClaw->self, no cast, single-target
    Clawmarks = 41309, // Boss->self, 5.0s cast, single-target
    ThreefoldMarks = 41310, // Boss->self, 5.0s cast, single-target
    ManifoldMarks = 41311, // Boss->self, 5.0s cast, single-target
    LethalNails1 = 41315, // Clawmarks2/Clawmarks3/Clawmarks1->self, 2.0s cast, range 60 width 7 rect
    LethalNails2 = 41316, // Clawmarks1/Clawmarks2->self, 4.0s cast, range 60 width 7 rect
    LethalNails3 = 41317, // Clawmarks1->self, 6.0s cast, range 60 width 7 rect
    HorizontalCrosshatch1 = 41324, // Boss->self, 5.0s cast, single-target
    HorizontalCrosshatch2 = 41331, // Boss->self, 7.5s cast, single-target
    VerticalCrosshatch1 = 41323, // Boss->self, 5.0s cast, single-target
    VerticalCrosshatch2 = 41330, // Boss->self, 7.5s cast, single-target
    RakingScratch = 41325, // Helper->self, no cast, range 50 90-degree cone
    SkulkingOrders1 = 41326, // Boss->self, 7.0s cast, single-target
    SkulkingOrders2 = 41329, // Boss->self, 7.0s cast, single-target
    ClawingShadowVisual = 41328, // Helper->self, 1.0s cast, range 50 90-degree cone
    ClawingShadow = 41327, // PhantomClaw->self, no cast, range 50 90-degree cone
    TheGripOfPoisonVisual = 41333, // Boss->self, 4.0s cast, single-target
    TheGripOfPoison = 41334 // Helper->self, no cast, ???
}

sealed class TheGripOfPoison(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.TheGripOfPoisonVisual, (uint)AID.TheGripOfPoison, 0.9f, "Raidwide + bleed");
sealed class DirtyNails(BossModule module) : Components.SingleTargetCast(module, (uint)AID.DirtyNails);

sealed class Clawmarks(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(16);
    private static readonly AOEShapeRect rect = new(60f, 3.5f);
    public enum MechanicType
    {
        Clawmarks = 0,
        ThreefoldMarks = 1,
        ManifoldMarks = 2
    }
    private MechanicType mechanic;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var deadline = aoes[0].Activation.AddSeconds(1d);

        var index = 0;
        while (index < count && aoes[index].Activation < deadline)
            ++index;

        return aoes[..index];
    }

    public override void OnActorCreated(Actor actor)
    {
        var activation = (actor.OID, mechanic) switch
        {
            ((uint)OID.Clawmarks1, MechanicType.Clawmarks) => 9.0d,
            ((uint)OID.Clawmarks2, MechanicType.Clawmarks) => 6.9d,
            ((uint)OID.Clawmarks3, MechanicType.ThreefoldMarks) => 6.9d,
            ((uint)OID.Clawmarks2, MechanicType.ThreefoldMarks) => 8.7d,
            ((uint)OID.Clawmarks1, MechanicType.ThreefoldMarks) => 10.8d,
            ((uint)OID.Clawmarks1, MechanicType.ManifoldMarks) => 6.7d,
            ((uint)OID.Clawmarks2, MechanicType.ManifoldMarks) => 8.8d,
            _ => default
        };
        if (activation != default)
        {
            _aoes.Add(new(rect, WPos.ClampToGrid(actor.Position), actor.Rotation, WorldState.FutureTime(activation), ActorID: actor.InstanceID));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        mechanic = spell.Action.ID switch
        {
            (uint)AID.Clawmarks => MechanicType.Clawmarks,
            (uint)AID.ThreefoldMarks => MechanicType.ThreefoldMarks,
            (uint)AID.ManifoldMarks => MechanicType.ManifoldMarks,
            _ => mechanic
        };
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.LethalNails1 or (uint)AID.LethalNails2 or (uint)AID.LethalNails3)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

sealed class ClawingShadow(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    public static readonly AOEShapeCone Cone = new(50f, 45f.Degrees());
    private readonly List<Actor> claws = new(4);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count > 1 ? CollectionsMarshal.AsSpan(_aoes)[..2] : [];

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00010002u && actor.OID == (uint)OID.ClawsOnFloor)
        {
            claws.Add(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SkulkingOrders1 or (uint)AID.SkulkingOrders2)
        {
            var count = claws.Count;
            for (var i = 0; i < count; ++i)
            {
                var claw = claws[i];
                _aoes.Add(new(Cone, WPos.ClampToGrid(claw.Position), claw.Rotation, WorldState.FutureTime(i < 2 ? 10.1d : 15.6d)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.ClawingShadow)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class Crosshatch(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (!aoes[0].Risky)
        {
            for (var i = 0; i < 2; ++i)
            {
                aoes[i].Risky = true;
            }
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (spell.Action.ID is (uint)AID.HorizontalCrosshatch1 or (uint)AID.HorizontalCrosshatch2 or (uint)AID.VerticalCrosshatch1 or (uint)AID.VerticalCrosshatch2)
        {
            Angle[] angles = [-90.004f.Degrees(), 89.999f.Degrees(), -0.003f.Degrees(), 180f.Degrees()];
            if (id is (uint)AID.VerticalCrosshatch1 or (uint)AID.VerticalCrosshatch2)
            {
                angles.Reverse();
            }
            for (var i = 0; i < 4; ++i)
            {
                _aoes.Add(new(ClawingShadow.Cone, spell.LocXZ, angles[i], Module.CastFinishAt(spell, i < 2 ? default : 2f), i < 2 ? Colors.Danger : default, i < 2));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.RakingScratch)
        {
            _aoes.RemoveAt(0);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _aoes.Count;
        if (count == 0)
            return;
        base.AddAIHints(slot, actor, assignment, hints);
        if (count > 2)
        {
            var aoe = _aoes[0];
            // stay close to the middle if there is more than one aoe left
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(aoe.Origin, 5f), aoe.Activation);
        }
    }
}

sealed class CE15CrawlingDeathStates : StateMachineBuilder
{
    public CE15CrawlingDeathStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Clawmarks>()
            .ActivateOnEnter<ClawingShadow>()
            .ActivateOnEnter<Crosshatch>()
            .ActivateOnEnter<TheGripOfPoison>()
            .ActivateOnEnter<DirtyNails>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 1018, NameID = 36)]
public sealed class CE15CrawlingDeath(WorldState ws, Actor primary) : BossModule(ws, primary, new(681, 534f), new ArenaBoundsSquare(21f));
