namespace BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.BorgnyTheVenomous;

public enum OID : uint {
    BorgnyTheVenomous = 0x4CD8,
    Helper = 0x233C,
    MagitekArmorPuddle = 0x1EB704, // R0.500, x0 (spawn during fight), EventObj type
    PoisonCloud = 0x4CDA, // R2.000, x0 (spawn during fight)
    ToxicMass = 0x4CD9, // R1.200, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 49680, // BorgnyTheVenomous->player, no cast, single-target
    Teleport = 48806, // BorgnyTheVenomous->location, no cast, single-target
    ToxicBreathBoss = 48807, // BorgnyTheVenomous->self, 3.0s cast, single-target
    ToxicBreath = 48808, // Helper->self, no cast, range ?-60 donut
    ToxicVomitBoss = 48809, // BorgnyTheVenomous->player, 3.5+1.5s cast, range 6 circle
    ToxicVomit = 48810, // BorgnyTheVenomous->player, no cast, range 6 circle
    CauterizeBoss = 48813, // BorgnyTheVenomous->self, 5.2+0.8s cast, single-target
    Cauterize = 48814, // Helper->self, 6.0s cast, range 48 width 20 rect
    TouchdownBoss = 48815, // BorgnyTheVenomous->self, 4.0s cast, single-target
    TouchdownKnockback = 48816, // Helper->self, 4.0s cast, range ?-60 donut
    TouchdownAOE = 48828, // Helper->self, 5.5s cast, range 6 circle
    FumingVomitBoss = 48811, // BorgnyTheVenomous->self, 4.0+2.0s cast, single-target
    FumingVomit = 48812, // Helper->location, 6.0s cast, range 6 circle
    WrigglingPhlegmBoss = 48817, // BorgnyTheVenomous->self, 7.9s cast, single-target
    WrigglingPhlegmTeleport = 48818, // BorgnyTheVenomous->location, no cast, single-target
    WrigglingPhlegm = 48819, // Helper->location, 4.0s cast, range 6 circle
    NoxiousExplosion = 48821, // ToxicMass->self, no cast, range 60 circle
    Unknown = 50543, // Helper->player, no cast, single-target - most likely walking into a moving aoe
    SalivousSnap = 48822, // BorgnyTheVenomous->player, 7.0s cast, single-target
}

public enum SID : uint {
    Toxicosis = 5183 // BorgnyTheVenomous->player, extra=0x1/0x2
}

public enum IconID : uint {
    ToxicVomitIcon = 171, // player->self
    WrigglingPhlegmLockOn = 669, // player->self
    SalivousSnapTankBuster = 475, // player->self
}

public enum TetherID : uint {
    ToxicVomitTether = 17 // BorgnyTheVenomous->player
}

sealed class Cauterize(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Cauterize, new AOEShapeRect(40f, 10f));
sealed class TouchdownAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TouchdownAOE, 6f);
sealed class FumingVomit(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FumingVomit, 6f);
sealed class SalivousSnap(BossModule module) : Components.SingleTargetCast(module, (uint)AID.SalivousSnap);

sealed class ToxicBreathBoss(BossModule module) : Components.GenericAOEs(module) {
    private AOEInstance[] _aoe = [];
    private readonly AOEShapeCone shape = new(60f, 60f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.ToxicBreathBoss) {
            var origin = (Arena.Center - Arena.Bounds.Radius * spell.Rotation.ToDirection()).Quantized();
            var rot = spell.Rotation;
            _aoe = [new(shape, origin, rot, Module.CastFinishAt(spell), shapeDistance: shape.Distance(origin, rot))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.ToxicBreath) {
            if (++NumCasts == 6) {
                _aoe = [];
                NumCasts = 0;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        var count = _aoe.Length;
        if (count == 0) {
            return;
        }

        var aoe = _aoe[0];
        hints.GoalZones.Add(p => aoe.Check(p) ? 0.0f : 5.0f);
    }
}

sealed class ToxicVomit(BossModule module) : Components.BaitAwayIcon(module, 6f, (uint)IconID.ToxicVomitIcon) {
    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.ToxicVomitBoss or (uint)AID.ToxicVomit) {
            NumCasts++;

            if (NumCasts == 4) {
                CurrentBaits.Clear();
                NumCasts = 0;
            }
        }
    }
}

