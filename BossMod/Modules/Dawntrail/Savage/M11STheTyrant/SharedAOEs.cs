namespace BossMod.Dawntrail.Savage.M11STheTyrant;

sealed class Cometite : Components.SimpleAOEs
{
    public Cometite(BossModule module) : base(module, (uint)AID.Cometite, 6f, 24)
    {
        MaxDangerColor = 8;
    }
}
sealed class MajesticMeteorStorm : Components.SimpleAOEs
{
    public MajesticMeteorStorm(BossModule module) : base(module, (uint)AID.MajesticMeteorStorm, 6f, 36)
    {
        MaxDangerColor = 6;
    }
}
sealed class MammothMeteor(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MammothMeteor, new AOEShapeCircle(18f));
sealed class AtomicImpact(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.AtomicImpactIcon, (uint)AID.AtomicImpact, 5f, 6);
sealed class Comet(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.CometIcon, (uint)AID.Comet, 6f, 5d);
sealed class CrushingComet(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.CrushingComet, 6f);
sealed class EyeOfTheHurricane(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.EyeOfTheHurricane, 6f, 2, 2);
sealed class Explosion(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Explosion1, (uint)AID.Explosion2], new AOEShapeRect(60f, 5f));
sealed class FireAndFury(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.FireAndFuryFront, (uint)AID.FireAndFuryBack], new AOEShapeCone(60f, 45.Degrees()));
sealed class GreatWallOfFire(BossModule module) : Components.BaitAwayCast(module, (uint)AID.GreatWallOfFire, new AOEShapeRect(60f, 3f), false, true, true);
sealed class GreatWallOfFireExplosion(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.GreatWallOfFire1, (uint)AID.GreatWallOfFire2, (uint)AID.GreatWallOfFireExplosion], new AOEShapeRect(60f, 3f));
sealed class FearsomeFireball(BossModule module) : Components.LineStack(module, (uint)IconID.FearsomeFireballIcon, (uint)AID.FearsomeFireball1, 5d, 60f, 4f, 4, 6, 1, true);
sealed class OneAndOnly(BossModule module) : Components.RaidwideCast(module, (uint)AID.OneAndOnly, "Raidwide");
sealed class Flatliner(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Flatliner, new AOEShapeRect(30f, 5f, 30f));
sealed class FlatlinerKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Flatliner1, 15f, true, 1, new AOEShapeCircle(5f));
sealed class ArcadionAvalanche(BossModule module) :
    Components.SimpleAOEGroups(module, [(uint)AID.ArcadionAvalanche_Pick1, (uint)AID.ArcadionAvalanche_Pick2,
    (uint)AID.ArcadionAvalanche_Pick3, (uint)AID.ArcadionAvalanche_Pick4], new AOEShapeRect(40f, 20f));
sealed class ArcadionAvalancheSmash(BossModule module) :
    Components.SimpleAOEGroups(module, [(uint)AID.ArcadionAvalanche_Smash1, (uint)AID.ArcadionAvalanche_Smash2,
    (uint)AID.ArcadionAvalanche_Smash3, (uint)AID.ArcadionAvalanche_Smash4], new AOEShapeRect(40f, 20f), riskyWithSecondsLeft: 5);
sealed class OrbitalOmen : Components.SimpleAOEs
{
    public OrbitalOmen(BossModule module) : base(module, (uint)AID.OrbitalOmen_Lines, new AOEShapeRect(60f, 5f), 4)
    {
        MaxDangerColor = 2;
    }
}
sealed class MassiveMeteor(BossModule module) : Components.StackWithIcon(module, (uint)IconID.MassiveMeteorIcon, (uint)AID.MassiveMeteorHit, 6f, 5, 4, 4, 5);
sealed class MaelstromVoidZones(BossModule module) : Components.GenericAOEs(module)
{
    // Adjust radius to match the real danger zone
    private static readonly AOEShapeCircle VoidZone = new(4f);

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
    private static readonly AOEShapeCone GustCone = new(60f, 45.Degrees());
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
sealed class AtomicImpactVoidZones(BossModule module) :
    Components.Voidzone(module, 5f, module => module.Enemies((uint)OID.AtomicImpactVoidZones).Where(z => z.EventState != 7));