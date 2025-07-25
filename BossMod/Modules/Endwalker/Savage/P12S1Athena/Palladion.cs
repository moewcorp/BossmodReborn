﻿namespace BossMod.Endwalker.Savage.P12S1Athena;

// general mechanic tracking
sealed class Palladion(BossModule module) : BossComponent(module)
{
    public Actor?[] JumpTargets = new Actor?[PartyState.MaxPartySize];
    public Actor?[] Partners = new Actor?[PartyState.MaxPartySize];
    public BitMask BaitOrder; // bit i is set if i'th action is a bait rather than center aoe
    public int NumBaitsAssigned;
    public int NumBaitsDone;
    private readonly Dictionary<ulong, bool> _baitedLight = []; // key = instance id, value = true if bait, false if center aoe

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var order = Array.IndexOf(JumpTargets, actor);
        if (order >= 0)
            hints.Add($"Order: {order + 1}", false);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var (target, partner) = iconID switch
        {
            (uint)IconID.Palladion1 => (0, 2),
            (uint)IconID.Palladion2 => (1, 3),
            (uint)IconID.Palladion3 => (2, 0),
            (uint)IconID.Palladion4 => (3, 1),
            (uint)IconID.Palladion5 => (4, 6),
            (uint)IconID.Palladion6 => (5, 7),
            (uint)IconID.Palladion7 => (6, 4),
            (uint)IconID.Palladion8 => (7, 5),
            _ => (-1, -1)
        };
        if (target >= 0)
        {
            JumpTargets[target] = actor;
            Partners[partner] = actor;
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.Anthropos)
        {
            switch (id)
            {
                case 0x11D3:
                    _baitedLight[actor.InstanceID] = false;
                    break;
                case 0x11D4:
                    _baitedLight[actor.InstanceID] = true;
                    break;
                case 0x1E39: // add jumping away - staggered, 1s apart, order corresponds to center aoe/bait
                    if (_baitedLight.TryGetValue(actor.InstanceID, out var bait))
                        BaitOrder[NumBaitsAssigned++] = bait;
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ClearCut or (uint)AID.WhiteFlame)
        {
            ++NumBaitsDone;
        }
    }
}

// limited arena for limit cuts
// TODO: reconsider - base activation on env controls, show danger zone instead of border?..
sealed class PalladionArena(BossModule module) : BossComponent(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        for (var i = 0; i < 8; ++i)
            Arena.PathLineTo(Arena.Center + 14f * (i * 45f).Degrees().ToDirection());
        MiniArena.PathStroke(true, Colors.Border, 2);
    }
}

// shockwave is targeted at next jump target; everyone except target and partner should avoid it
sealed class PalladionShockwave(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Palladion? _palladion = module.FindComponent<Palladion>();
    private WPos _origin = module.PrimaryActor.Position;
    private DateTime _activation = module.CastFinishAt(module.PrimaryActor.CastInfo, 0.3d);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_palladion != null && NumCasts < _palladion.JumpTargets.Length && _palladion.JumpTargets[NumCasts] is var target && target != null && actor != target && actor != _palladion.Partners[NumCasts])
        {
            var toTarget = target.Position.Quantized() - _origin;
            return new AOEInstance[1] { new(new AOEShapeRect(toTarget.Length(), 2f), _origin, Angle.FromDirection(toTarget), _activation) };
        }
        return [];
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_palladion != null && NumCasts < _palladion.JumpTargets.Length && _palladion.JumpTargets[NumCasts] is var target && (target == actor || target == _palladion.Partners[NumCasts]) && actor.Position.InCircle(_origin, 23))
            hints.Add("Too close!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_palladion != null && NumCasts < _palladion.JumpTargets.Length && _palladion.JumpTargets[NumCasts] is var target && target != null && (pc == target || pc == _palladion.Partners[NumCasts]))
        {
            var toTarget = target.Position - _origin;
            Arena.AddRect(_origin, toTarget.Normalized(), toTarget.Length(), default, 2f, Colors.Safe);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        // note: shockwave cast happens at exactly the same time, but it targets a player rather than location, making it slightly harder to work with
        if (spell.Action.ID == (uint)AID.PalladionAOE)
        {
            _origin = spell.TargetXZ;
            _activation = WorldState.FutureTime(3d);
            ++NumCasts;
        }
    }
}

