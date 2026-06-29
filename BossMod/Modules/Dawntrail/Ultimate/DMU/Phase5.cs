namespace BossMod.Dawntrail.Ultimate.DMU;

class UltimaRepeater(BossModule module) : Components.RaidwideCast(module, (uint)AID.UltimaRepeaterCast) {
    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.UltimaRepeaterRaidwide) {
            NumCasts++;
        }
    }
}

class FellForces(BossModule module) : Components.GenericBaitStack(module) {
    public bool active = true;
    private bool setup = false;

    public override void Update() {
        if (active == false || setup == true) {
            return;
        }

        setupBaits();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.FellForces || spell.Action.ID == (uint)AID.FellForces1 ||
            spell.Action.ID == (uint)AID.FellForces2) {
            NumCasts++;

            if (NumCasts == 9) {
                CurrentBaits.Clear();
            }
        }
    }

    private void setupBaits() {
        var boss = ((DMU)Module).KefkaP5();
        if (boss == null) {
            return;
        }

        var party = Raid.WithSlot(true, true, true);
        BitMask allowedTanks = default;
        BitMask allowedHealers = default;
        BitMask allowedDDs = default;

        for (int i = 0; i < party.Length; i++) {
            ref var p = ref party[i];

            if (p.Item2.Role == Role.Tank) {
                allowedTanks.Set(p.Item1);
            }

            if (p.Item2.Role == Role.Healer) {
                allowedHealers.Set(p.Item1);
            }

            if (p.Item2.Role == Role.Melee || p.Item2.Role == Role.Ranged) {
                allowedDDs.Set(p.Item1);
            }
        }

        var addedTank = false;
        var addedHealer = false;
        var addedDD = false;

        for (int i = 0; i < party.Length; i++) {
            ref var player = ref party[i];
            var p = player.Item2;

            if (p.IsDead) {
                continue;
            }

            if (p.Role == Role.Tank) {
                if (!addedTank) {
                    CurrentBaits.Add(new(p, boss, new AOEShapeCircle(3f), forbidden: ~allowedTanks));
                    addedTank = true;
                }
            }

            if (p.Role == Role.Healer) {
                if (!addedHealer) {
                    CurrentBaits.Add(new(p, boss, new AOEShapeCircle(5f), forbidden: ~allowedHealers));
                    addedHealer = true;
                }
            }

            if (p.Role == Role.Melee || p.Role == Role.Ranged) {
                if (!addedDD) {
                    CurrentBaits.Add(new(p, boss, new AOEShapeCircle(5f), forbidden: ~allowedDDs));
                    addedDD = true;
                }
            }
        }
        setup = true;
    }
}

class ChaoticFlood(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ChaoticFloodAOE, new AOEShapeRect(40.0f, 5.0f)) {
    // These are stored here since many different strategies use different waymarks, but these should work for all
    private WPos waymarkA = new(100.0f, 88.0f);
    private WPos waymarkB = new(112.0f, 100.0f);
    private WPos waymarkC = new(100.0f, 112.0f);
    private WPos waymarkD = new(88.0f, 100.0f);

    private List<AOEInstance> aoes = [];
    private List<AOEInstance> aoesHints = []; // Used for when the aoes are first cast as hints
    private int aoesHintsDisplayed = 0;

    private List<WPos> safeWaymarks = new();
    private bool? clockwise = null;
    private bool firstWaveResolved = false;
    private bool secondWaveResolved = false;
    private WDir currentDirection;
    private int castsResolved = 0;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.ChaoticFloodAOEDisplay) {
            aoes.Add(new(new AOEShapeRect(20.0f, 5.0f, 20.0f), caster.Position, caster.Rotation, actorID: caster.InstanceID));
            aoesHints.Add(new(new AOEShapeRect(20.0f, 5.0f, 20.0f), caster.Position, caster.Rotation, color: Colors.Enemy, actorID: caster.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.ChaoticFloodAOEDisplay) {
            aoesHintsDisplayed++;
            aoesHints.RemoveAt(0);
        }

        if (spell.Action.ID == (uint)AID.ChaoticFloodAOE) {
            if (++castsResolved == 2) {
                castsResolved = 0;
                if (clockwise != null) {
                    currentDirection = clockwise.Value ? new WDir(-currentDirection.Z, currentDirection.X) : new WDir(currentDirection.Z, -currentDirection.X);
                }
            }

            NumCasts++;
            aoes.RemoveAt(0);
        }
    }
    
    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (secondWaveResolved == false || clockwise == null || NumCasts == 8) {
            return;
        }

        Arena.AddCircle(Module.Center + currentDirection * 2.0f, 1.0f, Colors.Safe, 2.0f);

        if (NumCasts <= 4) {
            var nextDirection = clockwise.Value ? new WDir(-currentDirection.Z, currentDirection.X) : new WDir(currentDirection.Z, -currentDirection.X);
            Arena.AddCircle(Module.Center + nextDirection * 2.0f, 1.0f, Colors.Danger, 2.0f);
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        if (aoesHintsDisplayed < 8) {
            return CollectionsMarshal.AsSpan(aoesHints.Take(2).ToList());
        }

        return CollectionsMarshal.AsSpan(aoes.Take(4).ToList());
    }

    public override void Update() {
        if (firstWaveResolved == false && aoes.Count >= 2) {
            List<WPos> waymarks = [waymarkA, waymarkB, waymarkC, waymarkD];
            var aoesFirstWave = aoes.Take(2).ToList();

            foreach (var waymark in waymarks) {
                if (!aoesFirstWave.Any(aoe => aoe.Check(waymark))) {
                    safeWaymarks.Add(waymark);
                }
            }

            firstWaveResolved = true;
        }

        if (firstWaveResolved == true && secondWaveResolved == false && aoes.Count >= 4) {
            var aoesSecondWave = aoes.Skip(2).Take(2).ToList();
            foreach (var waymark in safeWaymarks) {
                if (!aoesSecondWave.Any(aoe => aoe.Check(waymark))) {
                    var otherWaymark = safeWaymarks.First(point => point != waymark) - Module.Center;
                    clockwise = otherWaymark.X * (waymark - Module.Center).Z - otherWaymark.Z * (waymark - Module.Center).X > 0;
                    currentDirection = (waymark - Module.Center).Normalized();
                    break;
                }
            }
            secondWaveResolved = true;
        }
    }
}

/*
    ChaoticFlood = 47951, // Helper->players, no cast, range 6 circle
    ChaoticFloodAOEDisplay = 49539, // Helper->self, 1.5s cast, range 40 width 10 rect
    ChaoticFloodAOE = 49769, // Helper->self, no cast, range 40 width 10 rect
    ChaoticFloodStack = 49471, // KefkaP5->self, 5.0+1.2s cast, single-target
 */
