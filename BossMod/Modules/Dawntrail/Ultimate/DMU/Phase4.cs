namespace BossMod.Dawntrail.Ultimate.DMU;

class GrandCrossRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.GrandCross);

// GrandCross will be casted three times in a row with a fake or real
class GrandCrossOrder(BossModule module) : BossComponent(module) {
    private List<(bool? tellingTruth, int set, List<(SID buff, DateTime expireAt)>[] playerBuffs)> grandCross = new();
    private bool tellingTruthCaught = false; // Two orbs spawn per cast, but we only want one
    private int NumCasts = 0;
    public int currentCast = 0; // Used for state machine so it easier to detect when the cast has finished

    public override void OnCastFinished(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.GrandCross) {
            tellingTruthCaught = false;
            currentCast++;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        if (actor.OID == (uint)OID.NeoExdeath && status.ID == (uint)SID.TellingTruthBuff) {
            if ((status.Extra == 0x461 || status.Extra == 0x462) && tellingTruthCaught == false) {
                var buffs = new List<(SID buff, DateTime expireAt)>[PartyState.MaxPartySize];
                for (var i = 0; i < buffs.Length; i++) {
                    buffs[i] = new();
                }
                grandCross.Add((status.Extra == 0x462, NumCasts, buffs)); // 0x461 is fake, 0x462 is real
                NumCasts++;
                tellingTruthCaught = true;
            }
        }

        if (status.ID == (uint)SID.BeyondDeath || status.ID == (uint)SID.AllaganField ||
            status.ID == (uint)SID.WhiteWound || status.ID == (uint)SID.BlackWound) {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0) {
                var id = grandCross.FindIndex(e => e.set == NumCasts - 1);
                if (id >= 0) {
                    grandCross[id].playerBuffs[slot].Add(((SID)status.ID, status.ExpireAt));
                }
            }
        }

        if (status.ID == (uint)SID.ForkedLightning || status.ID == (uint)SID.CompressedWater ||
            status.ID == (uint)SID.AccelerationBomb || status.ID == (uint)SID.CursedShriek) {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0) {
                var id = grandCross.FindIndex(e => e.set == NumCasts - 1);
                if (id >= 0) {
                    grandCross[id].playerBuffs[slot].Add(((SID)status.ID, status.ExpireAt));
                }
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status) {
        if (status.ID == (uint)SID.BeyondDeath || status.ID == (uint)SID.AllaganField ||
            status.ID == (uint)SID.WhiteWound || status.ID == (uint)SID.BlackWound ||
            status.ID == (uint)SID.ForkedLightning || status.ID == (uint)SID.CompressedWater ||
            status.ID == (uint)SID.AccelerationBomb || status.ID == (uint)SID.CursedShriek) {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0) {
                var sid = (SID)status.ID;
                var expireAt = status.ExpireAt;
                foreach (var entry in grandCross) {
                    entry.playerBuffs[slot].RemoveAll(b => b.buff == sid && b.expireAt == expireAt);
                }
            }
        }
    }

    public IReadOnlyList<(SID buff, DateTime expireAt)>? getCurrentPlayerBuffs(int slot, int set) {
        var id = grandCross.FindIndex(e => e.set == set);
        if (id < 0 || slot < 0) {
            return null;
        }

        return grandCross[id].playerBuffs[slot];
    }

    public IReadOnlyList<(int slot, DateTime expireAt, bool? tellingTruth)> getNextBuffPlayers(SID status, int count) {
        var result = new List<(int slot, DateTime expireAt, bool? tellingTruth)>();
        foreach (var index in grandCross) {
            for (int slot = 0; slot < index.playerBuffs.Length; slot++) {
                foreach (var buff in index.playerBuffs[slot]) {
                    if (buff.buff == status) {
                        result.Add((slot, buff.expireAt, index.tellingTruth));
                    }
                }
            }
        }
        return result.OrderBy(buff => buff.expireAt).Take(count).ToList();
    }

    // TODO remove
    public override void Update() {
        foreach (var (tellingTruth, wave, playerBuffs) in grandCross) {
            var buffsStr = string.Join(", ", playerBuffs.Select((buffs, slot) =>
                $"P{slot}:[{string.Join(",", buffs.Select(b => $"{b.buff}@{b.expireAt}"))}]"));
            //Service.Logger.Info($"Wave {wave} tellingTruth={tellingTruth} | {buffsStr}");
        }
    }
}

