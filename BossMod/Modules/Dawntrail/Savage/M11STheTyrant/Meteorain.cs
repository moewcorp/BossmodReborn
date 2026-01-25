namespace BossMod.Dawntrail.Savage.M11STheTyrant;

sealed class CometTethers(BossModule module) : Components.TankbusterTether(module, (uint)AID.ForegoneFatality, (uint)TetherID.CometTether, 0f);
sealed class CosmicKiss(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.CosmicKissIcon, (uint)AID.CosmicKiss, 4f, 5d);
sealed class Tether1(BossModule module) : Components.StretchTetherSingle(module, (uint)TetherID.ShortTether, 40f, new AOEShapeRect(60f, 5f));
sealed class Tether2(BossModule module) : Components.StretchTetherSingle(module, (uint)TetherID.LongTether, 40f, new AOEShapeRect(60f, 5f));
sealed class FireBreath(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(60f, 5f), (uint)IconID.CosmicKissIcon, (uint)AID.FireBreath1);
sealed class MajesticMeteor : Components.SimpleAOEs
{
    public MajesticMeteor(BossModule module) : base(module, (uint)AID.MajesticMeteorBaits, 3f, 24)
    {
        MaxDangerColor = 8;
    }
}
sealed class MeteorainComets(BossModule module) : BossComponent(module)
{
    private static readonly WPos InvalidPos = new(100f, 100f);
    private const float InvalidPosEpsilonSq = 0.01f;

    private bool _active = true;
    private readonly HashSet<ulong> _consumed = [];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_active)
            return;

        var comets = Module.Enemies((uint)OID.Comet);
        var count = comets.Count;

        for (int i = 0; i < count; ++i)
        {
            var c = comets[i];

            if (c.IsDead || _consumed.Contains(c.InstanceID))
                continue;

            if ((c.Position - InvalidPos).LengthSq() < InvalidPosEpsilonSq)
                continue;

            Arena.AddCircle(c.Position, c.HitboxRadius, Colors.Object);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Comet_Explosion)
        {
            _consumed.Add(caster.InstanceID);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TripleTyrannhilation2)
            _active = false;
    }
}

sealed class TripleTyrannhilation(BossModule module)
    : Components.CastLineOfSightAOE(module, (uint)AID.TripleTyrannhilation2, 60f)
{
    public override ReadOnlySpan<Actor> BlockerActors()
    {
        var comets = Module.Enemies((uint)OID.Comet);
        var count = comets.Count;
        if (count == 0)
            return default;

        var blockers = new List<Actor>();
        for (var i = 0; i < count; ++i)
        {
            var c = comets[i];
            if (!c.IsDead)
                blockers.Add(c);
        }

        return CollectionsMarshal.AsSpan(blockers);
    }
}

sealed class CosmicKissTowers(BossModule module) : Components.CastTowers(module, (uint)AID.CosmicKissTower, 4f, 1, 1, AIHints.PredictedDamageType.Tankbuster);
sealed class WeightyImpactTowers(BossModule module) : Components.CastTowers(module, (uint)AID.WeightyImpactTower, 4f, 2, 2, AIHints.PredictedDamageType.Shared);
sealed class ExplosionTowers(BossModule module) : Components.CastTowers(module, (uint)AID.ExplosionTower, radius: 4f, minSoakers: 2, maxSoakers: 2);
sealed class ExplosionTowerKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private readonly ExplosionTowers? _towers = module.FindComponent<ExplosionTowers>();
    private static readonly AOEShapeCircle Shape = new(4f);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_towers == null)
            return [];

        var towers = _towers.Towers;
        var count = towers.Count;
        if (count == 0)
            return [];

        Span<Knockback> sources = new Knockback[count];
        for (var i = 0; i < count; ++i)
        {
            var t = towers[i];
            sources[i] = new(
                t.Position,
                distance: 20f,
                activation: t.Activation,
                shape: Shape
            );
        }
        return sources;
    }
}
sealed class MeteorainPortals(BossModule module) : Components.GenericAOEs(module)
{
    // Meteorain line: length 60, width 10 (half-width = 5)
    private static readonly AOEShapeRect Shape = new(60f, 5f);

    // MapEffect index -> portal position
    private static readonly Dictionary<byte, WPos> PortalPositions = new()
    {
        { 0x16, new(79f, 75f) },
        { 0x17, new(89f, 75f) },
        { 0x18, new(111f, 75f) },
        { 0x19, new(121f, 75f) },
    };

    // Portals that have spawned this cycle
    private readonly Dictionary<byte, WPos> _activePortals = [];

    // Active AOEs once armed
    private readonly List<AOEInstance> _aoes = [];

    private bool _armed;

    // Capture portal spawns
    public override void OnMapEffect(byte index, uint state)
    {
        // Portal spawn event
        if (state == 0x00200010 && PortalPositions.TryGetValue(index, out var pos))
        {
            _activePortals[index] = pos;
        }
    }

    // Arm AOEs when Fire Breath begins
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FireBreath)
        {
            _armed = true;
            _aoes.Clear();

            var activation = Module.CastFinishAt(spell);

            foreach (var p in _activePortals.Values)
            {
                // If later needed, rotation can be adjusted here
                _aoes.Add(new AOEInstance(
                    Shape,
                    p,
                    default,
                    activation
                ));
            }
        }
    }

    // Clear once MajesticMeteorain resolves
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MajesticMeteorain)
        {
            _armed = false;
            _aoes.Clear();
            _activePortals.Clear();
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => _armed ? CollectionsMarshal.AsSpan(_aoes) : [];
}
