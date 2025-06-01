namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE110FlameOfDusk;

public enum OID : uint
{
    Boss = 0x46D1, // R4.0
    HinkypunkClone = 0x46D3, // R4.0
    AvianHusk = 0x46D2, // R4.0
    Deathwall = 0x1EBD55, // R0.5
    DeathwallHelper = 0x4863, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Deathwall = 41382, // DeathwallHelper->self, no cast, range 20n-25 donut

    LamplightVisual = 41381, // Boss->self, 5.0s cast, single-target, raidwide
    Lamplight = 41744, // Helper->self, 5.8s cast, ???
    DreadDive = 41380, // Boss->player, 5.0s cast, single-target, tankbuster

    BlowoutTelegraph = 41379, // Helper->self, 2.5s cast, range 50 circle
    ShadesNestTelegraph = 41374, // Helper->self, 2.5s cast, range 7-50 donut
    ShadesCrossingTelegraph = 41377, // Helper->self, 2.5s cast, range 50 width 15 cross
    BlowoutVisual = 41378, // Boss/HinkypunkClone->self, 2.0s cast, single-target
    Blowout = 41397, // Helper->self, 2.8s cast, ???, knockback 20, away from source
    ShadesNestVisual = 41373, // Boss/HinkypunkClone->self, 2.0s cast, single-target
    ShadesNestBossVisual = 41372, // Boss->self, 5.0+0,8s cast, single-target
    ShadesNestBoss = 42032, // Helper->self, 5.8s cast, range 7-50 donut
    ShadesNestHusk = 42033, // Helper->self, 2.8s cast, range 7-50 donut
    ShadesCrossingVisual = 41376, // Boss/HinkypunkClone->self, 2.0s cast, single-target
    ShadesCrossingBossVisual = 41375, // Boss->self, 5.0+0,8s cast, single-target
    ShadesCrossingBoss = 42034, // Helper->self, 5.8s cast, range 50 width 15 cross
    ShadesCrossingHusk = 42035, // Helper->self, 2.8s cast, range 50 width 15 cross

    Molt1 = 41368, // Boss->self, 13.0s cast, single-target
    Molt2 = 41369, // HinkypunkClone->self, 13.0s cast, single-target

    FlockOfSouls = 41370, // Boss->self, 5.0s cast, single-target
    BossDeath = 41396, // Boss->self, no cast, single-target
    ActivateHusk = 41371 // Helper->AvianHusk/Boss, no cast, single-target
}

sealed class DreadDive(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.DreadDive);
sealed class Lamplight(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.LamplightVisual, (uint)AID.Lamplight, 0.9f);

sealed class MoltAOEs(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private readonly List<(AOEShape?, bool knockback, DateTime activation)> pendingMechanics = new(4);
    private static readonly AOEShapeDonut donut = new(7f, 50f);
    private static readonly AOEShapeCross cross = new(50f, 7.5f);
    private bool first = true;
    private readonly MoltKB _kb = module.FindComponent<MoltKB>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        (AOEShape?, bool?) shape = spell.Action.ID switch
        {
            (uint)AID.ShadesCrossingTelegraph => (cross, false),
            (uint)AID.ShadesNestTelegraph => (donut, false),
            (uint)AID.BlowoutTelegraph => (null, true),
            (uint)AID.ShadesCrossingBoss => (cross, null),
            (uint)AID.ShadesNestBoss => (donut, null),
            _ => default
        };
        if (shape != default)
        {
            if (shape.Item2 != null)
            {
                var delay = (pendingMechanics.Count, first) switch
                {
                    (0, true) => 16.1f,
                    (1, true) => 18.4f,
                    (2, true) => 20.8f,
                    (3, true) => 23.2f,
                    (0, false) => 14.9f,
                    (1, false) => 15.3f,
                    (2, false) => 22.1f,
                    (3, false) => 22.5f,
                    _ => default
                };
                pendingMechanics.Add((shape.Item1, shape.Item2.Value, Module.CastFinishAt(spell, delay)));
                if (pendingMechanics.Count == 4)
                {
                    first = false;
                }
            }
            else
            {
                _aoes.Add(new(shape.Item1!, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ActivateHusk && pendingMechanics.Count != 0)
        {
            var mech = pendingMechanics[0];
            var pos = WorldState.Actors.Find(spell.MainTargetID)?.Position;
            if (pos is WPos position)
            {
                if (!mech.knockback)
                {
                    var husks = Module.Enemies((uint)OID.AvianHusk);
                    var count = husks.Count;

                    for (var i = 0; i < count; ++i)
                    {
                        var husk = husks[i];
                        if (husk.Position.AlmostEqual(position, 1f))
                        {
                            _aoes.Add(new(mech.Item1!, WPos.ClampToGrid(position), husk.Rotation, mech.activation));
                            if (_aoes.Count == 2)
                            {
                                _aoes.SortBy(aoe => aoe.Activation);
                            }
                            pendingMechanics.RemoveAt(0);
                            return;
                        }
                    }
                }
                else
                {
                    _kb.AddKnockback(WPos.ClampToGrid(position), mech.activation);
                    pendingMechanics.RemoveAt(0);
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.ShadesCrossingHusk or (uint)AID.ShadesNestHusk or (uint)AID.ShadesCrossingBoss or (uint)AID.ShadesNestBoss)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class MoltKB(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback? _kb;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => Utils.ZeroOrOne(ref _kb);

    public void AddKnockback(WPos position, DateTime activation) => _kb = new(position, 20f, activation);

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Blowout)
        {
            _kb = null;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb is Knockback kb)
        {
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var center = Arena.Center;
                var origin = kb.Origin;
                hints.AddForbiddenZone(p =>
                {
                    if ((p + 20f * (p - origin).Normalized()).InCircle(center, 20f))
                        return 1f;
                    return -1f;
                }, act);
            }
        }
    }
}

sealed class CE110FlameOfDuskStates : StateMachineBuilder
{
    public CE110FlameOfDuskStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MoltKB>()
            .ActivateOnEnter<MoltAOEs>()
            .ActivateOnEnter<DreadDive>()
            .ActivateOnEnter<Lamplight>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 1018, NameID = 47)]
public sealed class CE110FlameOfDusk(WorldState ws, Actor primary) : BossModule(ws, primary, WPos.ClampToGrid(new(-570f, -160f)), new ArenaBoundsCircle(20f));
