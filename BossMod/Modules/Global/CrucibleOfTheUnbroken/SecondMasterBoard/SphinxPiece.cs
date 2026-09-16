namespace BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.SphinxPiece;

public enum OID : uint {
    SphinxPiece = 0x4CFE,
    Helper = 0x233C,
    SquareNumber1 = 0x1EC105, // R0.500, x0 (spawn during fight), EventObj type
    SquareNumber2 = 0x1EC106, // R0.500, x0 (spawn during fight), EventObj type
    SquareNumber3 = 0x1EC107, // R0.500, x0 (spawn during fight), EventObj type
    SquareNumber4 = 0x1EC108, // R0.500, x0 (spawn during fight), EventObj type
    SquareNumber5 = 0x1EC109, // R0.500, x0 (spawn during fight), EventObj type
    SquareNumber6 = 0x1EC10A, // R0.500, x0 (spawn during fight), EventObj type
    SquareNumber7 = 0x1EC10B, // R0.500, x0 (spawn during fight), EventObj type
    SquareNumber8 = 0x1EC10C, // R0.500, x0 (spawn during fight), EventObj type
    SquareNumber9 = 0x1EC10D, // R0.500, x0 (spawn during fight), EventObj type
    DodoPiece = 0x4CFF, // R0.600-1.620, x0 (spawn during fight)
    PugilPiece = 0x4D00, // R0.600-1.950, x0 (spawn during fight)
    PukPiece = 0x4D02, // R0.600-1.500, x0 (spawn during fight)
    OpoOpoPiece = 0x4D01, // R0.600-1.400, x0 (spawn during fight)
    ThisBeast = 0x1EC0E3, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint {
    AutoAttack = 49356, // SphinxPiece->player, no cast, single-target
    Teleport = 49357, // SphinxPiece->location, no cast, single-target
    BanishDonutBoss = 49334, // SphinxPiece->self, 5.0s cast, single-target
    BanishDonut = 49335, // Helper->self, 6.0s cast, range 10-60 donut
    BanishConeBoss = 49338, // SphinxPiece->self, 5.0s cast, single-target
    BanishCone = 49339, // Helper->self, 6.0s cast, range 60 180.000-degree cone
    NumericRiddleBoss = 49352, // SphinxPiece->self, 4.0s cast, single-target
    NumericRiddle = 49353, // Helper->self, 5.0s cast, range 40 circle
    Assignment = 49354, // SphinxPiece->self, 3.0s cast, single-target
    LostHope = 49342, // SphinxPiece->player, 3.0s cast, single-target
    MnemonicRiddle = 49343, // SphinxPiece->self, 4.0s cast, single-target
    MnemonicRiddle1 = 49344, // Helper->self, 5.0s cast, range 40 circle
    Transfigure = 49345, // SphinxPiece->self, 2.0s cast, single-target

    // Adds
    AutoAttackOpoOpoPiece = 49680, // 4D01->player, no cast, single-target
    AutoAttackDodoPiece = 49681, // 4CFF->player, no cast, single-target
    AddTeleport = 49346, // 4D01/4CFF/4D00/4D02->location, no cast, single-target
    Unknown = 49347, // 4D01/4CFF/4D00/4D02->self, no cast, single-target

    // These shouldn't happen - only happens when you fail the mechanic
    RiddleFailedBoss = 49350, // SphinxPiece->self, 2.0s cast, single-target
    RiddleFailed = 49351, // Helper->self, 3.0s cast, range 40 circle
    RiddleFailed1 = 49355, // Helper->player, no cast, single-target
}

public enum SID : uint {
    VulnerabilityDown = 2198, // SphinxPiece->SphinxPiece, extra=0x0
    Transfiguration = 1433, // none->4D01/4CFF/4D00/4D02, extra=0x172
    TemporaryMisdirection = 3909, // SphinxPiece->player, extra=0x2D0
    AllPrime = 5150, // none->player, extra=0x0
    AllThree = 5151, // none->player, extra=0x0
    AllEvens = 5148, // none->player, extra=0x0
}

public enum IconID : uint {
    RiddleCountDown = 718, // player->self
    Unknown = 504, // player->self - most likely misdirection
    Unknown1 = 503, // player->self - most likely misdirection as well
}

sealed class BanishDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BanishDonut, new AOEShapeDonut(10.0f, 60.0f));
sealed class BanishCone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BanishCone, new AOEShapeCone(60.0f, 90.0f.Degrees()));
sealed class NumericRiddle(BossModule module) : Components.RaidwideCast(module, (uint)AID.NumericRiddle);