// TODO merge both raid wides together
class TsunamiRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.P4Tsunami);
class InfernoRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.P4Inferno);

// Elements will be casted two times in a row with a fake or real - TODO verify the status is working correctly
class TsunamiInfernoOrder(BossModule module) : BossComponent(module) {
    private List<(bool? tellingTruth, int set, List<(SID buff, DateTime expireAt)>[] playerBuffs)> tsunamiInferno = new();
    private bool tellingTruthCaught = false; // Two orbs spawn per cast, but we only want one
    private int NumCasts = 0;
    public int currentCast = 0; // Used for state machine so it easier to detect when the cast has finished

    public override void OnCastFinished(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.P4Inferno || spell.Action.ID == (uint)AID.P4Tsunami) {
            tellingTruthCaught = false;
            currentCast++;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        if (actor.OID == (uint)OID.ChaosP4 && status.ID == (uint)SID.TellingTruthBuff) {
            if ((status.Extra == 0x45F || status.Extra == 0x460) && tellingTruthCaught == false) {
                var buffs = new List<(SID buff, DateTime expireAt)>[PartyState.MaxPartySize];
                for (var i = 0; i < buffs.Length; i++) {
                    buffs[i] = new();
                }
                tsunamiInferno.Add((status.Extra == 0x460, NumCasts, buffs)); // 0x45F is fake, 0x460 is real
                NumCasts++;
                tellingTruthCaught = true;
            }
        }

        if (status.ID == (uint)SID.DynamicFluidP4 || status.ID == (uint)SID.EntropyP4) {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0) {
                var id = tsunamiInferno.FindIndex(e => e.set == NumCasts - 1);
                if (id >= 0) {
                    tsunamiInferno[id].playerBuffs[slot].Add(((SID)status.ID, status.ExpireAt));
                }
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status) {
        if (status.ID == (uint)SID.DynamicFluidP4 || status.ID == (uint)SID.EntropyP4) {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0) {
                var sid = (SID)status.ID;
                var expireAt = status.ExpireAt;
                foreach (var entry in tsunamiInferno) {
                    entry.playerBuffs[slot].RemoveAll(b => b.buff == sid && b.expireAt == expireAt);
                }
            }
        }
    }

    public IReadOnlyList<(SID buff, DateTime expireAt)>? getCurrentPlayerBuffs(int slot, int set) {
        var id = tsunamiInferno.FindIndex(e => e.set == set);
        if (id < 0 || slot < 0) {
            return null;
        }

        return tsunamiInferno[id].playerBuffs[slot];
    }

    public IReadOnlyList<(int slot, DateTime expireAt, bool? tellingTruth)> getNextBuffPlayers(SID status, int count) {
        var result = new List<(int slot, DateTime expireAt, bool? tellingTruth)>();
        foreach (var index in tsunamiInferno) {
            for (int slot = 0; slot < index.playerBuffs.Length; slot++) {
                foreach (var buff in index.playerBuffs[slot]) {
                    if (buff.buff == status) {
                        result.Add((slot, buff.expireAt, index.tellingTruth));
                    }
                }
            }
        }
        return result.OrderBy(buff => buff.expireAt).Take(count).ToList();
    }

    // TODO remove
    public override void Update() {
        foreach (var (tellingTruth, wave, playerBuffs) in tsunamiInferno) {
            var buffsStr = string.Join(", ", playerBuffs.Select((buffs, slot) =>
                $"P{slot}:[{string.Join(",", buffs.Select(b => $"{b.buff}@{b.expireAt}"))}]"));
            //Service.Logger.Info($"Wave {wave} tellingTruth={tellingTruth} | {buffsStr}");
        }
    }
}

// Kefka will cast truth and lies throughout the encounter which need to be tracked
class KefkaOrder(BossModule module) : BossComponent(module) {
    private bool active = false;
    private List<(bool tellingTruth, Element element)> tellingTruthOrder = new();
    private enum Element { Thunder, Blizzard }

