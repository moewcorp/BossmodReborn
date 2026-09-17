namespace BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.GigantisPiece;

public enum OID : uint {
    GigantisPiece = 0x4D03,
    Helper = 0x233C,
    CyclopsPiece = 0x4D04, // R2.000, x0 (spawn during fight)
    CongealedLightning = 0x4D05, // R2.500-5.000, x0 (spawn during fight)
    CongealedLightningSmall = 0x4D06, // R0.500, x0 (spawn during fight)
    CongealedKindling = 0x4D07, // R2.500, x0 (spawn during fight)
    CongealedKindlingSmall = 0x4D08, // R0.500, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 50786, // GigantisPiece->player, no cast, single-target
    GiganticRageCircleBoss = 49358, // GigantisPiece->self, 5.0+1.0s cast, single-target
    GiganticRageCircle = 49359, // Helper->self, 6.0s cast, range 15 circle
    GiganticRageConeBoss = 49360, // GigantisPiece->self, 5.0+1.0s cast, single-target
    GiganticRageCone = 49361, // Helper->self, 6.0s cast, range 40 180.000-degree cone
    GiganticRageConeBoss1 = 49362, // GigantisPiece->self, 5.0+1.0s cast, single-target
    GiganticRageCone1 = 49363, // Helper->self, 6.0s cast, range 40 180.000-degree cone
    AftersparkTower = 49364, // Helper->location, 3.0s cast, range 2 circle
    Afterspark = 49365, // Helper->self, no cast, range 60 circle - happens if you miss the tower
    AfterburnAOE = 49366, // Helper->self, 3.0s cast, range 6 circle
    SmashingStampBoss = 49377, // GigantisPiece->self, 2.6+0.4s cast, single-target
    SmashingStamp = 49378, // Helper->self, 3.0s cast, range 40 width 8 rect
    SmashingStampBait = 49379, // Helper->self, 3.0s cast, range 7 width 4 rect

    // CyclopsPiece
    AutoAttackCyclopsPiece = 50398, // 4D04->player, no cast, single-target
    Glower = 49367, // 4D04->self, 4.0s cast, range 40 width 3 rect
    Camaraderie = 49368, // CyclopsPiece->GigantisPiece, 8.0s cast, single-target

    // Slimes
    RuptureFire = 49375, // 4D07->self, 7.0s cast, range 40 circle
    RuptureLightning = 49373, // 4D05->self, 7.0s cast, range 40 circle
    AutoAttackThunder = 48623, // 4D05/4D06->player, no cast, single-target
    AutoAttackFire = 48622, // 4D07/4D08->player, no cast, single-target
}

public enum SID : uint {
    WeaponElement = 2056, // none->GigantisPiece, extra=0x499/0x49A - 0x499 - Lightning, 0x49A - Fire
    DamageUp = 2550, // none->4D05, extra=0x1
}

public enum IconID : uint {
    SmashingStampIcon = 234, // player->self
}

sealed class Glower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Glower, new AOEShapeRect(40.0f, 3.0f));

// Handles the bait and weapon's element
sealed class SmashingStampBait(BossModule module) : Components.GenericBaitProximity(module) {
    private readonly List<Actor> slimes = [];
    private readonly AOEShapeRect shape = new(7.0f, 2.0f);
    private bool active = false;
    public bool weaponRecentlyChanged = false;

    public enum Element { NONE, LIGHTNING, FIRE }
    public Element weaponElement = Element.NONE;

    public override void OnActorCreated(Actor actor) {
        if (actor.OID is (uint)OID.CongealedLightning or (uint)OID.CongealedKindling) {
            slimes.Add(actor);
            weaponRecentlyChanged = false;
        }
    }

