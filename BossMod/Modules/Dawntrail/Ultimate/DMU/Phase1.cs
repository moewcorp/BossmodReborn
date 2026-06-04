namespace BossMod.Dawntrail.Ultimate.DMU;

class RevoltingRuinIII(BossModule module) : Components.GenericBaitAway(module, (uint)AID.RevoltingRuinIII, tankbuster: true,
    damageType: AIHints.PredictedDamageType.Tankbuster) {

    private Actor? source; // If this ever becomes null, it means the mechanic has not started, or it has ended
    private Actor? target;
    private DateTime activation;
    private bool secondTB = false;

    public override void Update() {
        CurrentBaits.Clear();
        if (source == null) {
            return;
        }

        Actor? target = secondTB ? FindSecondAggroTarget(source) : this.target;
        if (target != null) {
            CurrentBaits.Add(new(source, target, new AOEShapeCone(30f, 45f.Degrees()), activation));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.RevoltingRuinIII) {
            source = caster;
            target = WorldState.Actors.Find(spell.TargetID);
            activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.RevoltingRuinIII) {
            NumCasts++;
            secondTB = true;
            activation = WorldState.FutureTime(0.3f); // TODO random guess find the actual timing
        }

        if (spell.Action.ID == (uint)AID.RevoltingRuinIII1) {
            NumCasts++;
            source = null;
            CurrentBaits.Clear();
        }
    }

    private Actor? FindSecondAggroTarget(Actor boss) {
        var hate = WorldState.Client.CurrentTargetHate;
        if (hate.InstanceID != boss.InstanceID) {
            return null;
        }

        Actor? mostHated = null;
        int bestEnmity = int.MinValue;
        Actor? secondMostHated = null;
        int secondBestEnmity = int.MinValue;

        foreach (var player in hate.Targets) {
            var actor = WorldState.Actors.Find(player.InstanceID);
            if (actor == null || Raid.FindSlot(actor.InstanceID) < 0) {
                continue;
            }

            if (player.Enmity > bestEnmity) {
                secondMostHated = mostHated; // The 1st person in aggro becomes the 2nd player in aggro
                secondBestEnmity = bestEnmity;
                mostHated = actor;
                bestEnmity = player.Enmity;
            } else if (player.Enmity > secondBestEnmity && (mostHated == null || actor.InstanceID != mostHated.InstanceID)) {
                secondMostHated = actor;
                secondBestEnmity = player.Enmity;
            }
        }

        return secondMostHated;
    }
}

class GravenImage(BossModule module) : Components.GenericKnockback(module, (uint)AID.PulseWave) {
    private List<(ulong SourceID, int slot)> tethers = [];

    public override void OnTethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID != (uint)TetherID.GravenImageTether) {
            return;
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null) {
            return;
        }

        var slot = Raid.FindSlot(tether.Target);
        if (slot < 0) {
            return;
        }

        tethers.Add((source.InstanceID, slot));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.PulseWave) {
            NumCasts++;
            tethers.Clear();
        }
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) {
        foreach (var target in tethers) {
            if (target.slot != slot) {
                continue;
            }

            var source = WorldState.Actors.Find(target.SourceID);
            if (source == null) {
                continue;
            }

            return new Knockback[] { new(source.Position, 14f, actorID: source.InstanceID) };
        }

        return [];
    }
}

