namespace BossMod.Dawntrail.Ultimate.DMU;

class UltimateEmbrace(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.UltimateEmbrace, 5.0f);

class Forsaken(BossModule module) : Components.RaidwideCast(module, (uint)AID.Forsaken);

// Used for towers' spawn locations and marking them as SW or SE depending on the spawn point.
class PathOfLight(BossModule module) : Components.GenericTowers(module, (uint)AID._Ability_ThePathOfLight) {
    public Tower? CurrentSW;
    public Tower? CurrentSE;

    public override void OnMapEffect(byte index, uint state) {
        if (index >= 1 && index <= 8 && state == 131073) {
            var angle = (180 - (index - 1) * 45).Degrees();
            Towers.Add(new(Arena.Center + angle.ToDirection() * 8, 4, 2, 2, default, WorldState.FutureTime(10.0f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == WatchedAction) {
            Towers.RemoveAll(tower => tower.Position.AlmostEqual(caster.Position, 1.0f));
        }
    }

    public override void Update() {
        if (Towers.Count != 2) {
            return;
        }

        var tower1 = Towers[0].Position;
        var tower2 = Towers[1].Position;

        var middleOfTowers = new WPos((tower1.X + tower2.X) * 0.5f, (tower1.Z + tower2.Z) * 0.5f);
        var southDirection = (middleOfTowers - Arena.Center).Normalized();
        if (southDirection.Length() <= 0) {
            return;
        }

        if ((tower1 - middleOfTowers).Dot(southDirection.OrthoL()) <= (tower2 - middleOfTowers).Dot(southDirection.OrthoL())) {
            CurrentSW = Towers[0];
            CurrentSE = Towers[1];
        } else {
            CurrentSW = Towers[1];
            CurrentSE = Towers[0];
        }
    }
}

// Used for setting up each player's role, such as the shape the player has, the pair the player belongs, if the player is a helper or soaker, etc.
class ForsakenShapes(BossModule module) : BossComponent(module) {
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    public int currentTowerSet = 1; // We start on odd tower set
    public bool towerSetLocked = false;
    public DateTime? lastTowerSetChange = null;

    public enum Shape { None, Spread, Cone, Stack }
    public Shape[] shapes = new Shape[8];
    public enum TowerRole { Unknown, Helper, Taker }

    public BitMask swSoakers;
    public BitMask seSoakers;
    public BitMask supportHelpers;
    public BitMask dpsHelpers;

    // TODO merge these together
    public bool pairsLocked = false;
    private bool pairsSwapped = false;

    public sealed class PairInfo {
        public PartyRolesConfig.Assignment player1Assignment;
        public PartyRolesConfig.Assignment player2Assignment;
        public bool isSupport;
        public TowerRole role;

        public PairInfo(PartyRolesConfig.Assignment player1, PartyRolesConfig.Assignment player2, bool isSupport) {
            player1Assignment = player1;
            player2Assignment = player2;
            this.isSupport = isSupport;
            role = TowerRole.Unknown;
        }
    }

    public readonly PairInfo[] pairs = [
        new(PartyRolesConfig.Assignment.MT, PartyRolesConfig.Assignment.H1, true),
        new(PartyRolesConfig.Assignment.OT, PartyRolesConfig.Assignment.H2, true),
        new(PartyRolesConfig.Assignment.M1, PartyRolesConfig.Assignment.R1, false),
        new(PartyRolesConfig.Assignment.M2, PartyRolesConfig.Assignment.R2, false),
    ];

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID._Ability_ThePathOfLight) {
            if (towerSetLocked == false) {
                lastTowerSetChange = WorldState.CurrentTime;
                towerSetLocked = true;
                currentTowerSet++;
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID) {
        var shape = (IconID)iconID switch {
            IconID.TowerSpreadIcon => Shape.Spread,
            IconID.TowerConeIcon => Shape.Cone,
            IconID.TowerStackIcon => Shape.Stack,
            _ => default
        };

        if (shape != default) {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0) {
                shapes[slot] = shape;
            }
        }
    }

    public override void Update() {
        swSoakers = default;
        seSoakers = default;
        supportHelpers = default;
        dpsHelpers = default;

        if (WorldState.CurrentTime - lastTowerSetChange > TimeSpan.FromSeconds(1.0) && towerSetLocked == true) {
            towerSetLocked = false;
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0) {
            return;
        }

        SetupPairs(slots);

        if (currentTowerSet == 4 && pairsSwapped == false) {
            foreach (var pair in pairs) {
                pair.role = pair.role switch {
                    TowerRole.Helper => TowerRole.Taker,
                    TowerRole.Taker => TowerRole.Helper,
                    _ => pair.role
                };
            }

            pairsSwapped = true;
        }

        foreach (var pair in pairs) {
            var slotPlayer1 = slots[(int)pair.player1Assignment];
            var slotPlayer2 = slots[(int)pair.player2Assignment];
            var shapeA = shapes[slotPlayer1];
            var shapeB = shapes[slotPlayer2];

            if (pair.role == TowerRole.Helper) {
                if (pair.isSupport) {
                    supportHelpers.Set(slotPlayer1);
                    supportHelpers.Set(slotPlayer2);
                } else {
                    dpsHelpers.Set(slotPlayer1);
                    dpsHelpers.Set(slotPlayer2);
                }
            }

            if (pair.role == TowerRole.Taker) {
                // First set of towers (tower set odd)
                if (currentTowerSet % 2 != 0) {
                    // Case: for the first set of towers no adjustment is needed between the Melee & Tank
                    if (currentTowerSet == 1) {
                        if (shapeA == Shape.Cone || shapeB == Shape.Cone) {
                            swSoakers.Set(slotPlayer1);
                            swSoakers.Set(slotPlayer2);
                        }

                        if (shapeA == Shape.Spread || shapeB == Shape.Spread) {
                            seSoakers.Set(slotPlayer1);
                            seSoakers.Set(slotPlayer2);
                        }
                    }

                    // Case: every odd tower beyond the first set, the Melee & Tank may need to adjust base on pair shapes
                    if (currentTowerSet > 1) {
                        Service.Logger.Info($"Tower set beyone 1 (odd set - 2nd)");
                        // Cone and Spread are always forced
                        if (shapeA == Shape.Cone) {
                            swSoakers.Set(slotPlayer1);
                        }

                        if (shapeB == Shape.Cone) {
                            swSoakers.Set(slotPlayer2);
                        }

                        if (shapeA == Shape.Spread) {
                            seSoakers.Set(slotPlayer1);
                        }

                        if (shapeB == Shape.Spread) {
                            seSoakers.Set(slotPlayer2);
                        }

                        // If both of them are stacks, the melee & tank must swap
                        if (shapeA == Shape.Stack && shapeB == Shape.Stack) {
                            Service.Logger.Info($"Tower set beyone 1 (odd set - 2nd) - both stacks");
                            // Supports
                            if (pair.isSupport) {
                                seSoakers.Set(slotPlayer1);
                                swSoakers.Set(slotPlayer2);
                            }

                            // DPS
                            if (pair.isSupport == false) {
                                swSoakers.Set(slotPlayer1);
                                seSoakers.Set(slotPlayer2);
                            }

                        }
                    }
                }

                // Second set of towers (tower set even)
                if (currentTowerSet % 2 == 0) {
                    if (pair.isSupport) {
                        // They have different shapes - both go to the same tower which is west tower
                        if (shapeA != shapeB) {
                            swSoakers.Set(slotPlayer1);
                            swSoakers.Set(slotPlayer2);
                        } else { // healer goes to SW tower, tank goes to SE tower - player2 is healer, player1 is tank
                            swSoakers.Set(slotPlayer2);
                            seSoakers.Set(slotPlayer1);
                        }
                    }

                    if (pair.isSupport == false) {
                        if (shapeA != shapeB) {
                            // They have different shapes - both go to the same tower which is east tower
                            seSoakers.Set(slotPlayer1);
                            seSoakers.Set(slotPlayer2);
                        } else {
                            // range goes to SE tower, melee goes to SE tower - player2 is range, player1 is melee
                            seSoakers.Set(slotPlayer2);
                            swSoakers.Set(slotPlayer1);
                        }
                    }
                }
            }
        }

        foreach (var pair in pairs)
        {
            Service.Logger.Info($"Pair {pair.player1Assignment} and {pair.player2Assignment} with shapes {shapes[slots[(int)pair.player1Assignment]]} and {shapes[slots[(int)pair.player2Assignment]]} is {(pair.role == TowerRole.Helper ? "Helper" : pair.role == TowerRole.Taker ? "Taker" : "Unknown")}");
        }
    }

    private void SetupPairs(int[] slots) {
        if (pairsLocked == true) {
            return;
        }

        pairsLocked = true;

        foreach (var pair in pairs) {
            if (pair.role != TowerRole.Unknown) {
                continue;
            }

            var shapeA = shapes[slots[(int)pair.player1Assignment]];
            var shapeB = shapes[slots[(int)pair.player2Assignment]];

            if (shapeA == Shape.None || shapeB == Shape.None) {
                pairsLocked = false;
                continue;
            }

            pair.role = shapeA == shapeB ? TowerRole.Helper : TowerRole.Taker;
        }

        foreach (var pair in pairs) {
            if (pair.role == TowerRole.Unknown) {
                pairsLocked = false;
                break;
            }
        }
    }
}

// Used for odd tower sets in figuring out what each player is responsible for
class ForsakenSolverSet1(BossModule module) : BossComponent(module) {
    private ForsakenShapes? shapes = module.FindComponent<ForsakenShapes>();
    private PathOfLight? towers = module.FindComponent<PathOfLight>();
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (shapes == null || towers == null) {
            return;
        }

        if (shapes.swSoakers.None() || shapes.seSoakers.None() || towers.CurrentSW == null || towers.CurrentSE == null) {
            return;
        }

        if (shapes.currentTowerSet % 2 == 0) {
            return;
        }


        var midpoint = new WPos((towers.CurrentSW.Value.Position.X + towers.CurrentSE.Value.Position.X) * 0.5f,
            (towers.CurrentSW.Value.Position.Z + towers.CurrentSE.Value.Position.Z) * 0.5f);
        var newSouth = (midpoint - Arena.Center).Normalized();

        var towardSW = (towers.CurrentSW.Value.Position - midpoint).Normalized();
        var towardSE = (towers.CurrentSE.Value.Position - midpoint).Normalized();

        // Case: SW players with different debuffs
        if (shapes.swSoakers[pcSlot]) {
            var swPosition = towers.CurrentSW.Value.Position;
            var shape = shapes.shapes[pcSlot];

            if (shape == ForsakenShapes.Shape.Stack) {
                Arena.AddCircle(swPosition + (-towardSW * 0.5f) + (-newSouth * 1.0f), 1.0f, Colors.Safe, 2.0f);
            }

            if (shape == ForsakenShapes.Shape.Cone) {
                Arena.AddCircle(swPosition + (newSouth * 3.0f), 1.0f, Colors.Safe, 2.0f);
            }
        }

        // Case: SW players with same debuffs
        if (shapes.supportHelpers[pcSlot]) {
            var swPosition = towers.CurrentSW.Value.Position;
            var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

            if (assignment == PartyRolesConfig.Assignment.H1 || assignment == PartyRolesConfig.Assignment.H2) {
                Arena.AddCircle(swPosition + (newSouth * 4.5f), 1.0f, Colors.Safe, 2.0f);
            }

            if (assignment == PartyRolesConfig.Assignment.MT || assignment == PartyRolesConfig.Assignment.OT) {
                Arena.AddCircle(swPosition + (-towardSW * 3.0f) + (-newSouth * 4.0f), 1.0f, Colors.Safe, 2.0f);
            }
        }

        // Case: SE players with different debuffs
        if (shapes.seSoakers[pcSlot]) {
            var sePosition = towers.CurrentSE.Value.Position;
            var shape = shapes.shapes[pcSlot];

            if (shape == ForsakenShapes.Shape.Stack) {
                Arena.AddCircle(sePosition + (-towardSE * 2.5f) + (newSouth * 2.5f), 1.0f, Colors.Safe, 2.0f);
            }

            if (shape == ForsakenShapes.Shape.Spread) {
                Arena.AddCircle(sePosition + (towardSE * 2.0f) + (-newSouth * 3.0f), 1.0f, Colors.Safe, 2.0f);
            }
        }

        // Case: SE players with different debuffs
        if (shapes.dpsHelpers[pcSlot]) {
            var sePosition = towers.CurrentSE.Value.Position;
            var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

            if (assignment == PartyRolesConfig.Assignment.M1 || assignment == PartyRolesConfig.Assignment.M2) {
                Arena.AddCircle(sePosition + (-towardSE * 4.0f) + (newSouth * 3.0f), 1.0f, Colors.Safe, 2.0f);
            }

            if (assignment == PartyRolesConfig.Assignment.R1 || assignment == PartyRolesConfig.Assignment.R2) {
                Arena.AddCircle(sePosition + (-towardSE * 4.0f) + (newSouth * 3.0f), 1.0f, Colors.Safe, 2.0f);
            }
        }
    }

    public override void Update() {
        if (shapes == null || towers == null) {
            return;
        }

        if (towers.Towers.Count != 2 || towers.CurrentSE == null || towers.CurrentSW == null) {
            return;
        }

        if (shapes.currentTowerSet % 2 == 0) {
            return;
        }

        var party = new BitMask(0xFF);

        for (int i = 0; i < towers.Towers.Count; i++) {
            var t = towers.Towers[i];

            if (t.Position.AlmostEqual(towers.CurrentSW.Value.Position, 1.0f)) {
                t.ForbiddenSoakers = party & ~shapes.swSoakers;
            }

            if (t.Position.AlmostEqual(towers.CurrentSE.Value.Position, 1.0f)) {
                t.ForbiddenSoakers = party & ~shapes.seSoakers;
            }

            towers.Towers[i] = t;
        }
    }
}

// Used for even tower sets in figuring out what each player is responsible for
class ForsakenSolverSet2(BossModule module) : BossComponent(module) {
    private ForsakenShapes? shapes = module.FindComponent<ForsakenShapes>();
    private PathOfLight? towers = module.FindComponent<PathOfLight>();
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (shapes == null || towers == null) {
            return;
        }

