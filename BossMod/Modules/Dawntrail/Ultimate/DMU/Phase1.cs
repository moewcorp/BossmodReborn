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
            secondTB = true;
            activation = WorldState.FutureTime(0.3f); // TODO random guess find the actual timing
        }

        if (spell.Action.ID == (uint)AID.RevoltingRuinIII1) {
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

// TODO improve hints so it glows red if the player will fall out of the map
// TODO only show the players tether not everyones
// TODO should be able to use the normal tether function or something - look around for knockback stuff
class GravenImage(BossModule module) : BossComponent(module) {
    private List<(Actor source, Actor player)> tethers = [];

    public override void OnTethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID == (uint)TetherID.GravenImageTether) {
            var target = WorldState.Actors.Find(tether.Target);

            if (target != null) {
                tethers.Add((source, target));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.PulseWave) {
            tethers.Clear();
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        foreach (var (source, target) in tethers) {
            var direction = (target.Position - source.Position).Normalized();
            Arena.AddLine(target.Position, target.Position + direction * 9f, Colors.Danger);
        }
    }
}

// TODO make it so the function cleans up
class StackSpreadOrbs(BossModule module) : Components.UniformStackSpread(module, 6f, 6f, 4, 4) {
    private bool? spread = null;
    private IconID? iconID = null;

    public override void Update() {
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

// TODO make it so the function cleans up
class BlizzardSafeSpots(BossModule module) : Components.GenericAOEs(module) {

    //      _Ability_BlizzardIIIBlowout = 47774, // Helper->self, 5.0s cast, range 40 ?-degree cone - EMPTY ZONE
    //      _Ability_BlizzardIIIBlowout1 = 47771, // Helper->self, 5.0s cast, range 40 ?-degree cone - QUESTIONMARK ZONE
    //      _Ability_BlizzardIIIBlowout2 = 47768, // Helper->self, 5.0s cast, range 40 ?-degree cone - QUESTIONMARK ZONE

    //     _Gen_Icon_m0462trg_c03c = 675, // Kefka->self // Questionmark - most likely upper ring - 2A3
    //     _Gen_Icon_m0462trg_c04c = 676, // Kefka->self // Blue orb - most likely upper ring - 2A4

    // Questionmark means go into zone

    private bool questionMark = false;
    private AOEShapeCone cone = new(40f, 45f.Degrees());
    private List<(uint AID, AOEInstance AOE)> AOEsAvailable = [];
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
            AOEsAvailable.Add((spell.Action.ID, new AOEInstance(cone, caster.Position, caster.Rotation, default, actorID: caster.InstanceID)));;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        aoes.Clear();

        for (int i = 0; i < AOEsAvailable.Count; i++) {
            var currentAOE = AOEsAvailable[i];

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

// TODO 2nd mechanic - add wave cannon stuff
// TODO 3rd mechanic
// TODO 4th mechanic
