namespace BossMod.Dawntrail.Savage.M11STheTyrant;

sealed class Cometite(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Circle = new(6f);
    private readonly List<AOEInstance> _aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo cast)
    {
        if (cast.Action.ID != (uint)AID.Cometite)
            return;

        // Resolve target at cast start
        var finish = WorldState.CurrentTime.AddSeconds(cast.NPCRemainingTime);

        _aoes.Add(new(
            Circle,
            cast.LocXZ,
            default,
            finish // THIS is the key
        ));
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
            return default;

        // Remove expired AOEs
        var now = WorldState.CurrentTime;
        for (var i = _aoes.Count - 1; i >= 0; --i)
            if (_aoes[i].Activation <= now)
                _aoes.RemoveAt(i);

        if (_aoes.Count == 0)
            return default;

        // The earliest activation among the displayed AOEs
        var firstActivation = _aoes[0].Activation;

        // Build colored output (Explosion pattern)
        var result = new AOEInstance[_aoes.Count];
        for (var i = 0; i < _aoes.Count; ++i)
        {
            var aoe = _aoes[i];
            var dt = (aoe.Activation - firstActivation).TotalSeconds;

            result[i] =
                dt < 1.0
                    ? aoe with { Color = Colors.Danger }
                    : aoe with { Risky = false };
        }

        return result;
    }
}
sealed class Comet(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.CometIcon, (uint)AID.Comet, 6f, 5d);
sealed class CrushingComet(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.CrushingComet, 6f);
sealed class EyeOfTheHurricane(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.EyeOfTheHurricane, 6f, 2, 2);
sealed class Explosion(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Explosion1, (uint)AID.Explosion2], new AOEShapeRect(60f, 5f));
sealed class FireAndFury(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.FireAndFuryFront, (uint)AID.FireAndFuryBack], new AOEShapeCone(60f, 45.Degrees()));
sealed class GreatWallOfFire(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.GreatWallOfFire1, (uint)AID.GreatWallOfFire2, (uint)AID.GreatWallOfFireExplosion], new AOEShapeRect(60f, 3f));
sealed class FearsomeFireball(BossModule module) : Components.LineStack(module, (uint)IconID.FearsomeFireballIcon, (uint)AID.FearsomeFireball1, 5d, 60f, 4f, 4, 6, 1, true);
sealed class OneAndOnly(BossModule module) : Components.RaidwideCast(module, (uint)AID.OneAndOnly, "Raidwide");
sealed class OrbitalOmen : Components.SimpleAOEs
{
    public OrbitalOmen(BossModule module) : base(module, (uint)AID.OrbitalOmen_Lines, new AOEShapeRect(60f, 5f), 4)
    {
        MaxDangerColor = 2;
    }
}
sealed class MaelstromVoidZones(BossModule module) : Components.GenericAOEs(module)
{
    // Adjust radius to match the real danger zone
    private static readonly AOEShapeCircle VoidZone = new(5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var maelstroms = WorldState.Actors.Where(a => (OID)a.OID == OID.Maelstrom);
        if (!maelstroms.Any())
            return default;

        var aoes = new List<AOEInstance>();
        foreach (var m in maelstroms)
        {
            aoes.Add(new AOEInstance(
                VoidZone,
                m.Position,
                default,
                default,
                Colors.Danger
            ));
        }

        return aoes.ToArray();
    }
}
sealed class MaelstromGustCones(BossModule module) : Components.GenericAOEs(module)
{
    // Adjust angle/range if needed
    private static readonly AOEShapeCone GustCone =
        new(radius: 60f, halfAngle: 15.Degrees());
    private bool _resolved;
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.PowerfulGust)
            _resolved = true;
    }
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_resolved)
            return default;
        // Find all active Maelstroms
        var maelstroms = WorldState.Actors
            .Where(a => (OID)a.OID == OID.Maelstrom)
            .ToList();

        // Only activate once all 4 are present
        if (maelstroms.Count < 4)
            return default;

        var players = WorldState.Party.WithoutSlot()
            .Where(p => !p.IsDead)
            .ToList();

        if (players.Count == 0)
            return default;

        var aoes = new List<AOEInstance>();

        foreach (var m in maelstroms)
        {
            // Find the two closest players to this Maelstrom
            var targets = players
                .OrderBy(p => (p.Position - m.Position).LengthSq())
                .Take(2);

            foreach (var t in targets)
            {
                var dir = Angle.FromDirection(t.Position - m.Position);
                aoes.Add(new AOEInstance(GustCone, m.Position, dir));
            }
        }

        return aoes.ToArray();
    }
}