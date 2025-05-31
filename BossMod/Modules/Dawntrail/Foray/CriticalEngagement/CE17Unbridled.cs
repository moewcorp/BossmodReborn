namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE17Unbridled;

public enum OID : uint
{
    Boss = 0x4703, // R4.5
    Deathwall = 0x1EBD5C, // R0.5
    DeathwallHelper = 0x4871, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Torch = 37808, // DeathwallHelper->self, no cast, range 25-30 donut

    BoilOverVisual = 30788, // Boss->self, 5.0s cast, single-target, raidwide
    BoilOver = 30789, // Helper->self, 6.0s cast, ???
    ChanneledRageVisual = 30790, // Boss->self, 3.0s cast, single-target, raidwide
    ChanneledRage = 30791, // Helper->self, 3.5s cast, ???
    HeightenedRageVisual = 37809, // Boss->self, 3.0s cast, single-target, raidwide
    HeightenedRage = 37815, // Helper->self, 3.5s cast, ???

    HoppingMadVisual1 = 37306, // Boss->location, 7.0s cast, single-target
    HoppingMadVisual2 = 30792, // Boss->location, 8.0s cast, single-target
    HoppingMadVisual3 = 30870, // Boss->location, no cast, single-target
    HoppingMadVisual4 = 30871, // Boss->location, no cast, single-target
    HoppingMad1 = 37323, // Helper->location, 8.0s cast, range 8 circle
    BedrockUplift1 = 37807, // Helper->self, 1.5s cast, range 8-60 donut
    HoppingMad2 = 30872, // Helper->self, 9.0s cast, range 24 circle
    BedrockUplift2 = 37805, // Helper->self, 1.5s cast, range 24-60 donut
    HoppingMad3 = 30873, // Helper->self, 15.5s cast, range 16 circle
    BedrockUplift3 = 37806, // Helper->self, 1.5s cast, range 16-60 donut
    HoppingMad4 = 37041, // Helper->self, 22.5s cast, range 8 circle

    ScathingSweep = 42691, // Boss->self, 6.0s cast, range 60 width 60 rect
    WhiteHotRage = 37800, // Boss->location, 4.0s cast, range 6 circle
    HeatedOutburst = 37804 // Helper->self, 6.0s cast, range 13 circle
}

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(25f, 30f);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BoilOverVisual && Arena.Bounds != CE17Unbridled.DefaultArena)
            _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell, 1f));
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Deathwall)
        {
            Arena.Bounds = CE17Unbridled.DefaultArena;
            Arena.Center = WPos.ClampToGrid(Arena.Center);
            _aoe = null;
        }
    }
}

sealed class BoilOverChanneledHeightenedRage(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.BoilOver, (uint)AID.ChanneledRage, (uint)AID.HeightenedRage]);
sealed class WhiteHotRage(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WhiteHotRage, 6f);
sealed class HeatedOutburst(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module, [(uint)AID.HeatedOutburst], 13f);
sealed class ScathingSweep(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ScathingSweep, new AOEShapeRect(60f, 30f));

sealed class HoppingMad(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeCircle circleSmall = new(8f), circleMedium = new(16f), circleBig = new(24f);
    private static readonly AOEShapeDonut donutSmall = new(8f, 60f), donutMedium = new(16f, 60f), donutBig = new(24, 60f);
    private readonly HeatedOutburst _aoe = module.FindComponent<HeatedOutburst>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        if (_aoe.Casters.Count != 0)
            return CollectionsMarshal.AsSpan(_aoes)[..1];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref var aoe = ref aoes[0];
        if (count != 1)
            aoe.Color = Colors.Danger;
        aoe.Risky = true;
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (spell.Action.ID == (uint)AID.HoppingMad1)
        {
            AddAOE(circleSmall);
            AddAOE(donutSmall, 2.1f, false);
            return;
        }
        var shape = id switch
        {
            (uint)AID.HoppingMad2 => circleBig,
            (uint)AID.HoppingMad3 => circleMedium,
            (uint)AID.HoppingMad4 => circleSmall,
            _ => default
        };
        if (shape != null)
        {
            AddAOE(shape);
            if (shape == circleBig)
                AddAOE(donutBig, 2.1f, false);
            if (_aoes.Count == 4)
                _aoes.SortBy(aoe => aoe.Activation);
        }
        void AddAOE(AOEShape shape, float delay = default, bool risky = true) => _aoes.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell, delay), Risky: risky));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
        {
            var id = spell.Action.ID;
            switch (id)
            {
                case (uint)AID.HoppingMad1:
                case (uint)AID.HoppingMad2:
                case (uint)AID.HoppingMad3:
                case (uint)AID.HoppingMad4:
                case (uint)AID.BedrockUplift1:
                case (uint)AID.BedrockUplift2:
                case (uint)AID.BedrockUplift3:
                    _aoes.RemoveAt(0);
                    break;
            }
        }
        var shape = spell.Action.ID switch
        {
            (uint)AID.BedrockUplift2 => (circleMedium, donutMedium),
            (uint)AID.BedrockUplift3 => (circleSmall, donutSmall),
            _ => default
        };
        if (shape != default)
        {
            var count = _aoes.Count;
            for (var i = 0; i < count; ++i)
            {
                var aoe = _aoes[i];
                if (aoe.Shape == shape.Item1)
                {
                    _aoes.Insert(1, new(shape.Item2, aoe.Origin, default, aoe.Activation.AddSeconds(2.1d), Risky: false));
                    return;
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_aoes.Count == 0)
            return;
        var aoe = _aoes[0];
        if (aoe.Shape is AOEShapeCircle circle)
            hints.GoalZones.Add(hints.GoalSingleTarget(aoe.Origin, circle.Radius + 3, 5f)); // stay close to next circle to get into donut
    }
}

sealed class CE17UnbridledStates : StateMachineBuilder
{
    public CE17UnbridledStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<BoilOverChanneledHeightenedRage>()
            .ActivateOnEnter<WhiteHotRage>()
            .ActivateOnEnter<HeatedOutburst>()
            .ActivateOnEnter<HoppingMad>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 1018, NameID = 35)]
public sealed class CE17Unbridled(WorldState ws, Actor primary) : BossModule(ws, primary, startingArena.Center, startingArena)
{
    private static readonly ArenaBoundsComplex startingArena = new([new Polygon(new(620f, 800f), 29.5f, 32)]);
    public static readonly ArenaBoundsCircle DefaultArena = new(25f); // default arena got no extra collision, just a donut aoe
}
