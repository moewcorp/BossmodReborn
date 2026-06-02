namespace BossMod.Dawntrail.Ultimate.DMU;

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

// TODO fix sizing of AOEs - they're not this big
// TODO remove valueSet variable - shouldn't be needed
// TODO figure out who actually gets the stack instead of randomly guessing
// TODO make it so the function cleans up
class StackSpreadOrbs(BossModule module) : Components.UniformStackSpread(module, 6f, 5f, 4, 4) {

    //     _Gen_Icon_m0462trg_c01c = 673, // Kefka->self // Questionmark - most likely lower ring - 2A1
    //     _Gen_Icon_m0462trg_c02c = 674, // Kefka->self // Blue orb - most likely lower ring - 2A2
    //     _Gen_Icon_m0462trg_a0c = 127, // player->self // Spread
    //     _Gen_Icon_m0462trg_b0c = 128, // player->self // Stack

    private bool spread = false;
    private bool valueSet = false;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID) {
        if (iconID == (uint)IconID.spreadIcon) {
            spread = true;
            valueSet = true;
        }

        if (iconID == (uint)IconID.stackIcon) {
            spread = false;
            valueSet = true;
        }

        // We do the opposite of whatever we are told
        if (iconID == (uint)IconID.FireRingQuestionMark) {
            if (valueSet == true) {
                if (spread == true) {
                    var support = Raid.WithoutSlot(true, true, true).FirstOrDefault(p => p.Class.IsSupport());
                    var dps = Raid.WithoutSlot(true, true, true).FirstOrDefault(p => p.Class.IsDD());

                    if (support == null || dps == null) {
                        return;
                    }

                    AddStack(support, default);
                    AddStack(dps, default);
                }

                if (spread == false) {
                    AddSpreads(Raid.WithoutSlot(true, true, true), default);
                }
            }
        }

        // We do what we are told
        if (iconID == (uint)IconID.FireRingBlueOrb) {
            if (valueSet == true) {
                if (spread == false) {
                    var support = Raid.WithoutSlot(true, true, true).FirstOrDefault(p => p.Class.IsSupport());
                    var dps = Raid.WithoutSlot(true, true, true).FirstOrDefault(p => p.Class.IsDD());

                    if (support == null || dps == null) {
                        return;
                    }

                    AddStack(support, default);
                    AddStack(dps, default);
                }
            }

            if (spread == true) {
                AddSpreads(Raid.WithoutSlot(true, true, true), default);
            }
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
        if (spell.Action.ID == (uint)AID._Ability_BlizzardIIIBlowout ||
            spell.Action.ID == (uint)AID._Ability_BlizzardIIIBlowout1 ||
            spell.Action.ID == (uint)AID._Ability_BlizzardIIIBlowout2) {
            AOEsAvailable.Add((spell.Action.ID, new AOEInstance(cone, caster.Position, caster.Rotation, default, actorID: caster.InstanceID)));;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        aoes.Clear();

        for (int i = 0; i < AOEsAvailable.Count; i++) {
            var currentAOE = AOEsAvailable[i];

            if (questionMark == true) {
                if (currentAOE.AID == (uint)AID._Ability_BlizzardIIIBlowout) {
                    aoes.Add(currentAOE.AOE);
                }
            }

            if (questionMark == false) {
                if (currentAOE.AID == (uint)AID._Ability_BlizzardIIIBlowout1 || currentAOE.AID == (uint)AID._Ability_BlizzardIIIBlowout2) {
                    aoes.Add(currentAOE.AOE);
                }
            }
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

// TODO add tank buster
// TODO 2nd mechanic - add wave cannon stuff
// TODO 3rd mechanic
// TODO 4th mechanic
