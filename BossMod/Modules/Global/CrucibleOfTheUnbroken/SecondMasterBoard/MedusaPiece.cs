namespace BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.MedusaPiece;

public enum OID : uint {
    MedusaPiece = 0x4CF9,
    Helper = 0x233C,
    LamiaPiece = 0x4CFA, // R1.400, x0 (spawn during fight)
    CyclopsPiece = 0x4CFB, // R2.800, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 50935, // MedusaPiece->player, no cast, single-target
    Summon = 49283, // MedusaPiece->self, 3.0s cast, single-target
    PetrifyingRegardBoss = 49277, // MedusaPiece->self, 3.0s cast, single-target
    PetrifyingRegard = 49278, // Helper->self, 3.0s cast, range 60 45.000-degree cone
    Impassion = 49298, // MedusaPiece->self, 8.0s cast, single-target
    ConstrictingEvisceration = 49292, // MedusaPiece->self, 6.5+0.5s cast, single-target
    Shockwave = 49296, // Helper->self, 7.0s cast, range 60 ?-degree cone
    RingingBlade = 49293, // Helper->self, 7.0s cast, range 5-60 donut
    CirclingBladeBoss = 49294, // MedusaPiece->self, no cast, single-target
    CirclingBlade = 49295, // Helper->self, 9.0s cast, range 6 circle
    PetrifyingPassionBoss = 49279, // MedusaPiece->self, 3.0s cast, single-target
    PetrifyingPassionLock = 49280, // MedusaPiece->self, no cast, single-target
    PetrifyingPassion = 49281, // Helper->self, 3.0s cast, range 60 45.000-degree cone
    PetrifyingPassion2 = 49282, // Helper->self, no cast, range 60 ?-degree cone

    // LamiaPiece
    AutoAttackFire = 48622, // 4CFA->player, no cast, single-target
    Raise = 49287, // 4CFA->4CFA, 10.0s cast, single-target

    // CyclopsPiece
    AutoAttackCyclopsPiece = 50937, // 4CFB->player, no cast, single-target
    Glower = 49370, // 4CFB->self, 4.0s cast, range 40 width 3 rect
    TonzeSwing1000 = 49297, // 4CFB->self, 10.0s cast, range 20 circle
}

public enum SID : uint {
    DamageUpLamiaPiece = 2550, // Helper->4CFA, extra=0x1
    StoneCurse = 437, // Helper->4CFB, extra=0x0
    DamageUp = 3129, // MedusaPiece->MedusaPiece, extra=0x0
    VulnerabilityUp = 1845, // MedusaPiece->MedusaPiece, extra=0x0
    Unknown = 2056, // MedusaPiece->MedusaPiece, extra=0x1D
}

public enum IconID : uint {
    PetrifyingRegardLockOn = 23, // player->self
    PetrifyingPassionLockOn = 244, // player->self
    TurnLeft = 236, // MedusaPiece->self
    TurnRight = 235, // MedusaPiece->self
}

public enum TetherID : uint {
    PetrifyingTether = 1, // MedusaPiece->player
}

sealed class Glower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Glower, new AOEShapeRect(40.0f, 1.5f));
sealed class Raise(BossModule module) : Components.CastInterruptHint(module, (uint)AID.Raise, showNameInHint: true);

