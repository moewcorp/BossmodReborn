namespace BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.CorpseFlowerPiece;

public enum OID : uint
{
    CorpseFlowerPiece = 0x4CBD,
    Helper = 0x233C,
    SaplingPiece = 0x4CBE, // R0.750, x0 (spawn during fight)
    QueenHawkPiece = 0x4CBF, // R0.720, x0 (spawn during fight)
    ThornPuddle = 0x1EC0E2, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 49681, // CorpseFlowerPiece->player, no cast, single-target
    AutoAttackQueenHawk = 48688, // 4CBF->player, no cast, single-target
    BuddingThorns = 48687, // CorpseFlowerPiece->self, 3.0s cast, single-target
    RottenStenchVisual = 48690, // CorpseFlowerPiece->self, 5.0+1.0s cast, single-target
    RottenStench = 48691, // CorpseFlowerPiece->self, no cast, range 45 width 12 rect
    FloralTrap = 48683, // CorpseFlowerPiece->self, 5.0s cast, range 80 circle
    FloralTrapPull = 48684, // CorpseFlowerPiece->self, no cast, range 45 ?-degree cone
    Devour = 48685, // CorpseFlowerPiece->self, no cast, range 8 ?-degree cone
    AcidRainBoss = 48692, // CorpseFlowerPiece->self, 3.0s cast, single-target
    AcidRainStart = 48693, // Helper->location, 2.0s cast, range 6 circle
    AcidRainRest = 48694, // Helper->location, no cast, range 6 circle
    FinalSting = 48689, // 4CBF->player, 5.0s cast, single-target
    Spit = 48686, // CorpseFlowerPiece->self, 4.0s cast, range 0 ???
}

public enum SID : uint
{
    Bind = 2518, // CorpseFlowerPiece->player, extra=0x0
    Stun = 2656, // CorpseFlowerPiece->4CBF, extra=0x0
    Briar = 5176, // none->player, extra=0x32
    DamageUp = 2550, // none->CorpseFlowerPiece, extra=0x1
    Devoured = 421, // CorpseFlowerPiece->player, extra=0x0
}

public enum IconID : uint
{
    FloralTrapLockOn = 703, // player->self
    RottenStenchWildCharge = 525, // CorpseFlowerPiece->player
    AcidRainLockOn = 197, // player->self
}

// TODO consider making components for growable circles since they are so common and annoying to work with, DateTime, SID etc
public class FloralTrap(BossModule module) : Components.CastCounter(module, (uint)AID.FloralTrap)
{
    private readonly AOEShapeCircle shape = new(6.0f);
    private readonly Dictionary<Actor, DateTime> growPuddlesTimers = [];

    private DateTime InvertResolveAt;
    private readonly int? ArenaProjectionLayer = null;
    private readonly bool? RestrictToArenaProjectionLayer = false;

