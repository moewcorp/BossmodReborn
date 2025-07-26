﻿namespace BossMod.Components;

// generic component for mechanics that require baiting some aoe (by proximity, by tether, etc) away from raid
// some players can be marked as 'forbidden' - if any of them is baiting, they are warned
// otherwise we show own bait as as outline (and warn if player is clipping someone) and other baits as filled (and warn if player is being clipped)
public class GenericBaitAway(BossModule module, uint aid = default, bool alwaysDrawOtherBaits = true, bool centerAtTarget = false, bool tankbuster = false, bool onlyShowOutlines = false, AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.None) : CastCounter(module, aid)
{
    public struct Bait(Actor source, Actor target, AOEShape shape, DateTime activation = default, BitMask forbidden = default, Angle? customRotation = null, int maxCasts = 1)
    {
        public Angle? CustomRotation = customRotation;
        public AOEShape Shape = shape;
        public Actor Source = source;
        public Actor Target = target;
        public DateTime Activation = activation;
        public BitMask Forbidden = forbidden;
        public int MaxCasts = maxCasts;

        public readonly Angle Rotation => CustomRotation ?? (Source != Target ? Angle.FromDirection(Target.Position - Source.Position) : Source.Rotation);

        public Bait(WPos source, Actor target, AOEShape shape, DateTime activation = default, Angle? customRotation = null, BitMask forbidden = default, int maxCasts = 1)
            : this(new(default, default, default, default, default!, default, default, default, default, source.ToVec4()), target, shape, activation, forbidden, customRotation, maxCasts) { }
    }

    public readonly bool AlwaysDrawOtherBaits = alwaysDrawOtherBaits; // if false, other baits are drawn only if they are clipping a player
    public readonly bool CenterAtTarget = centerAtTarget; // if true, aoe source is at target
    public readonly bool OnlyShowOutlines = onlyShowOutlines; // if true only show outlines
    public bool AllowDeadTargets = true; // if false, baits with dead targets are ignored
    public bool EnableHints = true;
    public bool IgnoreOtherBaits; // if true, don't show hints/aoes for baits by others
    public PlayerPriority BaiterPriority = PlayerPriority.Interesting;
    public BitMask ForbiddenPlayers; // these players should avoid baiting
    public List<Bait> CurrentBaits = [];
    public AIHints.PredictedDamageType DamageType = damageType;
    public const string BaitAwayHint = "Bait away from raid!";

    public List<Bait> ActiveBaits
    {
        get
        {
            var count = CurrentBaits.Count;
            if (count == 0)
            {
                return [];
            }
            List<Bait> activeBaits = new(count);
            var curBaits = CollectionsMarshal.AsSpan(CurrentBaits);
            for (var i = 0; i < count; ++i)
            {
                ref var bait = ref curBaits[i];
                if (!bait.Source.IsDead)
                {
                    if (AllowDeadTargets || !bait.Target.IsDead)
                    {
                        activeBaits.Add(bait);
                    }
                }
            }
            return activeBaits;
        }
    }

    public List<Bait> ActiveBaitsOn(Actor target)
    {
        var count = CurrentBaits.Count;
        if (count == 0)
        {
            return [];
        }
        List<Bait> activeBaitsOnTarget = new(count);
        var curBaits = CollectionsMarshal.AsSpan(CurrentBaits);
        var id = target.InstanceID;
        for (var i = 0; i < count; ++i)
        {
            ref readonly var bait = ref curBaits[i];
            if (!bait.Source.IsDead && bait.Target.InstanceID == id)
            {
                activeBaitsOnTarget.Add(bait);
            }
        }
        return activeBaitsOnTarget;
    }

    public List<Bait> ActiveBaitsNotOn(Actor target)
    {
        var count = CurrentBaits.Count;
        if (count == 0)
        {
            return [];
        }
        List<Bait> activeBaitsNotOnTarget = new(count);
        var curBaits = CollectionsMarshal.AsSpan(CurrentBaits);
        var id = target.InstanceID;
        for (var i = 0; i < count; ++i)
        {
            ref var bait = ref curBaits[i];
            if (!bait.Source.IsDead && bait.Target.InstanceID != id)
            {
                activeBaitsNotOnTarget.Add(bait);
            }
        }
        return activeBaitsNotOnTarget;
    }

