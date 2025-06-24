﻿namespace BossMod.Dawntrail.Ultimate.FRU;

class P2AbsoluteZero(BossModule module) : Components.CastCounter(module, (uint)AID.AbsoluteZeroAOE);

class P2SwellingFrost(BossModule module) : Components.GenericKnockback(module, (uint)AID.SwellingFrost, true)
{
    private readonly DateTime _activation = module.WorldState.FutureTime(3.2d);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        return new Knockback[1] { new(Arena.Center, 10f, _activation) };
    }
}

class P2SinboundBlizzard(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SinboundBlizzardAOE, new AOEShapeCone(50f, 10f.Degrees()));

class P2HiemalStorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HiemalStormAOE, 7f)
{
    private bool _slowDodges;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // storms are cast every 3s, ray voidzones appear every 2s; to place voidzones more tightly, we pretend radius is smaller during first half of cast
        // there's no point doing it before first voidzone appears, however
        var deadline = _slowDodges ? WorldState.FutureTime(1.5d) : DateTime.MaxValue;
        foreach (var c in Casters)
        {
            var activation = c.Activation;
            hints.AddForbiddenZone(ShapeDistance.Circle(c.Origin, activation > deadline ? 4f : 7f), activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.HiemalRay)
            _slowDodges = true;
    }
}

class P2HiemalRay(BossModule module) : Components.VoidzoneAtCastTarget(module, 4f, (uint)AID.HiemalRay, GetVoidzones, 0.7f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.HiemalRayVoidzone);
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

// TODO: show hint if ice veil is clipped
class P2Intermission(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private readonly P2SinboundBlizzard? _cones = module.FindComponent<P2SinboundBlizzard>();
    private readonly IReadOnlyList<Actor> _crystalsOfLight = module.Enemies(OID.CrystalOfLight);
    private readonly IReadOnlyList<Actor> _crystalsOfDarkness = module.Enemies(OID.CrystalOfDarkness);
    private readonly IReadOnlyList<Actor> _iceVeil = module.Enemies(OID.IceVeil);
    private bool _iceVeilInvincible = true;

    public bool CrystalsActive => CrystalsOfLight.Any();

    public override void Update()
    {
        IgnoreOtherBaits = true;
        CurrentBaits.Clear();
        if (_cones == null)
            return;
        foreach (var c in _crystalsOfDarkness)
        {
            var baiter = Raid.WithoutSlot(false, true, true).Closest(c.Position);
            if (baiter != null)
                CurrentBaits.Add(new(c, baiter, _cones.Shape));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // enemy priorities
        var clockSpot = _config.P2IntermissionClockSpots[assignment];
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.CrystalOfLight => CrystalPriority(e.Actor, clockSpot),
                (uint)OID.CrystalOfDarkness => AIHints.Enemy.PriorityPointless,
                (uint)OID.IceVeil => _iceVeilInvincible ? AIHints.Enemy.PriorityInvincible : 1,
                _ => 0
            };
        }

        // don't stand inside light crystals, to avoid bad puddle baits
        foreach (var c in CrystalsOfLight)
            hints.AddForbiddenZone(ShapeDistance.Circle(c.Position, 4f), WorldState.FutureTime(30d));

        // mechanic resolution
        if (clockSpot < 0)
        {
            // no assignment, oh well...
        }
        else if ((clockSpot & 1) == 0)
        {
            // cardinals - bait puddles accurately
            var assignedDir = (180f - 45f * clockSpot).Degrees();
            var assignedPosition = Arena.Center + 15f * assignedDir.ToDirection(); // crystal is at R=15
            var assignedCrystal = CrystalsOfLight.FirstOrDefault(c => c.Position.AlmostEqual(assignedPosition, 2f));
            if (assignedCrystal != null)
            {
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(assignedPosition, 5f), WorldState.FutureTime(60d));
                hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 17f), DateTime.MaxValue); // prefer to stay near border, unless everything else is covered with aoes
            }
            else
            {
                // go to the ice veil
                // TODO: consider helping other melees with their crystals? a bit dangerous, can misbait
                // TODO: consider helping nearby ranged to bait their cones?
                hints.AddForbiddenZone(ShapeDistance.InvertedCone(Arena.Center, 7f, assignedDir, 10f.Degrees()), DateTime.MaxValue);
            }
        }
        else
        {
            // intercardinals - bait cones
            if (_cones?.Casters.Count == 0)
            {
                var assignedPosition = Arena.Center + 9f * (180f - 45f * clockSpot).Degrees().ToDirection(); // crystal is at R=8
                var assignedCrystal = CrystalsOfDarkness.FirstOrDefault(c => c.Position.AlmostEqual(assignedPosition, 2f));
                if (assignedCrystal != null)
                    hints.AddForbiddenZone(ShapeDistance.PrecisePosition(assignedPosition, new WDir(default, 1f), Arena.Bounds.MapResolution, actor.Position, 0.1f));
            }
            // else: just dodge cones etc...
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(CrystalsOfLight);
        Arena.Actors(CrystalsOfDarkness, Colors.Object);
        Arena.Actor(IceVeil, _iceVeilInvincible ? Colors.Object : 0);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Invincibility)
            _iceVeilInvincible = false;
    }

    private IEnumerable<Actor> ActiveActors(IReadOnlyList<Actor> raw) => raw.Where(a => a.IsTargetable && !a.IsDead);
    private IEnumerable<Actor> CrystalsOfLight => ActiveActors(_crystalsOfLight);
    private IEnumerable<Actor> CrystalsOfDarkness => ActiveActors(_crystalsOfDarkness);
    private Actor? IceVeil => ActiveActors(_iceVeil).FirstOrDefault();

    private int CrystalPriority(Actor crystal, int clockSpot)
    {
        var offset = crystal.Position - Arena.Center;
        var priority = clockSpot switch
        {
            0 => offset.Z < -10f,
            2 => offset.X > +10f,
            4 => offset.Z > +10f,
            6 => offset.X < -10f,
            _ => false
        };
        return priority ? 2 : 1;
    }
}