    public override void OnActorDestroyed(Actor actor) {
        if (actor.OID is (uint)OID.CongealedLightning or (uint)OID.CongealedKindling) {
            slimes.Remove(actor);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        if (status.ID == (uint)SID.WeaponElement && status.Extra == 0x499) {
            weaponElement = Element.LIGHTNING;
            weaponRecentlyChanged = true;
        }

        if (status.ID == (uint)SID.WeaponElement && status.Extra == 0x49A) {
            weaponElement = Element.FIRE;
            weaponRecentlyChanged = true;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID) {
        if (iconID == (uint)IconID.SmashingStampIcon) {
            active = true;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.SmashingStampBait) {
            active = false;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        base.AddHints(slot, actor, hints);

        if (CurrentBaits.Count == 0 || slimes.Count == 0) {
            return;
        }

        switch (weaponElement) {
            case Element.NONE:
                hints.Add("Hit any slime with the bait!", false);
                break;
            case Element.LIGHTNING:
                hints.Add("Hit the orange slime with the bait!", false);
                break;
            case Element.FIRE:
                hints.Add("Hint the purple slime with the bait!", false);
                break;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        base.DrawArenaForeground(pcSlot, pc);

        if (CurrentBaits.Count == 0 || slimes.Count == 0) {
            return;
        }

        switch (weaponElement) {
            case Element.NONE:
                foreach (var slime in slimes) {
                    Arena.ZoneCircleOutline(slime.Position, 1.0f, Colors.Safe);
                }
                break;
            case Element.LIGHTNING:
                foreach (var slime in slimes) {
                    if (slime.OID == (uint)OID.CongealedKindling) {
                        Arena.ZoneCircleOutline(slime.Position, 1.0f, Colors.Safe);
                    }
                }
                break;
            case Element.FIRE:
                foreach (var slime in slimes) {
                    if (slime.OID == (uint)OID.CongealedLightning) {
                        Arena.ZoneCircleOutline(slime.Position, 1.0f, Colors.Safe);
                    }
                }
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);

        if (CurrentBaits.Count == 0 || slimes.Count == 0) {
            return;
        }

        var currentBait = CurrentBaits[0];
        if (!IsBaitTarget(ref currentBait, actor)) {
            return;
        }

        switch (weaponElement) {
            case Element.NONE:
                foreach (var slime in slimes) {
                    hints.AddForbiddenZone(new SDInvertedCircle(slime.Position, 1.0f));
                }
                break;
            case Element.LIGHTNING:
                foreach (var slime in slimes) {
                    if (slime.OID == (uint)OID.CongealedKindling) {
                        hints.AddForbiddenZone(new SDInvertedCircle(slime.Position, 1.0f));
                    }
                }
                break;
            case Element.FIRE:
                foreach (var slime in slimes) {
                    if (slime.OID == (uint)OID.CongealedLightning) {
                        hints.AddForbiddenZone(new SDInvertedCircle(slime.Position, 1.0f));
                    }
                }
                break;
        }
    }

    public override void Update() {
        base.Update();

        CurrentBaits.Clear();

        if (!active) {
            return;
        }

        CurrentBaits.Add(new(Module.PrimaryActor, shape));
    }
}
sealed class SmashingStamp(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SmashingStamp, new AOEShapeRect(40.0f, 4.0f));
sealed class SmashingStampBaitAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SmashingStamp, new AOEShapeRect(7.0f, 2.0f));

sealed class GiganticRageCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GiganticRageCircle, 15.0f);
sealed class GiganticRageCone(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.GiganticRageCone, (uint)AID.GiganticRageCone1],
    new AOEShapeCone(40.0f, 90.0f.Degrees()));
sealed class AftersparkTower(BossModule module) : Components.CastTowers(module, (uint)AID.AftersparkTower, 2.0f);
sealed class AfterburnAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AfterburnAOE, 6.0f);
sealed class GiganticRageType(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true, onlyShowOutlines: true) {
    private readonly SmashingStampBait? smashingStampBait = module.FindComponent<SmashingStampBait>();
    private readonly AOEShapeDonut donut = new(2.0f, 40.0f);
    private readonly AOEShapeCircle circle = new(6.0f);
    private bool active = false;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID is (uint)AID.GiganticRageCircle or (uint)AID.GiganticRageCone or (uint)AID.GiganticRageCone1) {
            active = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID is (uint)AID.GiganticRageCircle or (uint)AID.GiganticRageCone or (uint)AID.GiganticRageCone1) {
            active = false;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        if (smashingStampBait == null || smashingStampBait.weaponElement == SmashingStampBait.Element.NONE || !active) {
            return;
        }

        if (smashingStampBait.weaponElement == SmashingStampBait.Element.LIGHTNING) {
            hints.Add("You will drop a tower!", false);
        }

        if (smashingStampBait.weaponElement == SmashingStampBait.Element.FIRE) {
            hints.Add("You will drop a puddle!", false);
        }
    }

    public override void Update() {
        base.Update();

        CurrentBaits.Clear();

        if (smashingStampBait == null || smashingStampBait.weaponElement == SmashingStampBait.Element.NONE || !active) {
            return;
        }

        var party = Raid.WithoutSlot(true);
        var len = party.Length;

        if (smashingStampBait.weaponElement == SmashingStampBait.Element.LIGHTNING) {
            for (var i = 0; i < len; i++) {
                CurrentBaits.Add(new(Module.PrimaryActor, party[i], donut));
            }
        }

        if (smashingStampBait.weaponElement == SmashingStampBait.Element.FIRE) {
            for (var i = 0; i < len; i++) {
                CurrentBaits.Add(new(Module.PrimaryActor, party[i], circle));
            }
        }
    }
}