    public WPos BaitOrigin(Bait bait) => (CenterAtTarget ? bait.Target : bait.Source).Position;
    public bool IsClippedBy(Actor actor, Bait bait) => bait.Shape.Check(actor.Position, BaitOrigin(bait), bait.Rotation);
    public List<Actor> PlayersClippedBy(ref readonly Bait bait)
    {
        var actors = Raid.WithoutSlot();
        var len = actors.Length;
        List<Actor> result = new(len);
        for (var i = 0; i < len; ++i)
        {
            ref readonly var actor = ref actors[i];
            ref readonly var id = ref actor.InstanceID;
            if (id != bait.Target.InstanceID && bait.Shape.Check(actor.Position, BaitOrigin(bait), bait.Rotation))
                result.Add(actor);
        }

        return result;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!EnableHints)
            return;
        var baits = CollectionsMarshal.AsSpan(ActiveBaits);
        var count = baits.Length;
        if (count == 0)
            return;
        if (ForbiddenPlayers[slot])
        {
            if (ActiveBaitsOn(actor).Count != 0)
            {
                hints.Add("Avoid baiting!");
            }
        }
        else
        {
            var id = actor.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                ref var bait = ref baits[i];
                if (bait.Target.InstanceID != id)
                    continue;
                var clippedPlayers = PlayersClippedBy(ref bait);
                if (clippedPlayers.Count != 0)
                {
                    hints.Add(BaitAwayHint);
                    break;
                }
            }
        }

        if (!IgnoreOtherBaits)
        {
            var id = actor.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                ref var bait = ref baits[i];
                if (bait.Target.InstanceID == id)
                    continue;
                if (IsClippedBy(actor, bait))
                {
                    hints.Add("GTFO from baited aoe!");
                    break;
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var baits = CollectionsMarshal.AsSpan(ActiveBaits);
        var count = baits.Length;
        if (count == 0)
        {
            return;
        }
        BitMask predictedDamage = default;
        for (var i = 0; i < count; ++i)
        {
            ref var bait = ref baits[i];
            if (bait.Target != actor)
            {
                hints.AddForbiddenZone(bait.Shape, BaitOrigin(bait), bait.Rotation, bait.Activation);
            }
            else
            {
                AddTargetSpecificHints(ref actor, ref bait, ref hints);
            }
            if (DamageType != AIHints.PredictedDamageType.None)
            {
                predictedDamage.Set(Raid.FindSlot(bait.Target.InstanceID));
            }
        }
        if (predictedDamage != default)
        {
            hints.AddPredictedDamage(predictedDamage, CurrentBaits.Ref(0).Activation, DamageType);
        }
    }

    private void AddTargetSpecificHints(ref Actor actor, ref Bait bait, ref AIHints hints)
    {
        if (bait.Source == bait.Target) // TODO: think about how to handle source == target baits, eg. vomitting mechanics
            return;
        var raid = Raid.WithoutSlot();
        var len = raid.Length;
        for (var i = 0; i < len; ++i)
        {
            var a = raid[i];
            if (a == actor)
                continue;
            switch (bait.Shape)
            {
                case AOEShapeDonut:
                case AOEShapeCircle:
                    hints.AddForbiddenZone(bait.Shape, a.Position, default, bait.Activation);
                    break;
                case AOEShapeCone cone:
                    hints.AddForbiddenZone(ShapeDistance.Cone(bait.Source.Position, 100f, bait.Source.AngleTo(a), cone.HalfAngle), bait.Activation);
                    break;
                case AOEShapeRect rect:
                    hints.AddForbiddenZone(ShapeDistance.Cone(bait.Source.Position, 100f, bait.Source.AngleTo(a), Angle.Asin(rect.HalfWidth / (a.Position - bait.Source.Position).Length())), bait.Activation);
                    break;
                case AOEShapeCross cross:
                    hints.AddForbiddenZone(cross, a.Position, bait.Rotation, bait.Activation);
                    break;
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => ActiveBaitsOn(player).Count != 0 ? BaiterPriority : PlayerPriority.Irrelevant;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (OnlyShowOutlines || IgnoreOtherBaits)
            return;

        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var len = baits.Length;
        var pcID = pc.InstanceID;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var b = ref baits[i];
            if (!b.Source.IsDead && b.Target.InstanceID != pcID && (AlwaysDrawOtherBaits || IsClippedBy(pc, b)))
            {
                b.Shape.Draw(Arena, BaitOrigin(b), b.Rotation);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var len = baits.Length;
        var pcID = pc.InstanceID;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var b = ref baits[i];
            if (!b.Source.IsDead && (OnlyShowOutlines || !OnlyShowOutlines && b.Target.InstanceID == pcID))
            {
                b.Shape.Outline(Arena, BaitOrigin(b), b.Rotation);
            }
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (tankbuster && CurrentBaits.Count != 0)
            hints.Add("Tankbuster cleave");
    }
}

// bait on all players, requiring everyone to spread out
public class BaitAwayEveryone : GenericBaitAway
{
    public BaitAwayEveryone(BossModule module, Actor? source, AOEShape shape, uint aid = default) : base(module, aid, damageType: AIHints.PredictedDamageType.Raidwide)
    {
        AllowDeadTargets = false;
        if (source != null)
        {
            var party = Raid.WithoutSlot(true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                CurrentBaits.Add(new(source, party[i], shape));
            }
        }
    }
}

// component for mechanics requiring tether targets to bait their aoe away from raid
public class BaitAwayTethers(BossModule module, AOEShape shape, uint tetherID, uint aid = default, uint enemyOID = default, double activationDelay = default, bool centerAtTarget = false) : GenericBaitAway(module, aid, centerAtTarget: centerAtTarget, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public BaitAwayTethers(BossModule module, float radius, uint tetherID, uint aid = default, uint enemyOID = default, double activationDelay = default, bool centerAtTarget = true) : this(module, new AOEShapeCircle(radius), tetherID, aid, enemyOID, activationDelay, centerAtTarget) { }

    public AOEShape Shape = shape;
    public uint TID = tetherID;
    public readonly uint EnemyOID = enemyOID;
    public bool DrawTethers = true;
    public double ActivationDelay = activationDelay;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (DrawTethers)
        {
            var baits = ActiveBaits;
            var count = baits.Count;
            for (var i = 0; i < count; ++i)
            {
                var b = baits[i];
                Arena.AddLine(b.Source.Position, b.Target.Position);
            }
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var (player, enemy) = DetermineTetherSides(source, tether);
        if (player != null && enemy != null && (EnemyOID == default || enemy.OID == EnemyOID))
        {
            CurrentBaits.Add(new(enemy, player, Shape, WorldState.FutureTime(ActivationDelay)));
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        var (player, enemy) = DetermineTetherSides(source, tether);
        if (player != null && enemy != null)
        {
            var eID = enemy.InstanceID;
            var pID = player.InstanceID;
            var count = CurrentBaits.Count;
            var baits = CollectionsMarshal.AsSpan(CurrentBaits);
            for (var i = 0; i < count; ++i)
            {
                ref var b = ref baits[i];
                if (b.Source.InstanceID == eID && b.Target.InstanceID == pID)
                {
                    CurrentBaits.RemoveAt(i);
                    return;
                }
            }
        }
    }

    // we support both player->enemy and enemy->player tethers
    public (Actor? player, Actor? enemy) DetermineTetherSides(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID != TID)
            return (null, null);

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
            return (null, null);

        var (player, enemy) = source.Type is ActorType.Player or ActorType.Buddy ? (source, target) : (target, source);
        if (player.Type is not ActorType.Player and not ActorType.Buddy || enemy.Type is ActorType.Player or ActorType.Buddy)
        {
            ReportError($"Unexpected tether pair: {source.InstanceID:X} -> {target.InstanceID:X}");
            return (null, null);
        }

        return (player, enemy);
    }
}

// component for mechanics requiring icon targets to bait their aoe away from raid
public class BaitAwayIcon(BossModule module, AOEShape shape, uint iconID, uint aid = default, double activationDelay = 5.1d, bool centerAtTarget = false, Actor? source = null, bool tankbuster = false, AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Raidwide) : GenericBaitAway(module, aid, centerAtTarget: centerAtTarget, tankbuster: tankbuster, damageType: damageType)
{
    public BaitAwayIcon(BossModule module, float radius, uint iconID, uint aid = default, double activationDelay = 5.1d, bool centerAtTarget = true, Actor? source = null, bool tankbuster = false, AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Raidwide) : this(module, new AOEShapeCircle(radius), iconID, aid, activationDelay, centerAtTarget, source, tankbuster, damageType) { }

    public AOEShape Shape = shape;
    public uint IID = iconID;
    public double ActivationDelay = activationDelay;

    public virtual Actor? BaitSource(Actor target) => source ?? Module.PrimaryActor;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == IID && BaitSource(actor) is var source && source != null)
        {
            CurrentBaits.Add(new(source, WorldState.Actors.Find(targetID) ?? actor, Shape, WorldState.FutureTime(ActivationDelay)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (CurrentBaits.Count != 0 && spell.Action.ID == WatchedAction)
        {
            CurrentBaits.RemoveAt(0);
        }
    }

    public override void Update()
    {
        var count = CurrentBaits.Count - 1;
        for (var i = count; i >= 0; --i)
        {
            ref var b = ref CurrentBaits.Ref(i);
            if (b.Target.IsDead)
            {
                CurrentBaits.RemoveAt(i);
            }
        }
    }
}

// component for mechanics requiring cast targets to gtfo from raid (aoe tankbusters etc)
public class BaitAwayCast(BossModule module, uint aid, AOEShape shape, bool centerAtTarget = false, bool endsOnCastEvent = false, bool tankbuster = false, AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Raidwide) : GenericBaitAway(module, aid, centerAtTarget: centerAtTarget, tankbuster: tankbuster, damageType: damageType)
{
    public BaitAwayCast(BossModule module, uint aid, float radius, bool centerAtTarget = true, bool endsOnCastEvent = false, bool tankbuster = false, AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Raidwide) : this(module, aid, new AOEShapeCircle(radius), centerAtTarget, endsOnCastEvent, tankbuster, damageType: damageType) { }

    public AOEShape Shape = shape;
    public bool EndsOnCastEvent = endsOnCastEvent;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction && WorldState.Actors.Find(spell.TargetID) is var target && target != null)
        {
            CurrentBaits.Add(new(caster, target, Shape, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction && !EndsOnCastEvent)
        {
            var count = CurrentBaits.Count;
            var id = caster.InstanceID;
            var baits = CollectionsMarshal.AsSpan(CurrentBaits);
            for (var i = 0; i < count; ++i)
            {
                ref var b = ref baits[i];
                if (b.Source.InstanceID == id)
                {
                    CurrentBaits.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == WatchedAction && EndsOnCastEvent)
        {
            var count = CurrentBaits.Count;
            var id = caster.InstanceID;
            var baits = CollectionsMarshal.AsSpan(CurrentBaits);
            for (var i = 0; i < count; ++i)
            {
                ref var b = ref baits[i];
                if (b.Source.InstanceID == id)
                {
                    CurrentBaits.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

// a variation of BaitAwayCast for charges that end at target
public class BaitAwayChargeCast(BossModule module, uint aid, float halfWidth) : GenericBaitAway(module, aid, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private readonly float HalfWidth = halfWidth;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction && WorldState.Actors.Find(spell.TargetID) is var target && target != null)
        {
            CurrentBaits.Add(new(caster, target, new AOEShapeRect((target.Position - caster.Position).Length(), HalfWidth), Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var count = CurrentBaits.Count;
            var id = caster.InstanceID;
            var baits = CollectionsMarshal.AsSpan(CurrentBaits);
            for (var i = 0; i < count; ++i)
            {
                ref var b = ref baits[i];
                if (b.Source.InstanceID == id)
                {
                    CurrentBaits.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void Update()
    {
        var count = CurrentBaits.Count;
        if (count == 0)
            return;
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        for (var i = 0; i < count; ++i)
        {
            ref var b = ref baits[i];
            var length = (b.Target.Position - b.Source.Position).Length();
            if (b.Shape is AOEShapeRect rect && rect.LengthFront != length)
            {
                b.Shape = new AOEShapeRect(length, HalfWidth);
            }
        }
    }
}

// a variation of baits with tethers for charges that end at target
public class BaitAwayChargeTether(BossModule module, float halfWidth, float activationDelay, uint aidGood, uint aidBad = default, uint tetherIDBad = 57u, uint tetherIDGood = 1u, uint enemyOID = default, float minimumDistance = default)
: StretchTetherDuo(module, minimumDistance, activationDelay, tetherIDBad, tetherIDGood, new AOEShapeRect(default, halfWidth), default, enemyOID)
{
    public readonly uint AidGood = aidGood;
    public readonly uint AidBad = aidBad; // supports 2nd AID incase the AID changes between good and bad tethers
    public readonly float HalfWidth = halfWidth;

    public override void Update()
    {
        base.Update();
        var count = CurrentBaits.Count;
        if (count == 0)
            return;
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        for (var i = 0; i < count; ++i)
        {
            ref var b = ref baits[i];
            var length = (b.Target.Position - b.Source.Position).Length();
            if (b.Shape is AOEShapeRect rect && rect.LengthFront != length)
            {
                b.Shape = new AOEShapeRect(length, HalfWidth);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == AidGood || spell.Action.ID == AidBad)
        {
            ++NumCasts;
            var count = CurrentBaits.Count;
            var id = spell.MainTargetID;
            var baits = CollectionsMarshal.AsSpan(CurrentBaits);
            for (var i = 0; i < count; ++i)
            {
                ref var b = ref baits[i];
                if (b.Target.InstanceID == id)
                {
                    CurrentBaits.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveBaits.Count == 0)
            return;
        base.AddHints(slot, actor, hints);
        var count = CurrentBaits.Count;
        var id = actor.InstanceID;
        for (var i = 0; i < count; ++i)
        {
            var b = CurrentBaits[i];
            if (b.Target.InstanceID != id)
            {
                continue;
            }
            if (PlayersClippedBy(ref b).Count != 0)
            {
                hints.Add(BaitAwayHint);
                return;
            }
        }
    }
}
