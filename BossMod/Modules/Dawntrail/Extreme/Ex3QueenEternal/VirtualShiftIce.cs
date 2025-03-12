﻿namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

class VirtualShiftIce(BossModule module) : Components.GenericAOEs(module, default, "GTFO from broken bridge!")
{
    private readonly List<AOEInstance> _unsafeBridges = new(4);
    private readonly List<Rectangle> _destroyedBridges = [new(new(95f, 96f), 3f, 2f), new(new(95f, 104f), 3f, 2f), new(new(105f, 96f), 3f, 2f), new(new(95f, 104f), 3f, 2f)];

    private static readonly AOEShapeRect _shape = new(2, 3, 2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_unsafeBridges);

    public override void OnEventEnvControl(byte index, uint state)
    {
        WDir offset = index switch
        {
            0x04 => new(-5f, -4f),
            0x05 => new(-5f, +4f),
            0x06 => new(+5f, -4f),
            0x07 => new(+5f, +4f),
            _ => default
        };
        if (offset == default)
            return;

        var center = Ex3QueenEternal.ArenaCenter + offset;
        switch (state)
        {
            case 0x00020001: // destroyed bridge respawns
                _destroyedBridges.RemoveAll(s => s.Center == center);
                UpdateArena();
                break;
            case 0x00200010: // bridge gets damaged
                _unsafeBridges.Add(new(_shape, center));
                break;
            case 0x00400001: // damaged bridge gets repaired
            case 0x00080004: // bridges despawn
                RemoveUnsafeBridges();
                break;
            case 0x00800004: // bridge gets destroyed
                RemoveUnsafeBridges();
                _destroyedBridges.Add(new(center, 3, 2));
                UpdateArena();
                break;
        }

        void RemoveUnsafeBridges() => _unsafeBridges.RemoveAll(s => s.Origin == center);
        void UpdateArena() => Arena.Bounds = new ArenaBoundsComplex(Ex3QueenEternal.IceRectsAll, [.. _destroyedBridges]);
    }
}

class LawsOfIce(BossModule module) : Components.StayMove(module)
{
    public int NumCasts;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.LawsOfIce)
            SetState(Raid.FindSlot(actor.InstanceID), new(Requirement.Move, WorldState.FutureTime(4.2d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LawsOfIceAOE)
            ++NumCasts;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.FreezingUp)
            ClearState(Raid.FindSlot(actor.InstanceID));
    }
}

class Rush(BossModule module) : Components.GenericBaitAway(module)
{
    public DateTime Activation;
    private BitMask _unstretched;
    private readonly Ex3QueenEternalConfig _config = Service.Config.Get<Ex3QueenEternalConfig>();

    private static readonly AOEShapeRect _shapeTether = new(80f, 2f);
    private static readonly AOEShapeCircle _shapeUntethered = new(8f); // if there is no tether, pillar will just explode; this can happen if someone is dead

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_unstretched[slot])
            hints.Add("Stretch tether!");
        base.AddHints(slot, actor, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        for (var i = 0; i < CurrentBaits.Count; ++i)
        {
            var b = CurrentBaits[i];
            Arena.Actor(b.Source, Colors.Object, true);
            if (b.Target == pc)
            {
                Arena.AddLine(b.Source.Position, b.Target.Position, _unstretched[pcSlot] ? 0 : Colors.Safe);
                Arena.AddCircle(SafeSpot(b.Source, _config), 1f, Colors.Safe);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.RushFirst or (uint)AID.RushSecond)
        {
            Activation = Module.CastFinishAt(spell, 0.2f);
            if (!CurrentBaits.Any(b => b.Source == caster))
                CurrentBaits.Add(new(caster, caster, _shapeUntethered, Activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RushFirstAOE or (uint)AID.RushSecondAOE or (uint)AID.RushFirstFail or (uint)AID.RushSecondFail)
            ++NumCasts;
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.RushShort or (uint)TetherID.RushLong && WorldState.Actors.Find(tether.Target) is var target && target != null)
        {
            CurrentBaits.RemoveAll(b => b.Source == source);
            CurrentBaits.Add(new(source, target, _shapeTether, Activation));

            var slot = Raid.FindSlot(tether.Target);
            if (slot >= 0)
                _unstretched[slot] = tether.ID == (uint)TetherID.RushShort;
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.RushShort or (uint)TetherID.RushLong)
        {
            CurrentBaits.RemoveAll(b => b.Source == source);
            CurrentBaits.Add(new(source, source, _shapeUntethered, Activation));

            _unstretched.Clear(Raid.FindSlot(tether.Target));
        }
    }

    private static WPos SafeSpot(Actor source, Ex3QueenEternalConfig config)
    {
        var center = Ex3QueenEternal.ArenaCenter;
        var safeSide = source.Position.X > center.X ? -1 : +1;
        var offX = Math.Abs(source.Position.X - center.X);
        if (source.Position.Z > 110f)
        {
            // first order
            var inner = offX < 6f;
            return center + new WDir(safeSide * (inner ? 15f : 10f), -19f);
        }
        else
        {
            // second order
            var central = source.Position.Z < 96f;
            var strat = !config.SideTethersCrossStrategy ? (central ? -2f : 9f) : (central ? 9f : -9f);
            return center + new WDir(safeSide * 15f, strat);
        }
    }
}

class IceDart(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCircle(16), (uint)TetherID.IceDart, ActionID.MakeSpell(AID.IceDart), centerAtTarget: true)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            ForbiddenPlayers[Raid.FindSlot(spell.MainTargetID)] = true;
        }
    }
}

class RaisedTribute(BossModule module) : Components.GenericWildCharge(module, 4, ActionID.MakeSpell(AID.RaisedTribute), 80f)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.RaisedTribute)
        {
            Source = actor;
            var party = Raid.WithoutSlot(true, true, true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                ref var member = ref party[i];
                PlayerRoles[i] = member.InstanceID == targetID ? PlayerRole.Target : member.Tether.ID != 0 ? PlayerRole.Avoid : PlayerRole.Share;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            Source = null;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.IceDart && Raid.FindSlot(source.InstanceID) is var slot && slot >= 0 && PlayerRoles[slot] != PlayerRole.Target)
            PlayerRoles[slot] = PlayerRole.Avoid;
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.IceDart && Raid.FindSlot(source.InstanceID) is var slot && slot >= 0 && PlayerRoles[slot] != PlayerRole.Target)
            PlayerRoles[slot] = PlayerRole.Share;
    }
}
