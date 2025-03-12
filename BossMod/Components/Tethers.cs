﻿namespace BossMod.Components;

// generic component for tankbuster at tethered targets; tanks are supposed to intercept tethers and gtfo from the raid
public class TankbusterTether(BossModule module, ActionID aid, uint tetherID, float radius) : CastCounter(module, aid)
{
    public readonly uint TID = tetherID;
    public readonly float Radius = radius;
    private readonly List<(Actor Player, Actor Enemy)> _tethers = [];
    private BitMask _tetheredPlayers;
    private BitMask _inAnyAOE; // players hit by aoe, excluding selves

    public bool Active => _tetheredPlayers.Any();

    public override void Update()
    {
        _inAnyAOE = new();
        foreach (var slot in _tetheredPlayers.SetBits())
        {
            var target = Raid[slot];
            if (target != null)
                _inAnyAOE |= Raid.WithSlot().InRadiusExcluding(target, Radius).Mask();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Active)
            return;

        if (actor.Role == Role.Tank)
        {
            if (!_tetheredPlayers[slot])
            {
                hints.Add("Grab the tether!");
            }
            else if (Raid.WithoutSlot().InRadiusExcluding(actor, Radius).Any())
            {
                hints.Add("GTFO from raid!");
            }
        }
        else
        {
            if (_tetheredPlayers[slot])
            {
                hints.Add("Hit by tankbuster");
            }
            if (_inAnyAOE[slot])
            {
                hints.Add("GTFO from tanks!");
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (_tetheredPlayers[playerSlot])
            return PlayerPriority.Danger;

        // for tanks, other players are interesting, since tank should not clip them
        if (pc.Role == Role.Tank)
            return _inAnyAOE[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Normal;

        // for non-tanks, other players are irrelevant
        return PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // show tethered targets with circles
        for (var i = 0; i < _tethers.Count; ++i)
        {
            var side = _tethers[i];
            Arena.AddLine(side.Enemy.Position, side.Player.Position, side.Player.Role == Role.Tank ? Colors.Safe : 0);
            Arena.AddCircle(side.Player.Position, Radius);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var sides = DetermineTetherSides(source, tether);
        if (sides != null)
        {
            _tethers.Add((sides.Value.Player, sides.Value.Enemy));
            _tetheredPlayers.Set(sides.Value.PlayerSlot);
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        var sides = DetermineTetherSides(source, tether);
        if (sides != null)
        {
            _tethers.Remove((sides.Value.Player, sides.Value.Enemy));
            _tetheredPlayers.Clear(sides.Value.PlayerSlot);
        }
    }

    // we support both player->enemy and enemy->player tethers
    private (int PlayerSlot, Actor Player, Actor Enemy)? DetermineTetherSides(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID != TID)
            return null;

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
            return null;

        var (player, enemy) = Raid.WithoutSlot().Contains(source) ? (source, target) : (target, source);
        var playerSlot = Raid.FindSlot(player.InstanceID);
        return (playerSlot, player, enemy);
    }
}

// generic component for AOE at tethered targets; players are supposed to intercept tethers and gtfo from the raid
public class InterceptTetherAOE(BossModule module, ActionID aid, uint tetherID, float radius, uint[]? excludedAllies = null) : CastCounter(module, aid)
{
    public readonly uint[]? ExcludedAllies = excludedAllies;
    public readonly uint TID = tetherID;
    public readonly float Radius = radius;
    public readonly List<(Actor Player, Actor Enemy)> Tethers = [];
    private BitMask _tetheredPlayers;
    private BitMask _inAnyAOE; // players hit by aoe, excluding selves
    public DateTime Activation;

    public bool Active => Tethers.Count != 0;

    public override void Update()
    {
        _inAnyAOE = new();
        foreach (var slot in _tetheredPlayers.SetBits())
        {
            var target = Raid[slot];
            if (target != null)
                _inAnyAOE |= Raid.WithSlot().InRadiusExcluding(target, Radius).Mask();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Active)
            return;
        if (!_tetheredPlayers[slot])
        {
            hints.Add("Grab the tether!");
        }
        else if (Raid.WithoutSlot().InRadiusExcluding(actor, Radius).Any())
        {
            hints.Add("GTFO from raid!");
        }
        else
        {
            if (_tetheredPlayers[slot])
            {
                hints.Add("Hit by baited AOE");
            }
            if (_inAnyAOE[slot])
            {
                hints.Add("GTFO from baited AOE!");
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Tethers.Count;
        if (count == 0)
            return;
        var raid = Raid.WithoutSlot();
        for (var i = 0; i < count; ++i)
        {
            var tether = Tethers[i];
            if (tether.Player != actor)
                hints.AddForbiddenZone(ShapeDistance.Circle(tether.Player.Position, Radius), Activation);
            else
                for (var j = 0; j < raid.Length; ++j)
                {
                    ref var member = ref raid[i];
                    if (member != actor)
                        hints.AddForbiddenZone(ShapeDistance.Circle(member.Position, Radius), Activation);
                }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // show tethered targets with circles
        var count = Tethers.Count;
        if (count == 0)
            return;
        var len = ExcludedAllies?.Length;
        var exclude = new List<Actor>(len ?? 0);
        if (ExcludedAllies != null)
            for (var i = 0; i < len; ++i)
                exclude.AddRange(Module.Enemies(ExcludedAllies[i]));
        for (var i = 0; i < count; ++i)
        {
            var side = Tethers[i];
            Arena.AddLine(side.Enemy.Position, side.Player.Position, Raid.WithoutSlot().Exclude(exclude).Contains(side.Player) ? Colors.Safe : 0);
            Arena.AddCircle(side.Player.Position, Radius);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var sides = DetermineTetherSides(source, tether);
        if (sides != null)
        {
            Tethers.Add((sides.Value.Player, sides.Value.Enemy));
            _tetheredPlayers.Set(sides.Value.PlayerSlot);
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        var sides = DetermineTetherSides(source, tether);
        if (sides != null)
        {
            Tethers.Remove((sides.Value.Player, sides.Value.Enemy));
            _tetheredPlayers.Clear(sides.Value.PlayerSlot);
        }
    }

    // we support both player->enemy and enemy->player tethers
    private (int PlayerSlot, Actor Player, Actor Enemy)? DetermineTetherSides(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID != TID)
            return null;
        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
            return null;
        var (player, enemy) = Raid.WithoutSlot().Contains(source) ? (source, target) : (target, source);
        var playerSlot = Raid.FindSlot(player.InstanceID);
        return (playerSlot, player, enemy);
    }
}

// generic component for tethers that need to be intercepted eg. to prevent a boss from gaining buffs
public class InterceptTether(BossModule module, ActionID aid, uint tetherIDBad = 84, uint tetherIDGood = 17, uint[]? excludedAllies = null) : CastCounter(module, aid)
{
    public readonly uint TIDGood = tetherIDGood;
    public readonly uint TIDBad = tetherIDBad;
    public readonly uint[]? ExcludedAllies = excludedAllies;
    private readonly List<(Actor Player, Actor Enemy)> _tethers = [];
    private BitMask _tetheredPlayers;
    private const string hint = "Grab the tether!";
    public bool Active => _tethers.Count != 0;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Active)
            return;
        if (!_tetheredPlayers[slot])
        {
            hints.Add(hint);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!Active)
            return;
        var len = ExcludedAllies?.Length;
        var exclude = new List<Actor>(len ?? 0);
        if (ExcludedAllies != null)
            for (var i = 0; i < len; ++i)
                exclude.AddRange(Module.Enemies(ExcludedAllies[i]));
        for (var i = 0; i < _tethers.Count; ++i)
        {
            var side = _tethers[i];
            Arena.AddLine(side.Enemy.Position, side.Player.Position, Raid.WithoutSlot().Exclude(exclude).Contains(side.Player) ? Colors.Safe : 0);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var sides = DetermineTetherSides(source, tether);
        if (sides != null)
        {
            _tethers.Add((sides.Value.Player, sides.Value.Enemy));
            _tetheredPlayers.Set(sides.Value.PlayerSlot);
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        var sides = DetermineTetherSides(source, tether);
        if (sides != null)
        {
            _tethers.Remove((sides.Value.Player, sides.Value.Enemy));
            _tetheredPlayers.Clear(sides.Value.PlayerSlot);
        }
    }

    private (int PlayerSlot, Actor Player, Actor Enemy)? DetermineTetherSides(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID != TIDGood && tether.ID != TIDBad)
            return null;

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
            return null;

        var (player, enemy) = Raid.WithoutSlot().Contains(source) ? (source, target) : (target, source);
        var playerSlot = Raid.FindSlot(player.InstanceID);
        return (playerSlot, player, enemy);
    }
}

// generic component for tethers that need to be stretched and switch between a "good" and "bad" tether
// at the end of the mechanic various things are possible, eg. single target dmg, knockback/pull, AOE etc.
public class StretchTetherDuo(BossModule module, float minimumDistance, float activationDelay, uint tetherIDBad = 57, uint tetherIDGood = 1, AOEShape? shape = null, ActionID aid = default, uint enemyOID = default, bool knockbackImmunity = false) : GenericBaitAway(module, aid)
{
    public readonly AOEShape? Shape = shape;
    public readonly uint TIDGood = tetherIDGood;
    public readonly uint TIDBad = tetherIDBad;
    public readonly float MinimumDistance = minimumDistance;
    public readonly bool KnockbackImmunity = knockbackImmunity;
    public readonly List<Actor> _enemies = module.Enemies(enemyOID);
    public readonly List<(Actor, uint)> TetherOnActor = [];
    public readonly List<(Actor, DateTime)> ActivationDelayOnActor = [];
    public readonly float ActivationDelay = activationDelay;
    public const string HintGood = "Tether is stretched!";
    public const string HintBad = "Stretch tether further!";
    public const string HintKnockbackImmmunityGood = "Immune against tether mechanic!";
    public const string HintKnockbackImmmunityBad = "Tether can be ignored with knockback immunity!";

    protected struct PlayerImmuneState
    {
        public DateTime RoleBuffExpire; // 0 if not active
        public DateTime JobBuffExpire; // 0 if not active
        public DateTime DutyBuffExpire; // 0 if not active

        public readonly bool ImmuneAt(DateTime time) => RoleBuffExpire > time || JobBuffExpire > time || DutyBuffExpire > time;
    }

    protected PlayerImmuneState[] PlayerImmunes = new PlayerImmuneState[PartyState.MaxAllies];

    public bool IsImmune(int slot, DateTime time) => KnockbackImmunity && PlayerImmunes[slot].ImmuneAt(time);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            switch (status.ID)
            {
                case 3054: //Guard in PVP
                case (uint)WHM.SID.Surecast:
                case (uint)WAR.SID.ArmsLength:
                    PlayerImmunes[slot].RoleBuffExpire = status.ExpireAt;
                    break;
                case 1722: //Bluemage Diamondback
                case (uint)WAR.SID.InnerStrength:
                    PlayerImmunes[slot].JobBuffExpire = status.ExpireAt;
                    break;
                case 2345: //Lost Manawall in Bozja
                    PlayerImmunes[slot].DutyBuffExpire = status.ExpireAt;
                    break;
            }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            switch (status.ID)
            {
                case 3054: //Guard in PVP
                case (uint)WHM.SID.Surecast:
                case (uint)WAR.SID.ArmsLength:
                    PlayerImmunes[slot].RoleBuffExpire = default;
                    break;
                case 1722: //Bluemage Diamondback
                case (uint)WAR.SID.InnerStrength:
                    PlayerImmunes[slot].JobBuffExpire = default;
                    break;
                case 2345: //Lost Manawall in Bozja
                    PlayerImmunes[slot].DutyBuffExpire = default;
                    break;
            }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (ActiveBaits.Count == 0)
            return;
        if (!IsImmune(pcSlot, ActiveBaits.FirstOrDefault(x => x.Target == pc).Activation))
        {
            if (IsTether(pc, TIDBad))
                DrawTetherLines(pc);
            else if (IsTether(pc, TIDGood))
                DrawTetherLines(pc, Colors.Safe);
        }
    }

    protected bool IsTether(Actor actor, uint tetherID) => TetherOnActor.Contains((actor, tetherID));

    private void DrawTetherLines(Actor target, uint color = 0)
    {
        foreach (var bait in ActiveBaits.Where(x => x.Target == target))
            Arena.AddLine(bait.Source.Position, bait.Target.Position, color);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var (player, enemy) = DetermineTetherSides(source, tether);
        if (player != null && enemy != null && (enemyOID == default || _enemies.Contains(source)))
        {
            if (!ActivationDelayOnActor.Any(x => x.Item1 == player))
                ActivationDelayOnActor.Add((player, WorldState.FutureTime(ActivationDelay)));
            CurrentBaits.Add(new(enemy, player, Shape ?? new AOEShapeCircle(default), ActivationDelayOnActor.FirstOrDefault(x => x.Item1 == player).Item2));
            TetherOnActor.Add((player, tether.ID));
        }
    }

    public override void Update()
    {
        var count = ActivationDelayOnActor.Count;
        if (count > 0)
        {
            var actorsToRemove = new List<(Actor, DateTime)>();
            for (var i = 0; i < count; ++i)
            {
                var a = ActivationDelayOnActor[i];
                if (a.Item2.AddSeconds(1) <= WorldState.CurrentTime)
                    actorsToRemove.Add(a);
            }
            for (var i = 0; i < actorsToRemove.Count; ++i)
                ActivationDelayOnActor.Remove(actorsToRemove[i]);
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        var (player, enemy) = DetermineTetherSides(source, tether);
        if (player != null && enemy != null)
        {
            CurrentBaits.RemoveAll(b => b.Source == enemy && b.Target == player);
            TetherOnActor.Remove((WorldState.Actors.Find(tether.Target)!, tether.ID));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count == 0)
            return;
        var immunity = IsImmune(slot, ActiveBaits.FirstOrDefault(x => x.Target == actor).Activation);
        var bait = ActiveBaits.Any(x => x.Target == actor);
        if (immunity && bait)
            hints.Add(HintKnockbackImmmunityGood, false);
        else if (TetherOnActor.Contains((actor, TIDBad)))
            hints.Add(HintBad);
        else if (TetherOnActor.Contains((actor, TIDGood)))
            hints.Add(HintGood, false);
        if (KnockbackImmunity && bait && !immunity)
            hints.Add(HintKnockbackImmmunityBad);
    }

    public (Actor? player, Actor? enemy) DetermineTetherSides(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID != TIDGood && tether.ID != TIDBad)
            return (null, null);

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
            return (null, null);

        var (player, enemy) = Raid.WithoutSlot().Contains(source) ? (source, target) : (target, source);
        return (player, enemy);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveBaits.Count == 0)
            return;
        var immunity = IsImmune(slot, ActiveBaits.FirstOrDefault(x => x.Target == actor).Activation);
        var isImmune = immunity && KnockbackImmunity;
        var couldBeImmune = !immunity && KnockbackImmunity;
        if (couldBeImmune && ActivationDelayOnActor.Any(x => x.Item1 == actor && x.Item2.AddSeconds(-6d) <= WorldState.CurrentTime))
        {
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.ArmsLength), actor, ActionQueue.Priority.High);
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Surecast), actor, ActionQueue.Priority.High);
        }
        if (Shape != null)
            base.AddAIHints(slot, actor, assignment, hints);
        if (ActiveBaits.Any(x => x.Target == actor) && !isImmune)
            foreach (var b in ActiveBaits.Where(x => x.Target == actor))
                hints.AddForbiddenZone(ShapeDistance.Circle(b.Source.Position, MinimumDistance), b.Activation);
    }
}

// generic component for tethers that need to be stretched
public class StretchTetherSingle(BossModule module, uint tetherID, float minimumDistance, AOEShape? shape = null, ActionID aid = default, uint enemyOID = default, float activationDelay = default, bool knockbackImmunity = false, bool needToKite = false) :
StretchTetherDuo(module, minimumDistance, activationDelay, tetherID, tetherID, shape, aid, enemyOID, knockbackImmunity)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveBaits.Count == 0)
            return;
        if (needToKite && TetherOnActor.Contains((actor, TIDBad)))
            hints.Add("Kite the add!");
        else
            base.AddHints(slot, actor, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (needToKite && IsTether(pc, TIDBad))
            Arena.Actor(ActiveBaits.FirstOrDefault(x => x.Target == pc).Source, Colors.Object, true);
    }
}