        if (shapes.swSoakers.None() || towers.CurrentSW == null || shapes.seSoakers.None() || towers.CurrentSE == null) {
            return;
        }

        if (shapes.currentTowerSet % 2 != 0) {
            return;
        }

        // Case: SW players with different debuffs (soakers)
        if (shapes.swSoakers[pcSlot]) {
            var swPosition = towers.CurrentSW.Value.Position;
            var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

            var toCenter = (Arena.Center - swPosition).Normalized();
            if (toCenter.LengthSq() <= 0) {
                return;
            }

            if (shapes.shapes[pcSlot] == ForsakenShapes.Shape.Cone) {
                Arena.AddCircle(swPosition + toCenter.Normalized() * 3.5f, 1.0f, Colors.Safe, 2.0f);
            }

            if (shapes.shapes[pcSlot] == ForsakenShapes.Shape.Spread) {
                Arena.AddCircle(swPosition + (-toCenter.Normalized()) * 3.5f, 1.0f, Colors.Safe, 2.0f);
            }

            /*if (assignment == PartyRolesConfig.Assignment.H1 || assignment == PartyRolesConfig.Assignment.H2) {
                Arena.AddCircle(swPosition + toCenter.Normalized() * 3.5f, 1.0f, Colors.Safe, 2.0f);
            }

            if (assignment == PartyRolesConfig.Assignment.MT || assignment == PartyRolesConfig.Assignment.OT ||
                assignment == PartyRolesConfig.Assignment.M1 || assignment == PartyRolesConfig.Assignment.M2) {
                Arena.AddCircle(swPosition + (-toCenter.Normalized()) * 3.5f, 1.0f, Colors.Safe, 2.0f);
            }*/
        }

