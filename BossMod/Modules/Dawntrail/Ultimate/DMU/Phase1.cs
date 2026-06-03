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