    // We only care about his truth & lies after FloodOfNaught - TODO remove this after timeline has been setup?
    public override void OnCastFinished(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.FloodOfNaught) {
            active = true;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID) {
        if (actor.OID == (uint)OID.KefkaP4 && active == true) {
            if (iconID == (uint)IconID.PurpleRingBlueOrb) {
                tellingTruthOrder.Add((true, Element.Thunder));
            }

            if (iconID == (uint)IconID.PurpleRingQuestionMark) {
                tellingTruthOrder.Add((false, Element.Thunder));
            }

            if (iconID == (uint)IconID.BlueRingBlueOrb) {
                tellingTruthOrder.Add((true, Element.Blizzard));
            }

            if (iconID == (uint)IconID.BlueRingQuestionMark) {
                tellingTruthOrder.Add((false, Element.Blizzard));
            }
        }
    }

    // TODO remove
    public override void Update() {
        //Service.Logger.Info($"tellingTruthOrder: {string.Join(", ", tellingTruthOrder.Select(e => $"{e.tellingTruth} {e.element}"))}");
    }
}

// TODO fix AOE middle line - small aoe line where you cant stand at all, but can be done later there is a lot of free space
class EdgeOfDeath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EdgeOfDeath, new AOEShapeRect(47.0f, 1.0f));

// TODO fix AOE middle line - small aoe line where you cant stand at all, but can be done later there is a lot of free space
// TODO change to simple AOEs at some point maybe - so it has built-in NumCasts
class Antilight(BossModule module) : BossComponent(module) {
    private Actor? whiteAntilight = null;
    private Actor? blackAntilight = null;
    private bool? tellingTruth = null;
    private GrandCrossOrder? grandCrossOrder = module.FindComponent<GrandCrossOrder>();
    public int NumCasts = 0;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.WhiteAntilight) {
            whiteAntilight = caster;
        }

        if (spell.Action.ID == (uint)AID.BlackAntilight) {
            blackAntilight = caster;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.WhiteAntilight) {
            whiteAntilight = null;
            NumCasts++;
        }

        if (spell.Action.ID == (uint)AID.BlackAntilight) {
            blackAntilight = null;
            NumCasts++;
        }

        if (spell.Action.ID == (uint)AID.FloodOfNaught || spell.Action.ID == (uint)AID.EdgeOfDeath) {
            NumCasts++;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        // Real
        if (status.ID == (uint)SID.TellingTruthBuff && status.Extra == 0x462) {
            tellingTruth = true;
        }

        // Fake
        if (status.ID == (uint)SID.TellingTruthBuff && status.Extra == 0x461) {
            tellingTruth = false;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (grandCrossOrder == null || whiteAntilight == null || blackAntilight == null || tellingTruth == null) {
            return;
        }

        var buffs = grandCrossOrder.getCurrentPlayerBuffs(pcSlot, 2); // sets are 0, 1, 2
        if (buffs == null) {
            return;
        }

        if (!hasBuff(buffs, SID.BeyondDeath) && !hasBuff(buffs, SID.AllaganField)) {
            return;
        }

        Actor correctSide;
        Actor wrongSide;

        if (hasBuff(buffs, SID.BeyondDeath)) {
            correctSide = hasBuff(buffs, SID.WhiteWound) ? whiteAntilight : blackAntilight;
            wrongSide = hasBuff(buffs, SID.WhiteWound) ? blackAntilight : whiteAntilight;
        } else {
            correctSide = hasBuff(buffs, SID.WhiteWound) ? blackAntilight : whiteAntilight;
            wrongSide = hasBuff(buffs, SID.WhiteWound) ? whiteAntilight : blackAntilight;
        }

        // If it was a lie, flip it
        if (tellingTruth == false) {
            (correctSide, wrongSide) = (wrongSide, correctSide);
        }

        var shape = new AOEShapeRect(47.0f, 10.5f);
        shape.Draw(Arena, correctSide.Position, correctSide.Rotation, Colors.SafeFromAOE);
        shape.Draw(Arena, wrongSide.Position, wrongSide.Rotation, Colors.AOE);
    }

    private bool hasBuff(IReadOnlyList<(SID buff, DateTime expireAt)> buffs, SID status) {
        return buffs.Any(b => b.buff == status);
    }
}