sealed class SlimesInterrupt(BossModule module) : Components.CastInterruptHint(module, default, showNameInHint: true) {
    private readonly SmashingStampBait? smashingStampBait = module.FindComponent<SmashingStampBait>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        var count = Casters.Count;
        for (var i = 0; i < count; ++i) {
            var c = Casters[i];
            var e = hints.FindEnemy(c);
            if (e != null) {
                e.ShouldBeInterrupted |= CanBeInterrupted;
                e.ShouldBeStunned |= CanBeStunned;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (smashingStampBait == null) {
            return;
        }

        // Case: When we have no weapon element & somehow we missed the slime
        if (spell.Action.ID is (uint)AID.RuptureLightning or (uint)AID.RuptureFire) {
            Casters.Add(caster);
            if (ShowNameInHint) {
                UpdateHint();
            }
            return;
        }

        if (spell.Action.ID == (uint)AID.RuptureLightning && smashingStampBait.weaponElement == SmashingStampBait.Element.LIGHTNING) {
            Casters.Add(caster);
            if (ShowNameInHint) {
                UpdateHint();
            }
            return;
        }

        if (spell.Action.ID == (uint)AID.RuptureFire && smashingStampBait.weaponElement == SmashingStampBait.Element.FIRE) {
            Casters.Add(caster);
            if (ShowNameInHint) {
                UpdateHint();
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID is (uint)AID.RuptureLightning or (uint)AID.RuptureFire) {
            if (smashingStampBait == null) {
                return;
            }

            Casters.Remove(caster);

            if (ShowNameInHint) {
                UpdateHint();
            }
        }
    }

    private void UpdateHint() {
        if (!CanBeInterrupted && !CanBeStunned) {
            return;
        }

        var actionStr = !CanBeStunned ? "Interrupt" : !CanBeInterrupted ? "Stun" : "Interrupt/stun";
        var nameStr = ShowNameInHint && Casters.Count == 1 ? Casters[0].OID == (uint)OID.CongealedKindling ? " Orange slime" : " Purple slime" : "";
        Hint = $"{actionStr}{nameStr}!";
        if (HintExtra.Length > 0) {
            Hint += $" ({HintExtra})";
        }
    }
}

sealed class GigantisPieceStates : StateMachineBuilder {
    public GigantisPieceStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<SmashingStampBait>()
            .ActivateOnEnter<SmashingStampBaitAOE>()
            .ActivateOnEnter<SmashingStamp>()
            .ActivateOnEnter<GiganticRageCircle>()
            .ActivateOnEnter<GiganticRageCone>()
            .ActivateOnEnter<AftersparkTower>()
            .ActivateOnEnter<AfterburnAOE>()
            .ActivateOnEnter<GiganticRageType>()
            .ActivateOnEnter<Glower>()
            .ActivateOnEnter<SlimesInterrupt>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.GigantisPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1092u, NameID = 14670u, SortOrder = 8)]
public sealed class GigantisPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f)) {

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        SmashingStampBait? smashingStampBait = FindComponent<SmashingStampBait>()!;
        var element = smashingStampBait.weaponElement;

        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i) {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch {
                (uint)OID.CyclopsPiece => 4,
                (uint)OID.CongealedLightning => slimePriority(element, false),
                (uint)OID.CongealedKindling => slimePriority(element, true),
                (uint)OID.CongealedLightningSmall => 2,
                (uint)OID.CongealedKindlingSmall => 2,
                (uint)OID.GigantisPiece => 1,
                _ => 0
            };
        }
    }

    private int slimePriority(SmashingStampBait.Element element, bool isFire) {
        SmashingStampBait? smashingStampBait = FindComponent<SmashingStampBait>()!;
        if (smashingStampBait.weaponRecentlyChanged) {
            return 3;
        }

        return element switch {
            SmashingStampBait.Element.NONE => 3,
            SmashingStampBait.Element.FIRE => isFire ? 3 : AIHints.Enemy.PriorityForbidden,
            SmashingStampBait.Element.LIGHTNING => isFire ? AIHints.Enemy.PriorityForbidden : 3,
            _ => 2
        };
    }

    protected override void DrawEnemies(int pcSlot, Actor pc) {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.CyclopsPiece));
        Arena.Actors(Enemies((uint)OID.CongealedLightning));
        Arena.Actors(Enemies((uint)OID.CongealedKindling));
        Arena.Actors(Enemies((uint)OID.CongealedLightningSmall));
        Arena.Actors(Enemies((uint)OID.CongealedKindlingSmall));
    }

    private readonly string[] _prePullHints = [
        "Fight kill priority: CyclopsPiece -> Slimes (whichever one we are not killing with bait) -> small slimes / boss",
        "When the boss crushes a slime it will absorb its element and have additional effects after a Gigantic Rage cast ends. Lightning club will leave a tower under you (stay) after a Gigantic Rage, fire club will leave an aoe under you (move) after a Gigantic Rage",
        "Smashing Stamp: Line aoe in front, there is a smaller red rectangle in the aoe near the boss that will kill a slime in it and absorb its element. Avoid hitting a slime with the same element as the club, or the slime will grow large instead of dying",
        "Rapture: Interrupt - If there are 2 slimes, have the boss crush the one with the opposite element of the club, then interrupt the other one",
        "Camaraderie: Interrupt or it will heal the boss 5% max HP"
    ];

    public override string[] PrePullHints => _prePullHints;
}