// TODO add AI - check if needed
sealed class LostHope(BossModule module) : Components.TemporaryMisdirection(module, (uint)AID.LostHope);

// TODO add AI - we show which are safe, so we have to aim for them - we have the list of aoes, so we can just use that
sealed class Assignment : Components.GenericAOEs {
    private readonly AOEShapeRect shape = new(6.0f, 6.0f, 6.0f);
    private readonly List<AOEInstance> aoes = [];
    private readonly List<List<ActorStatus>> debuffs = [];

    public Assignment(BossModule module) : base(module) {
        for (var i = 0; i < PartyState.MaxPartySize; i++) {
            debuffs.Add([]);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        if (status.ID != (uint)SID.AllPrime && status.ID != (uint)SID.AllEvens && status.ID != (uint)SID.AllThree) {
            return;
        }

        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot < 0) {
            return;
        }

        debuffs[slot].Add(status);
        debuffs[slot].Sort((a, b) => a.ExpireAt.CompareTo(b.ExpireAt));
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status) {
        if (status.ID != (uint)SID.AllPrime && status.ID != (uint)SID.AllEvens && status.ID != (uint)SID.AllThree) {
            return;
        }

        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot < 0) {
            return;
        }

        var actorStatus = status;
        debuffs[slot].RemoveAll(d => d.ID == actorStatus.ID);
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        aoes.Clear();

        if (debuffs[slot].Count == 0) {
            return [];
        }

        var sid = debuffs[slot][0].ID;
        for (var i = 1; i <= 9; i++) {
            var safe = sid switch {
                (uint)SID.AllPrime => i is 2 or 3 or 5 or 7,
                (uint)SID.AllThree => i % 3 == 0,
                (uint)SID.AllEvens => i % 2 == 0,
                _ => false
            };

            if (safe) {
                var tile = tileNumber(i);
                if (tile != null) {
                    aoes.Add(new(shape, tile.Position, tile.Rotation, risky: false, color: Colors.SafeFromAOE));
                }
            }
        }

        return CollectionsMarshal.AsSpan(aoes);
    }

    private Actor? tileNumber(int n) {
        var oid = n switch {
            1 => (uint)OID.SquareNumber1,
            2 => (uint)OID.SquareNumber2,
            3 => (uint)OID.SquareNumber3,
            4 => (uint)OID.SquareNumber4,
            5 => (uint)OID.SquareNumber5,
            6 => (uint)OID.SquareNumber6,
            7 => (uint)OID.SquareNumber7,
            8 => (uint)OID.SquareNumber8,
            9 => (uint)OID.SquareNumber9,
            _ => 0u
        };

        return oid == 0u ? null : Module.Enemies(oid).FirstOrDefault();
    }
}

// TODO add animal puzzle

sealed class SphinxPieceStates : StateMachineBuilder {
    public SphinxPieceStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<BanishDonut>()
            .ActivateOnEnter<BanishCone>()
            .ActivateOnEnter<NumericRiddle>()
            .ActivateOnEnter<Assignment>()
            .ActivateOnEnter<LostHope>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Dummy, PrimaryActorOID = (uint)OID.SphinxPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1092u, NameID = 14665u, SortOrder = 6)]
public sealed class SphinxPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, 0f), new ArenaBoundsRect(20f, 20f)) {
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i) {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch {
                (uint)OID.DodoPiece => 2,
                (uint)OID.PugilPiece => 2,
                (uint)OID.PukPiece => 2,
                (uint)OID.OpoOpoPiece => 2,
                (uint)OID.SphinxPiece => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc) {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.DodoPiece));
        Arena.Actors(Enemies((uint)OID.PugilPiece));
        Arena.Actors(Enemies((uint)OID.PukPiece));
        Arena.Actors(Enemies((uint)OID.OpoOpoPiece));
    }
}