// 2x water & 2x Forked Lightning will go at the same time, these all share the same set
class ForkedWater(BossModule module) : Components.UniformStackSpread(module, 8.0f, 8.0f, 3, 3) {
    private GrandCrossOrder? grandCrossOrder = module.FindComponent<GrandCrossOrder>();
    public int NumCasts = 0; // 4 casts will always go off, better to watch this to know if the aoes are active or not

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.ForkedLightningFake || spell.Action.ID == (uint)AID.ForkedLightningReal ||
            spell.Action.ID == (uint)AID.CompressedWaterFake || spell.Action.ID == (uint)AID.CompressedWaterReal) {
            NumCasts++;
        }
    }

    public override void Update() {
        Stacks.Clear();
        Spreads.Clear();

        if (grandCrossOrder == null || NumCasts >= 4) {
            return;
        }

        var compressedPlayers = grandCrossOrder.getNextBuffPlayers(SID.CompressedWater, 2);
        foreach (var (slot, expireAt, tellingTruth) in compressedPlayers) {
            // Real
            if (tellingTruth == true) {
                var player = Raid[slot];
                if (player != null) {
                    AddStack(player, expireAt);
                }
            }

            // Fake
            if (tellingTruth == false) {
                var player = Raid[slot];
                if (player != null) {
                    AddSpread(player, expireAt);
                }
            }
        }

        var forkedPlayers = grandCrossOrder.getNextBuffPlayers(SID.ForkedLightning, 2);
        foreach (var (slot, expireAt, tellingTruth) in forkedPlayers) {
            // Real
            if (tellingTruth == true) {
                var player = Raid[slot];
                if (player != null) {
                    AddSpread(player, expireAt);
                }
            }

            // Fake
            if (tellingTruth == false) {
                var player = Raid[slot];
                if (player != null) {
                    AddStack(player, expireAt);
                }
            }
        }
    }
}

// 4x accleration bombs where two are from 1st set and the other 2 are from the 2nd set, so it can a mix of truth and lie
// TODO missing spell? or maybe it just doesn't have one
class AccelerationBomb(BossModule module) : Components.StayMove(module) {
    private GrandCrossOrder? grandCrossOrder = module.FindComponent<GrandCrossOrder>();

    public override void Update() {
        Array.Fill(PlayerStates, default);

        if (grandCrossOrder == null) {
            return;
        }

        foreach (var (slot, expireAt, tellingTruth) in grandCrossOrder.getNextBuffPlayers(SID.AccelerationBomb, 4)) {
            if ((expireAt - WorldState.CurrentTime).TotalSeconds > 7.0f) {
                continue;
            }

            if (tellingTruth == true) {
                PlayerStates[slot] = new(Requirement.Stay, expireAt);
            }

            if (tellingTruth == false) {
                PlayerStates[slot] = new(Requirement.Move, expireAt);
            }
        }
    }
}

// 2x CursedShriek from the same set always
class CursedShriek(BossModule module) : Components.GenericGaze(module) {
    private GrandCrossOrder? grandCrossOrder = module.FindComponent<GrandCrossOrder>();
    private List<(int slot, DateTime expireAt, bool inverted)> pendingGazes = new();
    private List<Eye> eyes = new();

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.CursedShriekReal || spell.Action.ID == (uint)AID.CursedShriekFake) {
            NumCasts++;
        }
    }

    public override ReadOnlySpan<Eye> ActiveEyes(int pcSlot, Actor pcActor) {
        eyes.Clear();

        foreach (var (slot, expireAt, inverted) in pendingGazes) {
            if (slot == pcSlot) {
                continue;
            }

            var actor = Raid[slot];
            if (actor == null) {
                continue;
            }

            eyes.Add(new Eye(actor.Position, expireAt, inverted: inverted));
        }

        return CollectionsMarshal.AsSpan(eyes);
    }

    public override void AddGlobalHints(GlobalHints hints) {
        if (eyes.Count == 0) {
            return;
        }

        if (eyes[0].Inverted == false) {
            hints.Add("Look away from gaze players");
        }

        if (eyes[0].Inverted == true) {
            hints.Add("Look at gaze players");
        }
    }

    public override void Update() {
        pendingGazes.Clear();

        if (grandCrossOrder == null || NumCasts >= 2) {
            return;
        }

        foreach (var (slot, expireAt, tellingTruth) in grandCrossOrder.getNextBuffPlayers(SID.CursedShriek, 2)) {
            if ((expireAt - WorldState.CurrentTime).TotalSeconds > 8.0) {
                continue;
            }

            pendingGazes.Add((slot, expireAt, tellingTruth == false));
        }
    }
}