sealed class PetrifyingRegardBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60.0f, 22.5f.Degrees()),
    (uint)IconID.PetrifyingRegardLockOn, activationDelay: 5.8f) {
    private IEnumerable<Actor> lamiaEnemies => Module.Enemies((uint)OID.LamiaPiece).Where(actor => !actor.IsDead);
    private IEnumerable<Actor> cyclopsEnemies => Module.Enemies((uint)OID.CyclopsPiece).Where(actor => !actor.IsDead);

    public override void OnEventCast(Actor caster, ActorCastEvent spell) { }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.PetrifyingRegard) {
            if (CurrentBaits.Count > 0) {
                CurrentBaits.RemoveAt(0);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        base.DrawArenaForeground(pcSlot, pc);

        if (!IsBaitTarget(pc) || CurrentBaits.Count == 0) {
            return;
        }

        var bait = CurrentBaits[0];
        var source = bait.Source.Position;
        var direction = Angle.FromDirection(bait.Target.Position - source);
        var baitAngle = ((AOEShapeCone)bait.Shape).HalfAngle;

        foreach (var enemy in lamiaEnemies) {
            var angle = baitAngle + Angle.Asin(enemy.HitboxRadius / (enemy.Position - source).Length());
            var onHitBox = new AOEShapeCone(60.0f, angle).Check(enemy.Position, source, direction);
            Arena.ZoneCircleOutline(enemy.Position, enemy.HitboxRadius, onHitBox ? Colors.Danger : Colors.Border);
        }

        foreach (var enemy in cyclopsEnemies) {
            var angle = baitAngle + Angle.Asin(enemy.HitboxRadius / (enemy.Position - source).Length());
            var onHitBox = new AOEShapeCone(60.0f, angle).Check(enemy.Position, source, direction);
            Arena.ZoneCircleOutline(enemy.Position, enemy.HitboxRadius, onHitBox ? Colors.Safe : Colors.Border);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);

        if (!IsBaitTarget(actor) || CurrentBaits.Count == 0) {
            return;
        }

        var bait = CurrentBaits[0];
        var source = bait.Source.Position;
        var baitAngle = ((AOEShapeCone)bait.Shape).HalfAngle;

        foreach (var enemy in lamiaEnemies) {
            var direction = Angle.FromDirection(enemy.Position - source);
            var angle = baitAngle + Angle.Asin(enemy.HitboxRadius / (enemy.Position - source).Length());
            hints.AddForbiddenZone(new SDCone(source, 60.0f, direction, angle), bait.Activation);
        }

        Actor? closest = null;
        var bestDistance = float.MaxValue;
        foreach (var enemy in cyclopsEnemies) {
            var distance = (enemy.Position - source).LengthSq();
            if (distance < bestDistance) {
                bestDistance = distance;
                closest = enemy;
            }
        }

        if (closest == null) {
            return;
        }

        var angleGoal = baitAngle + Angle.Asin(closest.HitboxRadius / (closest.Position - source).Length());
        hints.GoalZones.Add(AIHints.GoalSingleTarget(Module.PrimaryActor, 2.0f));
        hints.GoalZones.Add(p => new AOEShapeCone(60.0f, angleGoal).Check(closest.Position, source, Angle.FromDirection(p - source)) ? 5.0f : 0.0f);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        if (!IsBaitTarget(actor) || CurrentBaits.Count == 0) {
            return;
        }

        hints.Add("Avoid hitting LamiaPieces with the bait, while hitting the CyclopsPieces");
    }
}
sealed class PetrifyingRegard(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PetrifyingRegard, new AOEShapeCone(60.0f, 22.5f.Degrees()));

sealed class Shockwave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shockwave, new AOEShapeCone(60.0f, 22.5f.Degrees()), 4);