class StackSpreadOrbs(BossModule module) : Components.UniformStackSpread(module, 6f, 5f, 4, 4) {
    private bool? spread = null;
    private IconID? iconID = null;

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.FlagrantFireIIIStack || spell.Action.ID == (uint)AID.FlagrantFireIIISpread) {
            spread = null;
            iconID = null;
        }
    }

    public override void Update() {
        Stacks.Clear();
        Spreads.Clear();

        if (spread == null || iconID == null) {
            return;
        }

        // We do the opposite of whatever we are told
        if (iconID == IconID.FireRingQuestionMark) {
            if (spread == true) {
                var support = Raid.WithoutSlot(true, true, true).FirstOrDefault(p => p.Class.IsSupport());
                var dps = Raid.WithoutSlot(true, true, true).FirstOrDefault(p => p.Class.IsDD());

                if (support == null || dps == null) {
                    return;
                }

                AddStack(support);
                AddStack(dps);
            }

            if (spread == false) {
                AddSpreads(Raid.WithoutSlot(true, true, true));
            }
        }

        // We do what we are told
        if (iconID == IconID.FireRingBlueOrb) {
            if (spread == false) {
                var support = Raid.WithoutSlot(true, true, true).FirstOrDefault(p => p.Class.IsSupport());
                var dps = Raid.WithoutSlot(true, true, true).FirstOrDefault(p => p.Class.IsDD());

                if (support == null || dps == null) {
                    return;
                }

                AddStack(support);
                AddStack(dps);
            }

            if (spread == true) {
                AddSpreads(Raid.WithoutSlot(true, true, true));
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID) {
        if (iconID == (uint)IconID.spreadIcon) {
            spread = true;
        }

        if (iconID == (uint)IconID.stackIcon) {
            spread = false;
        }

        if (iconID == (uint)IconID.FireRingQuestionMark) {
            this.iconID = (IconID)iconID;
        }

        if (iconID == (uint)IconID.FireRingBlueOrb) {
            this.iconID = (IconID)iconID;
        }
    }
}

class BlizzardSafeSpots(BossModule module) : Components.GenericAOEs(module) {
    private bool questionMark = false;
    private List<(uint AID, AOEInstance AOE)> aoesAvailable = [];
    private List<AOEInstance> aoes = [];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID) {
        if (iconID == (uint)IconID.BlueRingQuestionMark) {
            questionMark = true;
        }

        if (iconID == (uint)IconID.BlueRingBlueOrb) {
            questionMark = false;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.BlizzardIIIBlowout ||
            spell.Action.ID == (uint)AID.BlizzardIIIBlowout1 ||
            spell.Action.ID == (uint)AID.BlizzardIIIBlowout2) {
            aoesAvailable.Add((spell.Action.ID, new AOEInstance(new AOEShapeCone(40f, 45f.Degrees()), caster.Position, caster.Rotation, actorID: caster.InstanceID)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.BlizzardIIIBlowout ||
            spell.Action.ID == (uint)AID.BlizzardIIIBlowout1 ||
            spell.Action.ID == (uint)AID.BlizzardIIIBlowout2) {
            NumCasts++;
            aoesAvailable.Clear();
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        aoes.Clear();

        foreach (var currentAOE in aoesAvailable) {
            if (questionMark == true) {
                if (currentAOE.AID == (uint)AID.BlizzardIIIBlowout) {
                    aoes.Add(currentAOE.AOE);
                }
            }

            if (questionMark == false) {
                if (currentAOE.AID == (uint)AID.BlizzardIIIBlowout1 || currentAOE.AID == (uint)AID.BlizzardIIIBlowout2) {
                    aoes.Add(currentAOE.AOE);
                }
            }
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

// TODO ensure size is correct - it should be
// TODO this is a bit of a lazy way to do it, but unsure how you would actually figure out the 4 people who are getting hit at the moment
// TODO however, its not needed to know who is getting hit as you can't get hit by a laser since you have to take a tower next
class WaveCannon(BossModule module) : Components.BaitAwayEveryone(module,
    module.Enemies((uint)OID.StatueBodyOrb).FirstOrDefault(), new AOEShapeRect(100f, 2f),
    (uint)AID.WaveCannon) {

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.WaveCannon) {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
}

// TODO add priority system
// TODO improve display of towers
// TODO mostly will be combined with priority, but if you have a magic vuln debuff must highlight not to take a tower
class WaveCannonTowers(BossModule module) : Components.CastTowers(module, (uint)AID.TowerExplosion, 4);

class LightningSafeSpots(BossModule module) : Components.GenericAOEs(module) {

    private bool questionMark = false;
    private List<(uint AID, AOEInstance AOE)> aoesAvailable = [];
    private List<AOEInstance> aoes = [];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID) {
        if (iconID == (uint)IconID.PurpleRingQuestionMark) {
            questionMark = true;
        }

        if (iconID == (uint)IconID.PurpleRingBlueOrb) {
            questionMark = false;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.ThrummingThunderIII ||
            spell.Action.ID == (uint)AID.ThrummingThunderIII1 ||
            spell.Action.ID == (uint)AID.ThrummingThunderIII2) {
            aoesAvailable.Add((spell.Action.ID, new AOEInstance(new AOEShapeRect(40f, 5f), caster.Position, caster.Rotation, actorID: caster.InstanceID)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.ThrummingThunderIII ||
             spell.Action.ID == (uint)AID.ThrummingThunderIII1 ||
             spell.Action.ID == (uint)AID.ThrummingThunderIII2) {
            NumCasts++;
            aoesAvailable.Clear();
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        aoes.Clear();

        foreach (var currentAOE in aoesAvailable) {
            if (questionMark == true) {
                if (currentAOE.AID == (uint)AID.ThrummingThunderIII2) {
                    aoes.Add(currentAOE.AOE);
                }
            }

            if (questionMark == false) {
                if (currentAOE.AID == (uint)AID.ThrummingThunderIII || currentAOE.AID == (uint)AID.ThrummingThunderIII1) {
                    aoes.Add(currentAOE.AOE);
                }
            }
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

// TODO bosses hitbox for non debuff
// TODO can most likely add a slight offset for debuff players
class DoubleTroubleTrapStacks(BossModule module) : Components.UniformStackSpread(module, 6f, 5f, 4, 4) {
    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        if (status.ID == (uint)SID.DoubleTroubleTrap) {
            AddStack(actor, status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status) {
        if (status.ID == (uint)SID.DoubleTroubleTrap) {
            Stacks.Clear();
        }
    }
}

class DoubleTroubleTrapKnockback(BossModule module) : Components.GenericKnockback(module, (uint)AID.DoubleTroubleTrap1) {
    private List<Knockback> knockbacks = [];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) {
        knockbacks.Clear();

        var stack = Module.FindComponent<DoubleTroubleTrapStacks>();
        if (stack == null) {
            return [];
        }

        foreach (var stackPoint in stack.Stacks) {
            if (actor.Position.InCircle(stackPoint.Target.Position.Quantized(), stackPoint.Radius)) {
                knockbacks.Add(new(stackPoint.Target.Position, 10f, stackPoint.Activation, actorID: stackPoint.Target.InstanceID));
            }
        }

        return CollectionsMarshal.AsSpan(knockbacks);
    }
}

class HyperDrive(BossModule module) : Components.GenericBaitAway(module, (uint)AID.Hyperdrive, centerAtTarget: true, tankbuster: true,
    damageType: AIHints.PredictedDamageType.Tankbuster) {
    private DateTime? activation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.LightOfJudgment) {
            activation = Module.CastFinishAt(spell, 3.0f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == WatchedAction) {
            NumCasts++;

            if (NumCasts >= 3) {
                activation = null;
            }
        }
    }

    public override void Update() {
        CurrentBaits.Clear();

        if (activation != null) {
            var target = WorldState.Actors.Find(Module.PrimaryActor.TargetID);
            if (target != null) {
                CurrentBaits.Add(new(Module.PrimaryActor, target, new AOEShapeCircle(5), activation.Value));
            }
        }
    }
}

// TODO Improve logic for stack green circles - ensure the 2nd circles are not inside the first AOE
// TODO Improve logic for spread green circles - should only show role one not all 4
// TODO party config settings required - if none set then just use what is already there
class Gravitas(BossModule module) : Components.UniformStackSpread(module, 5, 5, 4, 4) {
    private List<Spread> spreadsIncoming = [];
    private int totalStacks = 0;

    private enum TetherGroup { Support, DPS }
    private TetherGroup tetherGroup = TetherGroup.Support;

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        base.DrawArenaForeground(pcSlot, pc);

        if (Stacks.Count != 0) {
            Arena.AddCircle(Module.Center + new WDir(0, 19.5f), 1.0f, Colors.Safe, 2);
            Arena.AddCircle(Module.Center + new WDir(0, -19.5f), 1.0f, Colors.Safe, 2);
        }

        if (Stacks.Count != 0 && totalStacks >= 5) {
            Arena.AddCircle(Module.Center + new WDir(0, 13.0f), 1.0f, Colors.Safe, 2);
            Arena.AddCircle(Module.Center + new WDir(0, -13.0f), 1.0f, Colors.Safe, 2);
        }

        if (Spreads.Count != 0) {
            if (tetherGroup == TetherGroup.Support ? pc.Class.IsSupport() : tetherGroup == TetherGroup.DPS && pc.Class.IsDD()) {
                // MT/M1 - [90.054, 0.000, 100.718, -178.033]
                Arena.AddCircle(new WPos(90.054f, 100.718f), 1.0f, Colors.Safe, 2);

                // H1/R1 - [87.971, 0.000, 86.119, 37.967]
                Arena.AddCircle(new WPos(87.971f, 86.119f), 1.0f, Colors.Safe, 2);

                // OT/M2 - [109.197, 0.000, 99.618, -87.793]
                Arena.AddCircle(new WPos(109.197f, 99.618f), 1.0f, Colors.Safe, 2);

                // H2/R2 - [113.566, 0.000, 112.415, -133.273]
                Arena.AddCircle(new WPos(113.566f, 112.415f), 1.0f, Colors.Safe, 2);
            }
            else {
                // Middle
                Arena.AddCircle(Module.Center, 1.0f, Colors.Safe, 2);
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID != (uint)TetherID.GravenImageTether) {
            return;
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null) {
            return;
        }

        var slot = Raid.FindSlot(tether.Target);
        if (slot < 0) {
            return;
        }

        if (source.Position.AlmostEqual(new(102.500f, 22.500f), 5)) {
            AddStack(target, WorldState.FutureTime(6.5f));
            totalStacks++;
        }

        if (source.Position.AlmostEqual(new(126.000f, 41.500f), 5)) {
            spreadsIncoming.Add(new(target, 5, WorldState.FutureTime(10.6f)));
            tetherGroup = target.Class.IsSupport() ? TetherGroup.Support : TetherGroup.DPS;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.Gravitas) {
            Stacks.Clear();
            Spreads.AddRange(spreadsIncoming);
            spreadsIncoming.Clear();
        }

        if (spell.Action.ID == (uint)AID.Vitrophyre) {
            Spreads.Clear();
        }
    }
}

// TODO voidzone don't check for event cast sadly, maybe add it?
// TODO needs to be cleaned up - change the colour during a specific point in time and maybe just count the number of times the spell has occured instead
class GravitasPuddles(BossModule module) : Components.PersistentInvertibleVoidzoneByCast(module, 5,
    m => m.Enemies((uint)OID._Gen_Actor1ec022).Where(e => e.EventState != 7), (uint)AID.Gravitas)  {

    public override void DrawArenaBackground(int pcSlot, Actor pc) {
        var count = 0;
        foreach (var puddles in Sources(Module)) {
            count++;
        }

        var colour = count == 8 ? Colors.SafeFromAOE : default;
        foreach (var puddles in Sources(Module)) {
            Shape.Draw(Arena, puddles.Position, puddles.Rotation, colour);
        }
    }
}