class Inferno(BossModule module) : Components.GenericBaitProximity(module) {
    private TsunamiInfernoOrder? tsunamiInfernoOrder = module.FindComponent<TsunamiInfernoOrder>();

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.StrayFlamesP4) {
            CurrentBaits.Clear();
        }
    }

    public override void Update() {
        if (tsunamiInfernoOrder == null) {
            return;
        }

        var players = tsunamiInfernoOrder.getNextBuffPlayers(SID.EntropyP4, 8);
        if (players.Count == 0) {
            OnlyShowOutlines = false;
            return;
        }

        OnlyShowOutlines = true;
        CurrentBaits.Clear();

        foreach (var (slot, expireAt, tellingTruth) in players) {
            var player = Raid[slot];
            if (player == null) {
                continue;
            }

            // Real
            if (tellingTruth == true) {
                CurrentBaits.Add(new(player.Position, new AOEShapeCircle(6.0f)));
            }

            // Fake
            if (tellingTruth == false) {
                CurrentBaits.Add(new(player.Position, new AOEShapeDonut(3.0f, 6.0f))); // TODO aoe size is a guess for now
            }
        }
    }
}

class Tsunami(BossModule module) : Components.GenericBaitProximity(module) {
    private TsunamiInfernoOrder? tsunamiInfernoOrder = module.FindComponent<TsunamiInfernoOrder>();

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.StraySprayP4) {
            CurrentBaits.Clear();
        }
    }

    public override void Update() {
        if (tsunamiInfernoOrder == null) {
            return;
        }

        var players = tsunamiInfernoOrder.getNextBuffPlayers(SID.DynamicFluidP4, 8);
        if (players.Count == 0) {
            OnlyShowOutlines = false;
            return;
        }

        OnlyShowOutlines = true;
        CurrentBaits.Clear();

        foreach (var (slot, expireAt, tellingTruth) in players) {
            var player = Raid[slot];
            if (player == null) {
                continue;
            }

            if (tellingTruth == true) {
                CurrentBaits.Add(new(player.Position, new AOEShapeDonut(3.0f, 6.0f))); // TODO aoe size is a guess for now
            }

            // Fake
            if (tellingTruth == false) {
                CurrentBaits.Add(new(player.Position, new AOEShapeCircle(6.0f)));
            }
        }
    }
}

/*
    // TODO setup gaze activation
    // TODO setup timeline from P3 until slide 13
    // TODO ensure everything cleans up correctly and looks correct
    // TODO add hints for safe spots - these can be set
    // TODO setup remaining slides


    // 9 - 15 (fake) - Kefka
    // 12 - 15 (real) - Kefka
    // 13 - stores two for later? for a mystery magic ice and lightning (both real) - Kefka

    // So we use the two from slide 9 & 12
    // And then decide if we need to swap them base on mystery magic

    // real fake = fake
    // fake real = fake
    // real real = real
    // fake fake = real

    // TODO change the timers to actually be cast events instead, so if acclerationbomb just ended then we can start the gazes
    // TODO once setup above - ensure stacks & spreads are done correctly along with gzes

    // TODO figure what casts belong to what to allow clean up and correct size of aoes


    // TODO everything should be complete - clean up
    // TODO add safe spots to stand in

    // TODO fire appears to always be first
    // TODO water appears to always be second
 */