sealed class RingCircleBlade(BossModule module) : Components.GenericAOEs(module) {
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeDonut ringingBladeShape = new(5.0f, 60.0f);
    private readonly AOEShapeCircle circlingBladeShape = new(6.0f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.RingingBlade) {
            aoes.Add(new(ringingBladeShape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            SortHelpers.SortAOEByActivation(aoes);
        }

        if (spell.Action.ID == (uint)AID.CirclingBlade) {
            aoes.Add(new(circlingBladeShape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            SortHelpers.SortAOEByActivation(aoes);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID is (uint)AID.RingingBlade or (uint)AID.CirclingBlade) {
            if (aoes.Count > 0) {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        var count = aoes.Count;
        if (count == 0) {
            return [];
        }

        var incomingAOEs = CollectionsMarshal.AsSpan(aoes);
        var max = count > 1 ? 1 : count;
        return incomingAOEs[..max];
    }
}

sealed class PetrifyingPassionBait(BossModule module) : Components.GenericBaitAway(module) {
    private Angle increment = default;
    private Actor? target;
    private readonly double ActivationDelay = 5.8f;
    private readonly AOEShapeCone shape = new(60.0f, 22.5f.Degrees());
    private readonly AOEShapeCone fullCone = new(60.0f, 82.5f.Degrees()); // Used as the full cone bait put together for AIHints
    private bool baitsLocked = false; // Used as baits will lock, but the boss won't start casting for roughly 1.0 seconds

    private IEnumerable<Actor> cyclopsEnemies => Module.Enemies((uint)OID.CyclopsPiece).Where(actor => !actor.IsDead);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID) {
        if (iconID == (uint)IconID.PetrifyingPassionLockOn) {
            var player = WorldState.Actors.Find(targetID);
            if (player == null) {
                return;
            }

            target = player;
            InitIfReady();
            return;
        }

        increment = iconID switch {
            (uint)IconID.TurnLeft => 30.0f.Degrees(),
            (uint)IconID.TurnRight => -30.0f.Degrees(),
            _ => default
        };

        InitIfReady();
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4) {
        if (updateID == 9 && param1 == 2 && param2 == 1664) {
            if (CurrentBaits.Count > 0) {
                // Lock all baits positions
                var baits = CollectionsMarshal.AsSpan(CurrentBaits);
                for (var i = 0; i < baits.Length; i++) {
                    ref var bait = ref baits[i];
                    bait.CustomRotation ??= Angle.FromDirection(bait.Target.Position - Module.PrimaryActor.Position);
                }

                baitsLocked = true;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.PetrifyingPassion) {
            CurrentBaits.Clear();
            baitsLocked = false;
            increment = default;
            target = null;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc) {
        if (!baitsLocked) {
            base.DrawArenaForeground(pcSlot, pc);
            return;
        }

        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        for (var i = 0; i < baits.Length; i++) {
            shape.Draw(Arena, Module.PrimaryActor.Position, baits[i].Rotation, i == 0 ? Colors.Danger : Colors.AOE);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (!baitsLocked) {
            base.DrawArenaForeground(pcSlot, pc);
        }
    }

    public override void Update() {
        if (CurrentBaits.Count == 0) {
            return;
        }

        var count = CurrentBaits.Count - 1;
        for (var i = count; i >= 0; --i) {
            ref var b = ref CurrentBaits.Ref(i);
            if (b.Target.IsDead) {
                CurrentBaits.RemoveAt(i);
            }
        }

        if (baitsLocked) {
            return;
        }

        for (var i = 0; i < CurrentBaits.Count; i++) {
            if (i == 0) {
                continue;
            }

            var bait = CurrentBaits[i];
            bait.CustomRotation = Angle.FromDirection(bait.Target.Position - Module.PrimaryActor.Position) + increment * i;
            CurrentBaits[i] = bait;
        }
    }

    // Handles hitting as many CyclopsPiece as possible
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        if (baitsLocked || CurrentBaits.Count == 0 || target == null || increment == default) {
            return;
        }

        var bestAngle = default(Angle);
        var bestCount = 0;
        for (var degree = 0.0f; degree < 360.0f; degree = degree + 10.0f) {
            var angle = degree.Degrees();
            var count = 0;
            foreach (var enemy in cyclopsEnemies) {
                if (fullCone.Check(enemy.Position, Module.PrimaryActor.Position, angle)) {
                    count++;
                }
            }

            if (count > bestCount) {
                bestCount = count;
                bestAngle = angle;
            }
        }

        if (bestCount == 0) {
            return;
        }

        hints.GoalZones.Add(AIHints.GoalProximity(Module.PrimaryActor.Position + 3.0f * (bestAngle - increment * 2).ToDirection(), 3.0f, 5.0f));
    }

    private void InitIfReady() {
        if (target == null || increment == default) {
            return;
        }

        var direction = Angle.FromDirection(target.Position - Module.PrimaryActor.Position);
        CurrentBaits.Add(new(Module.PrimaryActor, target, shape, WorldState.FutureTime(ActivationDelay)));
        for (var i = 1; i <= 4; i++) {
            CurrentBaits.Add(new(Module.PrimaryActor, target, shape, WorldState.FutureTime(ActivationDelay), customRotation: direction + increment * i));
        }
    }
}

sealed class PetrifyingPassion(BossModule module) : Components.GenericRotatingAOE(module) {
    private ActorCastInfo? spellInfo;
    private Angle increment = default;
    private readonly AOEShapeCone shape = new(60.0f, 22.5f.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID) {
        increment = iconID switch {
            (uint)IconID.TurnLeft => 30.0f.Degrees(),
            (uint)IconID.TurnRight => -30.0f.Degrees(),
            _ => default
        };

        InitIfReady();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.PetrifyingPassion) {
            spellInfo = spell;
            InitIfReady();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.PetrifyingPassion or (uint)AID.PetrifyingPassion2) {
            if (Sequences.Count > 0) {
                AdvanceSequence(0, WorldState.CurrentTime);
            }
        }
    }

    private void InitIfReady() {
        if (spellInfo != null && increment != default) {
            Sequences.Add(new(shape, spellInfo.LocXZ, spellInfo.Rotation, increment, Module.CastFinishAt(spellInfo), 1.7d, 5, 5));
            spellInfo = null;
            increment = default;
        }
    }
}

sealed class TonzeSwing1000(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TonzeSwing1000, 20.0f) {
    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        if (status.ID == (uint)SID.StoneCurse) {
            var count = Casters.Count;
            var id = actor.InstanceID;
            var aoes = CollectionsMarshal.AsSpan(Casters);
            for (var i = 0; i < count; ++i) {
                if (aoes[i].ActorID == id) {
                    Casters.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        if (Casters.Count >= 4) {
            return;
        }

        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        if (Casters.Count >= 4) {
            return;
        }

        base.AddHints(slot, actor, hints);
    }
}

sealed class MedusaPieceStates : StateMachineBuilder {
    public MedusaPieceStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<Glower>()
            .ActivateOnEnter<Raise>()
            .ActivateOnEnter<PetrifyingRegardBait>()
            .ActivateOnEnter<PetrifyingRegard>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<RingCircleBlade>()
            .ActivateOnEnter<PetrifyingPassionBait>()
            .ActivateOnEnter<PetrifyingPassion>()
            .ActivateOnEnter<TonzeSwing1000>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, PrimaryActorOID = (uint)OID.MedusaPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1092u, NameID = 14660u, SortOrder = 7)]
public sealed class MedusaPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, 0f), new ArenaBoundsRect(20f, 20f)) {
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i) {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch {
                (uint)OID.LamiaPiece => 3,
                (uint)OID.CyclopsPiece => e.Actor.FindStatus((uint)SID.StoneCurse) != null ? 6 : 1,
                (uint)OID.MedusaPiece => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc) {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.LamiaPiece), Colors.Vulnerable);
        Arena.Actors(Enemies((uint)OID.CyclopsPiece));
    }

    private readonly string[] _prePullHints = [
        "Baited petrify cone, will damage buff lamias and petrify cyclops on hit. Can also cast rotating versions. YOU WILL INSTANTLY DIE IF YOU GET PETRIFIED BY THIS",
        "Lamia: Will attempt to revive any dead lamias, interrupt this cast.",
        "Cyclops: Casts PB aoes covering the entire arena when spawned, petrify them to stop it. These will also instantly die upon taking any damage while petrified"
    ];

    public override string[] PrePullHints => _prePullHints;
}