        // Case: SW players with same debuffs (helpers)
        if (shapes.supportHelpers[pcSlot]) {
            var swPosition = towers.CurrentSW.Value.Position;
            var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

            var toCenter = (Arena.Center - swPosition).Normalized();
            if (toCenter.LengthSq() <= 0) {
                return;
            }

            if (assignment == PartyRolesConfig.Assignment.H1 || assignment == PartyRolesConfig.Assignment.H2) {
                Arena.AddCircle(swPosition + toCenter.Rotate(90f.Degrees()).Normalized() * 4.5f, 1.0f, Colors.Safe, 2.0f);
            }

            if (assignment == PartyRolesConfig.Assignment.MT || assignment == PartyRolesConfig.Assignment.OT) {
                Arena.AddCircle(swPosition + toCenter.Rotate(35f.Degrees()).Normalized() * 9.8f, 1.0f, Colors.Safe, 2.0f);
            }
        }

        // Case: SE players with different debuffs (soakers)
        if (shapes.seSoakers[pcSlot]) {
            var sePosition = towers.CurrentSE.Value.Position;
            var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

            var toCenter = (Arena.Center - sePosition).Normalized();
            if (toCenter.LengthSq() <= 0) {
                return;
            }

            if (shapes.shapes[pcSlot] == ForsakenShapes.Shape.Cone) {
                Arena.AddCircle(sePosition + toCenter.Normalized() * 3.5f, 1.0f, Colors.Safe, 2.0f);
            }

            if (shapes.shapes[pcSlot] == ForsakenShapes.Shape.Spread) {
                Arena.AddCircle(sePosition + (-toCenter.Normalized()) * 3.5f, 1.0f, Colors.Safe, 2.0f);
            }

            /*if (assignment == PartyRolesConfig.Assignment.R1 || assignment == PartyRolesConfig.Assignment.R2) {
                Arena.AddCircle(sePosition + (-toCenter.Normalized()) * 3.5f, 1.0f, Colors.Safe, 2.0f);
            }

            if (assignment == PartyRolesConfig.Assignment.MT || assignment == PartyRolesConfig.Assignment.OT ||
                assignment == PartyRolesConfig.Assignment.M1 || assignment == PartyRolesConfig.Assignment.M2) {
                Arena.AddCircle(sePosition + toCenter.Normalized() * 3.5f, 1.0f, Colors.Safe, 2.0f);
            }*/
        }