    private readonly Func<BossModule, IEnumerable<Actor>> Sources = GetVoidzones;
    private bool Inverted => InvertResolveAt != default;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.ThornPuddle && state == 0x00100020u)
        {
            growPuddlesTimers[actor] = WorldState.CurrentTime;
        }
    }

    public override void OnActorEState(Actor actor, ushort state)
    {
        if (actor.OID == (uint)OID.ThornPuddle && state == 4)
        {
            growPuddlesTimers.Remove(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FloralTrap)
        {
            InvertResolveAt = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Devour)
        {
            InvertResolveAt = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!ArenaProjectionLayerParticipantApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            return;
        }

        var inVoidzone = false;
        foreach (var s in Sources(Module))
        {
            if (ArenaProjectionLayerParticipantApplies(s, ArenaProjectionLayer, RestrictToArenaProjectionLayer) && ShapeSize(s).Check(actor.Position, s))
            {
                inVoidzone = true;
                break;
            }
        }

        if (Inverted)
        {
            hints.Add(inVoidzone ? "Stay in voidzone" : "Go to voidzone!", !inVoidzone);
        }
        else if (inVoidzone)
        {
            hints.Add("GTFO from voidzone!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!ArenaProjectionLayerApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            return;
        }

        var shapes = new List<ShapeDistance>();
        foreach (var source in Sources(Module))
        {
            if (ArenaProjectionLayerParticipantApplies(source, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                var shape = ShapeSize(source).Distance(source.Position.Quantized(), source.Rotation);
                shapes.Add(shape);
            }
        }

        if (shapes.Count == 0)
        {
            return;
        }

        hints.AddForbiddenZone(Inverted ? new SDInvertedUnion([.. shapes]) : new SDUnion([.. shapes]), InvertResolveAt, arenaProjectionLayer: ArenaProjectionLayerForAI(ArenaProjectionLayer, RestrictToArenaProjectionLayer));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var color = Inverted ? Colors.SafeFromAOE : default;
        using (Arena.WorldProjectionLayer(ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            foreach (var s in Sources(Module))
            {
                if (ArenaProjectionLayerParticipantApplies(s, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
                    ShapeSize(s).Draw(Arena, s.Position, s.Rotation, color);
            }
        }
    }

    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.ThornPuddle);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }

    private AOEShapeCircle ShapeSize(Actor source)
    {
        if (!growPuddlesTimers.TryGetValue(source, out var growPuddle))
        {
            return shape;
        }

        var duration = (WorldState.CurrentTime - growPuddle).TotalSeconds;
        // base + (max grow - base) * (time happened / max growth time to reach max size)
        return new((float)Math.Min(6.0f + (10.0f - 6.0f) * (duration / 10.0f), 10.0f));
    }
}

sealed class RottenStench(BossModule module) : Components.GenericWildCharge(module, 6f, (uint)AID.RottenStenchVisual)
{
    private Actor? target;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.RottenStenchWildCharge)
        {
            var targetPlayer = WorldState.Actors.Find(targetID);
            if (targetPlayer == null)
            {
                return;
            }

            target = targetPlayer;
            InitIfReady();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Source = caster;
            Activation = Module.CastFinishAt(spell);
            InitIfReady();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.RottenStench)
        {
            target = null;
            Source = null;
        }
    }

    private void InitIfReady()
    {
        if (target == null || Source == null)
        {
            return;
        }

        foreach (var (slot, player) in Raid.WithSlot(false, true, true))
        {
            PlayerRoles[slot] = player.InstanceID == target.InstanceID ? PlayerRole.Target : PlayerRole.Share;
        }
    }
}

sealed class AcidRainBait(BossModule module) : Components.BaitAwayIcon(module, 6f, (uint)IconID.AcidRainLockOn)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AcidRainStart)
        {
            CurrentBaits.Clear();
        }
    }
}

sealed class AcidRain(BossModule module) : Components.StandardChasingAOEs(module, 6.0f, (uint)AID.AcidRainStart, (uint)AID.AcidRainRest, 5f, 1d, 8,
    icon: (uint)IconID.AcidRainLockOn);