sealed class MagitekArmorPuddles(BossModule module) : Components.Voidzone(module, 6f, GetVoidzones) {
    private static Actor[] GetVoidzones(BossModule module) {
        var enemies = module.Enemies((uint)OID.MagitekArmorPuddle);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i) {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

sealed class PoisonClouds(BossModule module) : Components.GenericAOEs(module) {
    private AOEInstance[] aoes = [];
    private readonly List<Actor> puddles = [];
    private readonly AOEShapeCapsule shape = new(2f, 2.5f);

    public override void OnActorCreated(Actor actor) {
        if (actor.OID is (uint)OID.PoisonCloud) {
            puddles.Add(actor);
        }
    }

    public override void OnActorDestroyed(Actor actor) {
        if (actor.OID is (uint)OID.PoisonCloud) {
            puddles.Remove(actor);
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        return aoes;
    }

    public override void Update() {
        var count = puddles.Count;
        aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i) {
            var puddle = puddles[i];
            aoes[i] = new(shape, puddle.Position, puddle.Rotation, color: Colors.Danger);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        var count = puddles.Count;
        if (count == 0) {
            return;
        }

        var forbiddenNearFuture = WorldState.FutureTime(1.1d);
        var forbiddenSoon = WorldState.FutureTime(3.0d);
        var forbideenFarFuture = DateTime.MaxValue;

        for (var i = 0; i < count; i++) {
            var puddle = puddles[i];
            var position = puddle.Position;
            var rotation = puddle.Rotation;

            hints.AddForbiddenZone(new SDCapsule(position, rotation, shape.Length, shape.Radius), forbiddenNearFuture);
            //hints.AddForbiddenZone(new SDCapsule(position, rotation, shape.Length * 2.0f, shape.Radius), forbiddenSoon);
            hints.AddForbiddenZone(new SDCapsule(position, rotation, shape.Length * 3.0f, shape.Radius), forbideenFarFuture);
            hints.TemporaryObstacles.Add(new SDCircle(position.Quantized(), shape.Radius));
        }
    }
}

sealed class WrigglingPhlegm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WrigglingPhlegm, 6.0f);
sealed class WrigglingPhlegmBait(BossModule module) : Components.BaitAwayIcon(module, 6.0f, (uint)IconID.WrigglingPhlegmLockOn) {
    private readonly MagitekArmorPuddles? magitekArmorPuddles = module.FindComponent<MagitekArmorPuddles>();

    public override void OnEventCast(Actor caster, ActorCastEvent spell) { }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.WrigglingPhlegm) {
            if (CurrentBaits.Count > 0) {
                CurrentBaits.RemoveAt(0);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        base.AddHints(slot, actor, hints);

        if (CurrentBaits.Count == 0) {
            return;
        }

        hints.Add("Bait away from puddles!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);
        if (CurrentBaits.Count == 0 || !IsBaitTarget(actor) || magitekArmorPuddles == null) {
            return;
        }

        List<ShapeDistance> circles = [];
        foreach (var puddle in magitekArmorPuddles.Sources(Module)) {
            circles.Add(new SDCircle(puddle.Position, 6.0f));
        }

        hints.GoalZones.Add(p => new SDUnion([.. circles]).Distance(p));
    }
}

sealed class TouchdownKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.TouchdownKnockback, 30.0f, stopAfterWall: true) {
    private readonly PoisonClouds? poisonClouds = module.FindComponent<PoisonClouds>();
    private readonly MagitekArmorPuddles? magitekArmorPuddles = module.FindComponent<MagitekArmorPuddles>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        if (Casters.Count == 0 || poisonClouds == null || magitekArmorPuddles == null) {
            return;
        }

        var knockbacks = CollectionsMarshal.AsSpan(Casters);
        ref var knockback = ref knockbacks[0];
        if (IsImmune(slot, knockback.Activation)) {
            return;
        }

        List<(WPos origin, float radius)> circles = [];
        var aoes = poisonClouds.ActiveAOEs(slot, actor);
        for (var i = 0; i < aoes.Length; i++) {
            circles.Add((aoes[i].Origin, 2.0f));
        }

        foreach (var puddle in magitekArmorPuddles.Sources(Module)) {
            circles.Add((puddle.Position, 6.0f));
        }

        // Avoid any moving aoes / puddles on the ground
        hints.AddForbiddenZone(new SDInCircleAwayFromOriginPlusIntersectAOECircles(knockback.Origin, knockback.Distance, [.. circles], circles.Count),
            knockback.Activation);

        // Aim to stay by the knockback circle
        hints.GoalZones.Add(AIHints.GoalDonut(Arena.Center, 6.0f, 12.0f));
    }
}

sealed class BorgnyTheVenomousStates : StateMachineBuilder {
    public BorgnyTheVenomousStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<ToxicBreathBoss>()
            .ActivateOnEnter<ToxicVomit>()
            .ActivateOnEnter<MagitekArmorPuddles>()
            .ActivateOnEnter<TouchdownAOE>()
            .ActivateOnEnter<FumingVomit>()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<PoisonClouds>()
            .ActivateOnEnter<WrigglingPhlegmBait>()
            .ActivateOnEnter<WrigglingPhlegm>()
            .ActivateOnEnter<SalivousSnap>()
            .ActivateOnEnter<TouchdownKnockback>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.BorgnyTheVenomous, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1091u, NameID = 14628u, SortOrder = 10)]
public sealed class BorgnyTheVenomous(WorldState ws, Actor primary) : BossModule(ws, primary, new(920f, -420f), new ArenaBoundsCircle(19.7f, 0.35f)) {
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i) {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch {
                (uint)OID.ToxicMass => 2,
                (uint)OID.BorgnyTheVenomous => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc) {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ToxicMass));
    }
}