        // Case: SE players with same debuffs (helpers)
        if (shapes.dpsHelpers[pcSlot]) {
            var sePosition = towers.CurrentSE.Value.Position;
            var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

            var toCenter = (Arena.Center - sePosition).Normalized();
            if (toCenter.LengthSq() <= 0) {
                return;
            }

            if (assignment == PartyRolesConfig.Assignment.R1 || assignment == PartyRolesConfig.Assignment.R2) {
                Arena.AddCircle(sePosition + toCenter.Rotate(-90f.Degrees()).Normalized() * 4.5f, 1.0f, Colors.Safe, 2.0f);
            }

            if (assignment == PartyRolesConfig.Assignment.M1 || assignment == PartyRolesConfig.Assignment.M2) {
                Arena.AddCircle(sePosition + toCenter.Rotate(-35f.Degrees()).Normalized() * 9.8f, 1.0f, Colors.Safe, 2.0f);
            }
        }
    }

    public override void Update() {
        if (shapes == null || towers == null) {
            return;
        }

        if (towers.Towers.Count != 2 || towers.CurrentSE == null || towers.CurrentSW == null) {
            return;
        }

        if (shapes.currentTowerSet % 2 != 0) {
            return;
        }

        var party = new BitMask(0xFF);

        for (int i = 0; i < towers.Towers.Count; i++) {
            var t = towers.Towers[i];

            if (t.Position.AlmostEqual(towers.CurrentSW.Value.Position, 1.0f)) {
                t.ForbiddenSoakers = party & ~shapes.swSoakers;
            }

            if (t.Position.AlmostEqual(towers.CurrentSE.Value.Position, 1.0f)) {
                t.ForbiddenSoakers = party & ~shapes.seSoakers;
            }

            towers.Towers[i] = t;
        }
    }
}
