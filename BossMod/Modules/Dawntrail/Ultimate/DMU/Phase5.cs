using BossMod.Stormblood.Foray.BaldesionArsenal.BA3AbsoluteVirtue;

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
    public int expectedCasts = 9;

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

            if (NumCasts == expectedCasts) {
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

class ChaoticFloodStack(BossModule module) : Components.GenericStackSpread(module) {
    public int NumCasts = 0;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.ChaoticFlood) {
            var target = WorldState.Actors.Find(caster.TargetID);
            if (target == null) {
                return;
            }
            Stacks.Add(new(target, 6.0f, 8, 8));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.ChaoticFloodStack) {
            NumCasts++;

            if (NumCasts == 4) {
                Stacks.Clear();
            }
        }
    }
}

class MaddeningOrchestra(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true) {
    private bool active = false;
    private readonly DateTime[] magicVulnerability = new DateTime[PartyState.MaxPartySize];
    private bool firstWave = false;

    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        if (status.ID == (uint)SID.MagicVulnerabilityUp) {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0) {
                magicVulnerability[slot] = status.ExpireAt;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.MaddeningOrchestra) {
            active = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.Holy || spell.Action.ID == (uint)AID.Flare) {
            NumCasts++;

            if (NumCasts >= 5) {
                firstWave = true;
            }
        }
    }

    public override void Update() {
        CurrentBaits.Clear();

        if (active == false) {
            return;
        }

        var boss = ((DMU)Module).KefkaP5();
        if (boss == null) {
            return;
        }

        var targets = Raid.WithoutSlot().SortedByRange(boss.Position).ToList();
        if (firstWave == false) {
            foreach (var player in targets) { // Tanks & rest of party are hit with different spells, but same size AOEs
                CurrentBaits.Add(new(boss.Position, player, new AOEShapeCircle(5.0f)));
            }

            return;
        }

        targets = targets.Take(3).ToList();
        BitMask forbiddenPlayers = Raid.WithSlot().Where(p => magicVulnerability[p.Item1] > WorldState.CurrentTime || p.Item2.Role == Role.Tank).Mask();
        foreach (var target in targets) {
            CurrentBaits.Add(new(boss.Position, target, new AOEShapeCircle(5.0f), forbidden: forbiddenPlayers));
        }

        ForbiddenPlayers = forbiddenPlayers;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        base.AddHints(slot, actor, hints);

        if (!ForbiddenPlayers[slot] && firstWave == true) {
            if (IsBaitTarget(actor)) {
                hints.Add("Bait!", false);
            } else {
                hints.Add("Bait!");
            }
        }
    }
}

class ChaoticFlareTB(BossModule module) : Components.GenericBaitStack(module, (uint)AID.ChaoticFlareTB) {
    public bool active = false;

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.ChaoticFlareTB) {
            NumCasts++;
            active = false;
        }
    }

    public override void Update() {
        if (active == false) {
            return;
        }

        var boss = ((DMU)Module).KefkaP5();
        if (boss == null) {
            return;
        }

        CurrentBaits.Clear();

        var party = Raid.WithSlot();
        BitMask allowedTanks = default;

        for (int i = 0; i < party.Length; i++) {
            ref var p = ref party[i];

            if (p.Item2.Role == Role.Tank) {
                allowedTanks.Set(p.Item1);
            }
        }

        var addedTank = false;

        for (int i = 0; i < party.Length; i++) {
            ref var player = ref party[i];
            var p = player.Item2;

            if (p.IsDead) {
                continue;
            }

            if (p.Role == Role.Tank) {
                if (!addedTank) {
                    CurrentBaits.Add(new(p, boss, new AOEShapeCircle(5.0f), forbidden: ~allowedTanks));
                    addedTank = true;
                }
            }
        }
    }
}

class ChaoticHolyFlareDiffusion(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true, onlyShowOutlines: true) {
    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        if (status.ID == (uint)SID.SurpriseHoly) {
            CurrentBaits.Add(new(actor, actor, new AOEShapeCircle(6.0f), status.ExpireAt));
        }

        if (status.ID == (uint)SID.SurpriseFlare) {
            CurrentBaits.Add(new(actor, actor, new AOEShapeCircle(25.0f), status.ExpireAt));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.FlareDiffusion || spell.Action.ID == (uint)AID.ChaoticHoly) {
            NumCasts++;

            if (NumCasts == 2) {
                CurrentBaits.Clear();
            }
        }
    }

}

// Towers sets ~7.5 seconds apart, towers explosions and tower glows happen at the same time, so remove the tower instance from the list
class Celestriad(BossModule module) : Components.GenericTowers(module) {
    private List<Actor> towers = [];

    public override void OnActorEAnim(Actor actor, uint state) {
        if (actor.OID == (uint)OID.IceTower || actor.OID == (uint)OID.FireTower || actor.OID == (uint)OID.ThunderTower) {
            if (state == (uint)Animations.TowerGlow) {
                Towers.Add(new(actor.Position, 3.0f, 2, 2));
            }

        }
    }

    //  TowerGlow = 1048608,
    //  TowerExplosion = 65600,


}

// TODO Celestriad should be pretty easy to implement
//  Create a list of players with slots[8] and each player will gain debuffs (2 people will not get debuffs, but in a way this is a debuff)
//  Only four towers are ever active at a time meaning we can make things simple
//  Index all 9 towers in order from A and then create a list of active towers with their index reference
//  1st set just debuff players CW from A into the first available tower they can take, non-debuffs ccw or just whichever tower is left over
//      This works for non debuff players since we have the index in order, so if tower 1,3 we know which one is CW first
//  2nd set: Now since we have the last set indexed, we can just get people to rotate from their last set tower position until they find another safe one

// Might be easier to get the tower order set of elemenets like { ICE, FIRE, THUNDER} as this will tell us the order
// Then we can solve buffs easier like on slide 17 since it always set and the only one that needs to be figured out is the 2 towers of same element
// Could be combained with the method above

// TODO after towers setup add the in/out mechanic then its done
// TODO add safe spots to MaddeningOrchestra, add config option of 1-6 (somehow don't include tanks or set them to 0), 0 players will not be included
//  Lowest number is left, highest number is right, last number remaining is middle