sealed class PalladionStack : Components.UniformStackSpread
{
    private int _numCasts;
    private readonly Palladion? _palladion;

    public PalladionStack(BossModule module) : base(module, 6, 0, raidwideOnResolve: false)
    {
        _palladion = module.FindComponent<Palladion>();
        UpdateStack(Module.CastFinishAt(Module.PrimaryActor.CastInfo, 0.3f));
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => IsStackTarget(player) ? PlayerPriority.Interesting : PlayerPriority.Normal;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.PalladionAOE)
        {
            ++_numCasts;
            UpdateStack(WorldState.FutureTime(3d));
        }
    }

    private void UpdateStack(DateTime activation)
    {
        Stacks.Clear();
        if (_palladion != null && _numCasts < _palladion.JumpTargets.Length && _palladion.JumpTargets[_numCasts] is var target && target != null)
            AddStack(target, activation, Raid.WithSlot(true, true, true).Exclude(_palladion.Partners[_numCasts]).Mask());
    }
}

sealed class PalladionVoidzone(BossModule module) : Components.VoidzoneAtCastTarget(module, 6f, (uint)AID.PalladionAOE, GetVoidzones, 0.9d)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.PalladionVoidzone);
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
}

sealed class PalladionClearCut(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Palladion? _palladion = module.FindComponent<Palladion>();

    private static readonly AOEShapeCircle _shape = new(4); // note: it's really a 270? degree cone, but we don't really know rotation early enough, and we just shouldn't stay in center anyway

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_palladion != null && _palladion.NumBaitsDone < _palladion.NumBaitsAssigned && !_palladion.BaitOrder[_palladion.NumBaitsDone])
            return new AOEInstance[1] { new(_shape, Arena.Center) };
        return [];
    }
}

// TODO: reconsider - show always, even if next is clear cut?..
sealed class PalladionWhiteFlame : Components.GenericBaitAway
{
    private readonly Palladion? _palladion;
    public static readonly WPos FakeActor = new(100f, 100f);
    private static readonly AOEShapeRect _shape = new(100f, 2f);

    public PalladionWhiteFlame(BossModule module) : base(module)
    {
        _palladion = module.FindComponent<Palladion>();
        UpdateBaiters();
    }

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_palladion != null && _palladion.NumBaitsDone < _palladion.NumBaitsAssigned && _palladion.BaitOrder[_palladion.NumBaitsDone])
            foreach (var t in Raid.WithoutSlot(false, true, true).SortedByRange(Arena.Center).Take(2))
                CurrentBaits.Add(new(FakeActor, t, _shape));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (NumCasts < 4 && !ForbiddenPlayers[slot])
            hints.Add("Bait next aoe", CurrentBaits.Count != 0 && ActiveBaitsOn(actor).Count == 0);
        base.AddHints(slot, actor, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (CurrentBaits.Count != 0)
            Arena.Actor(Arena.Center, default, Colors.Object);
        base.DrawArenaForeground(pcSlot, pc);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WhiteFlame)
        {
            ++NumCasts;
            UpdateBaiters();
        }
    }

    private void UpdateBaiters()
    {
        // TODO: we assume very strict bait order (5/7, 6/8, 1/3, 2/4), this is not strictly required...
        ForbiddenPlayers.Reset();
        if (_palladion != null && NumCasts < 4)
        {
            var (b1, b2) = NumCasts switch
            {
                0 => (4, 6),
                1 => (5, 7),
                2 => (0, 2),
                _ => (1, 3)
            };
            ForbiddenPlayers = Raid.WithSlot(true, true, true).Exclude(_palladion.JumpTargets[b1]).Exclude(_palladion.JumpTargets[b2]).Mask();
        }
    }
}

sealed class PalladionDestroyPlatforms(BossModule module) : Components.GenericAOEs(module, (uint)AID.PalladionDestroyPlatforms, "Go to safe platform!")
{
    private static readonly AOEShapeRect _shape = new(10f, 20f, 10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return new AOEInstance[1] { new(_shape, Arena.Center) };
    }
}
