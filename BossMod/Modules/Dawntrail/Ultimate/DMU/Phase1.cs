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

        Actor? target = secondTB ? RaidByEnmity(source, true).Skip(1).FirstOrDefault() : this.target;
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
}

class GravenImage(BossModule module) : Components.GenericKnockback(module, (uint)AID.PulseWave) {
    private List<(ulong SourceID, int slot)> tethers = [];
    private List<Knockback> knockbacks = [];

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
        knockbacks.Clear();

        foreach (var target in tethers) {
            if (target.slot != slot) {
                continue;
            }

            var source = WorldState.Actors.Find(target.SourceID);
            if (source == null) {
                continue;
            }

            knockbacks.Add(new(source.Position, 14f, actorID: source.InstanceID));
            return CollectionsMarshal.AsSpan(knockbacks);
        }

        return CollectionsMarshal.AsSpan(knockbacks);
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

class WaveCannon(BossModule module) : Components.BaitAwayEveryone(module,
    module.Enemies((uint)OID.StatueBodyOrb).FirstOrDefault(), new AOEShapeRect(100f, 3f),
    (uint)AID.WaveCannon) {

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.WaveCannon) {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
}

// TODO add priority system
class WaveCannonTowers(BossModule module) : Components.CastTowers(module, (uint)AID.TowerExplosion, 4) {
    private DateTime[] magicVulnerability = new DateTime[PartyState.MaxPartySize];

    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        if (status.ID == (uint)SID.MagicVulnerabilityUp) {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0) {
                magicVulnerability[slot] = status.ExpireAt;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        base.OnCastStarted(caster, spell);

        if (spell.Action.ID == WatchedAction) {
            for (int i = 0; i < Towers.Count; i++)
            {
                Towers.Ref(i).ForbiddenSoakers = Raid.WithSlot()
                    .WhereSlot(player => magicVulnerability[player] > Towers[i].Activation).Mask();
            }
        }
    }
}

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

class DoubleTroubleTrapStacks(BossModule module) : Components.UniformStackSpread(module, 6f, 5f, 4, 4) {
    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        if (status.ID == (uint)SID.DoubleTroubleTrap) {
            AddStack(actor, status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status) {
        if (status.ID == (uint)SID.DoubleTroubleTrap) {
            Stacks.RemoveAt(0);
        }
    }
}

class DoubleTroubleTrapKnockback(BossModule module) : Components.GenericKnockback(module, (uint)AID.DoubleTroubleTrap1) {
    private List<Knockback> knockbacks = [];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) {
        knockbacks.Clear();

        var stack = Module.FindComponent<DoubleTroubleTrapStacks>();
        if (stack == null) {
            return CollectionsMarshal.AsSpan(knockbacks);
        }

        foreach (var stackPoint in stack.Stacks) {
            if (actor.Position.InCircle(stackPoint.Target.Position, stackPoint.Radius)) {
                knockbacks.Add(new(stackPoint.Target.Position, 14f, stackPoint.Activation, actorID: stackPoint.Target.InstanceID));
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

// TODO party config settings for display of green circles for stacks & spreads
class Gravitas(BossModule module) : Components.UniformStackSpread(module, 5, 5, 4, 4) {
    private List<Spread> spreadsIncoming = [];
    private int totalStacks = 0;
    public int NumCasts = 0;

    private enum TetherGroup { Support, DPS }
    private TetherGroup tetherGroup = TetherGroup.Support;

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        base.DrawArenaForeground(pcSlot, pc);

        if (Stacks.Count != 0 && totalStacks == 4) {
            Arena.AddCircle(Module.Center + new WDir(0, 19.5f), 1.0f, Colors.Safe, 2);
            Arena.AddCircle(Module.Center + new WDir(0, -19.5f), 1.0f, Colors.Safe, 2);
        }

        if (Stacks.Count != 0 && totalStacks == 8) {
            var puddles = Module.FindComponent<GravitasPuddles>();

            if (puddles == null) {
                return;
            }

            var puddleSources = puddles.Sources(Module).ToList();

            var puddle1 = puddleSources.Where(p => p.Position.Z > Module.Center.Z).MinBy(p => (p.Position - Module.Center).Length());
            if (puddle1 == null) {
                return;
            }

            var puddle2 = puddleSources.Where(p => p.Position.Z < Module.Center.Z).MinBy(p => (p.Position - Module.Center).Length());
            if (puddle2 == null) {
                return;
            }

            Arena.AddCircle(puddle1.Position - (puddle1.Position - Module.Center) * 5.5f, 1.0f, Colors.Safe, 2);
            Arena.AddCircle(puddle2.Position - (puddle2.Position - Module.Center) * 5.5f, 1.0f, Colors.Safe, 2);
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
            } else {
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
        if (spell.Action.ID == (uint)AID.Gravitas && Stacks.Count > 0) {
            Stacks.RemoveAt(0);
            NumCasts++;
            Spreads.AddRange(spreadsIncoming);
            spreadsIncoming.Clear();
        }

        if (spell.Action.ID == (uint)AID.Vitrophyre && Spreads.Count > 0) {
            Spreads.RemoveAt(0);
        }
    }
}

// TODO voidzone don't check for event cast sadly, maybe add it?
// TODO needs to be cleaned up - change the colour during a specific point in time and maybe just count the number of times the spell has occured instead
class GravitasPuddles(BossModule module) : Components.PersistentInvertibleVoidzoneByCast(module, 5,
    m => m.Enemies((uint)OID.PurplePuddles).Where(e => e.EventState != 7), (uint)AID.Gravitas)  {

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

class GravitationalWave(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];

    public override void OnActorEAnim(Actor actor, uint state) {
        if (actor.OID == 0x1EBFBD && state == (uint)Animations.PulseOrbStart) {
            aoes.Add(new AOEInstance(new AOEShapeRect(40, 20), Arena.Center, 90.Degrees(), actorID: actor.InstanceID));
        }

        if (actor.OID == 0x1EBFBC && state == (uint)Animations.PulseOrbStart) {
            aoes.Add(new AOEInstance(new AOEShapeRect(40, 20), Arena.Center, -90.Degrees(), actorID: actor.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.GravitationalWave || spell.Action.ID == (uint)AID.IntemperateWill) {
            NumCasts++;
            aoes.Clear();
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        return CollectionsMarshal.AsSpan(aoes);
    }
}

/*
A1 MARK:
A - [93.696, 93.692]
1RIGHT - [94.219, 87.589]
ALEFT - [87.173, 93.588]
1 - [87.347, 87.392]

B2 MARK:
B - [106.246, 93.555]
BRIGHT - [112.405, 93.744]
2 - [112.704, 87.231]
2LEFT - [106.611, 87.215]

C3 MARK:
C - [106.397, 106.515]
3LEFT - [106.261, 112.634]
3 - [112.631, 112.776]
CRIGHT - [112.787, 106.504]

D4 MARK:
D - [93.761, 106.631]
DLEFT - [87.566, 106.333]
4 - [87.219, 112.880]
4RIGHT - [93.390, 112.900]
*/

// TODO make it so after the first one has gone off it removes the circle and leaves a puddle behind
// TODO clean up this is a bit messy and can most likely be easier to setup
// TODO Allow different configures
// TODO remove slotList
class TeleTrouncing(BossModule module) : BossComponent(module) {
    public int NumCasts = 0;

    private uint[] TeleSIDs = [(uint)SID.TelePortentRIGHT, (uint)SID.TelePortentUP, (uint)SID.TelePortentDOWN, (uint)SID.TelePortentLEFT,
        (uint)SID.TelePortentRIGHT2, (uint)SID.TelePortentUP2, (uint)SID.TelePortentDOWN2, (uint)SID.TelePortentLEFT2];

    private bool IsTelePortent(uint sid) => Array.IndexOf(TeleSIDs, sid) >= 0;
    private bool IsDown(uint sid) => sid is (uint)SID.TelePortentDOWN or (uint)SID.TelePortentDOWN2;
    private bool IsLeft(uint sid) => sid is (uint)SID.TelePortentLEFT or (uint)SID.TelePortentLEFT2;
    private bool IsUp(uint sid) => sid is (uint)SID.TelePortentUP or (uint)SID.TelePortentUP2;
    private bool IsRight(uint sid) => sid is (uint)SID.TelePortentRIGHT or (uint)SID.TelePortentRIGHT2;

    public override void OnStatusGain(Actor target, ref ActorStatus status) {
        if (!IsTelePortent(status.ID)) {
            return;
        }

        var slot = Raid.FindSlot(target.InstanceID);
        if ((uint)slot >= PartyState.MaxPartySize) {
            return;
        }

        GetOrCreateSlotList(slot).Add((status.ID, status.ExpireAt));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (NumCasts == 16) {
            return;
        }

        if (!activeBySlot.TryGetValue(pcSlot, out var sids) || sids.Count < 2) {
            return;
        }

        var doubleDebuff = sids[0].SID == sids[1].SID;
        if (doubleDebuff) {
            if (IsDown(sids[0].SID)) {
                Arena.AddCircle(new WPos(87.173f, 93.588f), 1.0f, Colors.Safe, 2);
                Arena.AddCircle(new WPos(87.347f, 87.392f), 1.0f, Colors.AOE, 2);
            }

            if (IsLeft(sids[0].SID)) {
                Arena.AddCircle(new WPos(112.704f, 87.231f), 1.0f, Colors.Safe, 2);
                Arena.AddCircle(new WPos(106.611f, 87.215f), 1.0f, Colors.AOE, 2);
            }

            if (IsUp(sids[0].SID)) {
                Arena.AddCircle(new WPos(112.631f, 112.776f), 1.0f, Colors.Safe, 2);
                Arena.AddCircle(new WPos(112.787f, 106.504f), 1.0f, Colors.AOE, 2);
            }

            if (IsRight(sids[0].SID)) {
                Arena.AddCircle(new WPos(87.219f, 112.880f), 1.0f, Colors.Safe, 2);
                Arena.AddCircle(new WPos(93.390f, 112.900f), 1.0f, Colors.AOE, 2);
            }

            return;
        }

        if (DrawMixedCase(sids, IsUp, new WPos(93.696f, 93.692f), IsLeft, new WPos(94.219f, 87.589f))) {
            return;
        }

        if (DrawMixedCase(sids, IsRight, new WPos(106.246f, 93.555f), IsUp, new WPos(112.405f, 93.744f))) {
            return;
        }

        if (DrawMixedCase(sids, IsDown, new WPos(106.397f, 106.515f), IsRight, new WPos(106.261f, 112.634f))) {
            return;
        }

        if (DrawMixedCase(sids, IsDown, new WPos(87.566f, 106.333f), IsLeft, new WPos(93.761f, 106.631f))) {
            return;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.TeleTrouncing1) {
            NumCasts++;
        }
    }

    // TODO this can be removed once I have confirmed everything is working, just to check other player's hints
    private Dictionary<int, List<(uint SID, DateTime ExpireAt)>> activeBySlot = [];

    // TODO can be removed like above
    private List<(uint SID, DateTime ExpireAt)> GetOrCreateSlotList(int slot) {
        if (!activeBySlot.TryGetValue(slot, out var list)) {
            activeBySlot[slot] = list = [];
        }
        return list;
    }

    // TODO most likely just move this logic into the Draw function
    private bool DrawMixedCase(List<(uint SID, DateTime ExpireAt)> sids, Func<uint, bool> dir1Match, WPos point1, Func<uint, bool> dir2Match, WPos point2) {
        if (!TryFindFirst(sids, dir1Match, out var d1) || !TryFindFirst(sids, dir2Match, out var d2)) {
            return false;
        }

        var d1First = d1.ExpireAt <= d2.ExpireAt;
        if (d1First) {
            Arena.AddCircle(point1, 1.0f, Colors.Safe, 2);
            Arena.AddCircle(point2, 1.0f, Colors.AOE, 2);
        } else {
            Arena.AddCircle(point2, 1.0f, Colors.Safe, 2);
            Arena.AddCircle(point1, 1.0f, Colors.AOE, 2);
        }

        return true;
    }

    private bool TryFindFirst(List<(uint SID, DateTime ExpireAt)> sids, Func<uint, bool> match, out (uint SID, DateTime ExpireAt) entry) {
        for (var i = 0; i < sids.Count; ++i) {
            if (match(sids[i].SID)) {
                entry = sids[i];
                return true;
            }
        }

        entry = default;
        return false;
    }

}

// TODO most likely will implement this at some point, but currently the RP uses static spots, so we don't care about the tethers at all
// TODO add actual spots base on role config
// TODO does 48370 - GravenImage to apply tethers
// TODO LEFT tether is 1EBFBF
// TODO RIGHT tether is 1EBFBE
// TODO add the spreads from sleep to show the radius of it
// TODO the tether AOE might be the wrong one - all of the RP changed - but it will still work fine, but should be fixed
// TODO since all the same role type get the same tether type, you can just add the spread to one role type to show it

/*
Marker middle A - [89.393, 89.193]
Hitbox of A - [96.019, 96.796]

Marker middle B - [110.024, 89.869]
Hitbox of B - [103.378, 95.483]

Marker middle C - [109.500, 109.772]
Hitbox of C - [104.167, 103.239]

Markers middle D - [89.765, 110.050]
Hitbox of D - [96.912, 104.158]
 */
class GravenImage2(BossModule module) : Components.UniformStackSpread(module, 5, 5, 1, 1) {
    public override void OnTethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID != (uint)TetherID.GravenImageTether) {
            return;
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null) {
            return;
        }


        if (source.Position.AlmostEqual(new(126.000f, 41.500f), 5)) {
            AddSpread(target, WorldState.FutureTime(6.5f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID._Ability_IdyllicWill) {
            Spreads.Clear();
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (pc.Class.GetRole() == Role.Tank) {
            Arena.AddCircle(new WPos(96.019f, 89.193f), 1.0f, Colors.Safe, 2);
            Arena.AddCircle(new WPos(103.378f, 95.483f), 1.0f, Colors.Safe, 2);
        }

        if (pc.Class.GetRole() == Role.Healer) {
            Arena.AddCircle(new WPos(109.500f, 109.772f), 1.0f, Colors.Safe, 2);
            Arena.AddCircle(new WPos(89.765f, 110.050f), 1.0f, Colors.Safe, 2);
        }

        if (pc.Class.GetRole() == Role.Melee) {
            Arena.AddCircle(new WPos(104.167f, 103.239f), 1.0f, Colors.Safe, 2);
            Arena.AddCircle(new WPos(96.912f, 104.158f), 1.0f, Colors.Safe, 2);
        }

        if (pc.Class.GetRole() == Role.Ranged) {
            Arena.AddCircle(new WPos(89.393f, 89.193f), 1.0f, Colors.Safe, 2);
            Arena.AddCircle(new WPos(110.024f, 89.869f), 1.0f, Colors.Safe, 2);
        }
    }
}