// TODO clean up next time - just checking this works then need to sort it out since we shouldn't copy everything from a component
sealed class Devour(BossModule module) : Components.GenericBaitProximity(module) {
    private readonly AOEShapeCone shape = new(7f, 15f.Degrees()); // TODO check shape size
    private DateTime waitTime = default; // Used for when Devour is casted and causes the player to wait 1 second before moving in since they can still get hit otherwise

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.FloralTrap) {
            CurrentBaits.Add(new(Module.PrimaryActor, shape));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.Devour) {
            waitTime = WorldState.FutureTime(1.0f);
        }
    }

    public override void Update() {
        base.Update();

        if (waitTime != default && WorldState.CurrentTime > waitTime) {
            if (CurrentBaits.Count > 0) {
                CurrentBaits.RemoveAt(0);
            }

            waitTime = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        var baits = CollectionsMarshal.AsSpan(ActiveBaits);
        var len = baits.Length;

        if (len == 0)
        {
            return;
        }

        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            if (!BaitParticipantAppliesToArenaProjectionLayer(actor, b))
            {
                continue;
            }
            var baiter = IsBaitTarget(ref b, actor) ? actor : default;
            if (baiter == default)
            {
                continue;
            }

            var clippedPlayers = PlayersClippedBy(ref b, baiter);
            var clippedCount = clippedPlayers.Count;
            //hints.Add($"Clipped ({clippedCount})", false);

            if (b.IsStack)
            {
                // increment to include player in stack count
                clippedCount++;
                if (clippedCount < b.MinStack)
                {
                    hints.Add("Not enough in stack!");
                    break;
                }
                else if (clippedCount > b.MaxStack)
                {
                    hints.Add("Too many in stack!");
                    break;
                }
            }
            else
            {
                if (clippedPlayers.Count != 0)
                {
                    hints.Add(BaitAwayHint);
                    break;
                }
            }
        }
        if (!IgnoreOtherBaits)
        {
            for (var i = 0; i < len; ++i)
            {
                ref var b = ref baits[i];
                if (!BaitParticipantAppliesToArenaProjectionLayer(actor, b))
                {
                    continue;
                }
                var targets = GetTargets(b);
                var tarLen = targets.Length;

                // show all baits, or all baits aside from yourself
                var subTargets = new Actor[tarLen - (IsBaitTarget(ref b, actor) ? 1 : 0)];
                var subCount = 0;
                for (var j = 0; j < tarLen; ++j)
                {
                    if (targets[j] != actor)
                    {
                        subTargets[subCount++] = targets[j];
                    }
                }

                for (var j = 0; j < subCount; ++j)
                {
                    var target = subTargets[j];
                    if (IsClippedBy(actor, ref b, target))
                    {
                        if (b.IsStack)
                        {
                            var clippedPlayers = PlayersClippedBy(ref b, target);
                            if (clippedPlayers.Count + 1 > b.MaxStack)
                            {
                                hints.Add(BaitAOEHint);
                                return;
                            }
                        }
                        else
                        {
                            hints.Add(BaitAOEHint);
                            return;
                        }
                    }
                }
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (OnlyShowOutlines || IgnoreOtherBaits)
        {
            return;
        }

        BitMask targetted = default;
        var baits = CollectionsMarshal.AsSpan(ActiveBaits);
        var len = baits.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            var targets = GetTargets(b);
            var tarLen = targets.Length;

            for (var j = 0; j < tarLen; ++j)
            {
                var target = targets[j];
                var slot = Raid.FindSlot(target.InstanceID);
                targetted.Set(slot);

                // always draw stacks even if player isn't clipped by it
                if (target != pc && (AlwaysDrawOtherBaits || b.IsStack || IsClippedBy(pc, ref b, target)))
                {
                    using (Arena.WorldProjectionLayer(b.ResolveArenaProjectionLayer(Module, target), b.RestrictToArenaProjectionLayer))
                        b.Shape.Draw(Arena, BaitOrigin(ref b, target), BaitRotation(ref b, target),
                            b.IsStack && ArenaProjectionLayerParticipantApplies(pc, b.ResolveArenaProjectionLayer(Module, target), b.RestrictToArenaProjectionLayer) ? Colors.Safe : Colors.AOE);
                }
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var baits = CollectionsMarshal.AsSpan(ActiveBaits);
        var len = baits.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            var targets = GetTargets(b);
            var tarLen = targets.Length;

            for (var j = 0; j < tarLen; ++j)
            {
                var target = targets[j];
                if (OnlyShowOutlines || !OnlyShowOutlines && target == pc)
                {
                    using (Arena.WorldProjectionLayer(b.ResolveArenaProjectionLayer(Module, target), b.RestrictToArenaProjectionLayer))
                        b.Shape.Outline(Arena, BaitOrigin(ref b, target), BaitRotation(ref b, target));
                }
            }
        }
    }

    private bool IsActive => CurrentBaits.Count > 0;

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        // one bait can have multiple targets
        // just show everyone if there are active baits
        // maybe write so it only shows players that are baiting or getting clipped by bait?
        if (!IsActive)
        {
            return PlayerPriority.Irrelevant;
        }

        var baits = CollectionsMarshal.AsSpan(ActiveBaits);
        var len = baits.Length;

        var haveApplicableBait = false;
        for (var i = 0; i < len; ++i)
        {
            ref var bait = ref baits[i];
            foreach (var target in GetTargets(bait))
            {
                if (!ArenaProjectionLayerApplies(pc, bait.ResolveArenaProjectionLayer(Module, target), bait.RestrictToArenaProjectionLayer))
                {
                    continue;
                }
                haveApplicableBait = true;
                if (target == player)
                {
                    return PlayerPriority.Danger;
                }
            }
        }

        return haveApplicableBait ? PlayerPriority.Normal : PlayerPriority.Irrelevant;
    }

    private new ReadOnlySpan<Actor> GetTargets(Bait bait) {
        var party = Raid.WithSlot(AllowDeadTargets, true, true);
        if (bait.ForbiddenPlayers.Any()) {
            party = [.. party.ExcludedFromMask(bait.ForbiddenPlayers)];
        }

        var partyLen = party.Length;

        var partyRoles = new Actor[partyLen];
        var roleLen = 0;
        for (var i = 0; i < partyLen; ++i) {
            var actor = party[i].Item2;

            // If slot is a pet then skip over it
            if (actor.Type == ActorType.Pet) {
                continue;
            }

            if ((bait.SpecifiedRole == Role.None || actor.Role == bait.SpecifiedRole)
                && Module.ActorMatchesArenaProjectionLayer(actor, bait.ArenaProjectionLayer, bait.RestrictToArenaProjectionLayer)) {
                partyRoles[roleLen++] = actor;
            }
        }

        (Actor actor, float distSq)[] distances = new (Actor, float)[roleLen];
        var result = new Actor[roleLen];

        for (var i = 0; i < roleLen; ++i) {
            var p = partyRoles[i];
            var distSq = (p.Position - bait.Position).LengthSq();
            distances[i] = (p, distSq);
        }

        var isNearest = bait.FromNearest;

        var targets = Math.Min(bait.NumTargets, roleLen);
        for (var i = 0; i < targets; ++i) {
            var selIdx = i;
            for (var j = i + 1; j < roleLen; ++j) {
                if (isNearest && distances[j].distSq < distances[selIdx].distSq || !isNearest && distances[j].distSq > distances[selIdx].distSq) {
                    selIdx = j;
                }
            }

            if (selIdx != i) {
                (distances[selIdx], distances[i]) = (distances[i], distances[selIdx]);
            }

            result[i] = distances[i].actor;
        }

        return result.AsSpan()[..targets];
    }
}

sealed class CorpseFlowerPieceStates : StateMachineBuilder
{
    public CorpseFlowerPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FloralTrap>()
            .ActivateOnEnter<RottenStench>()
            .ActivateOnEnter<AcidRainBait>()
            .ActivateOnEnter<AcidRain>()
            .ActivateOnEnter<Devour>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, PrimaryActorOID = (uint)OID.CorpseFlowerPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1091u, NameID = 14603u, SortOrder = 3)]
public sealed class CorpseFlowerPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.QueenHawkPiece => 2,
                (uint)OID.CorpseFlowerPiece => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.QueenHawkPiece), Colors.Vulnerable);
    }

    private readonly string[] _prePullHints = [
        "Fight kill order priority: QueenHawkPiece (purple) -> Boss (red)",
        "You can use the FloralTrap pull-in to have the boss kill the QueenHawkPiece if you stand behind it during the mechanic"
    ];

    public override string[] PrePullHints => _prePullHints;
}
