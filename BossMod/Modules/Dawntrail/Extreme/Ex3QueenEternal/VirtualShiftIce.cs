namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

class VirtualShiftIce(BossModule module) : Components.GenericAOEs(module, default, "GTFO from broken bridge!")
{
    private readonly List<AOEInstance> _unsafeBridges = [];
    private readonly List<Rectangle> _destroyedBridges = [];

    private static readonly AOEShapeRect _shape = new(2, 3, 2);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _unsafeBridges;

    public override void OnEventEnvControl(byte index, uint state)
    {
        WDir offset = index switch
        {
            4 => new(-5, -4),
            5 => new(-5, +4),
            6 => new(+5, -4),
            7 => new(+5, +4),
            _ => default
        };
        if (offset == default)
            return;

        var center = Ex3QueenEternal.ArenaCenter + offset;
        switch (state)
        {
            case 0x00200010:
                _unsafeBridges.Add(new(_shape, center));
                break;
            case 0x00400001:
                _unsafeBridges.RemoveAll(s => s.Origin == center);
                break;
            case 0x00800004:
                _unsafeBridges.RemoveAll(s => s.Origin == center);
                _destroyedBridges.Add(new(center, 3, 2));
                Arena.Bounds = new ArenaBoundsComplex(Ex3QueenEternal.IceRectsAll, [.. _destroyedBridges], Offset: Trial.T03QueenEternal.T03QueenEternal.OffSet);
                break;
        }
    }
}

class LawsOfIce(BossModule module) : Components.StayMove(module)
{
    public int NumCasts;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.LawsOfIce)
            SetState(Raid.FindSlot(actor.InstanceID), new(Requirement.Move, WorldState.FutureTime(4.2f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LawsOfIceAOE)
            ++NumCasts;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.FreezingUp)
            ClearState(Raid.FindSlot(actor.InstanceID));
    }
}

class Rush(BossModule module) : Components.GenericBaitAway(module)
{
    public DateTime Activation;
    private BitMask _unstretched;

    private static readonly AOEShapeRect _shapeTether = new(80, 2);
    private static readonly AOEShapeCircle _shapeUntethered = new(8); // if there is no tether, pillar will just explode; this can happen if someone is dead

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
                Arena.AddLine(b.Source.Position, b.Target.Position, _unstretched[pcSlot] ? Colors.Danger : Colors.Safe);
                Arena.AddCircle(SafeSpot(b.Source), 1, Colors.Safe);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.RushFirst or AID.RushSecond)
        {
            Activation = Module.CastFinishAt(spell, 0.2f);
            if (!CurrentBaits.Any(b => b.Source == caster))
                CurrentBaits.Add(new(caster, caster, _shapeUntethered, Activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RushFirstAOE or AID.RushSecondAOE or AID.RushFirstFail or AID.RushSecondFail)
            ++NumCasts;
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.RushShort or TetherID.RushLong && WorldState.Actors.Find(tether.Target) is var target && target != null)
        {
            CurrentBaits.RemoveAll(b => b.Source == source);
            CurrentBaits.Add(new(source, target, _shapeTether, Activation));

            var slot = Raid.FindSlot(tether.Target);
            if (slot >= 0)
                _unstretched[slot] = (TetherID)tether.ID == TetherID.RushShort;
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.RushShort or TetherID.RushLong)
        {
            CurrentBaits.RemoveAll(b => b.Source == source);
            CurrentBaits.Add(new(source, source, _shapeUntethered, Activation));

            _unstretched.Clear(Raid.FindSlot(tether.Target));
        }
    }

    private WPos SafeSpot(Actor source)
    {
        var center = Ex3QueenEternal.ArenaCenter;
        var safeSide = source.Position.X > center.X ? -1 : +1;
        var offX = Math.Abs(source.Position.X - center.X);
        if (source.Position.Z > 110)
        {
            // first order
            var inner = offX < 6;
            return center + new WDir(safeSide * (inner ? 15 : 10), -19);
        }
        else
        {
            // second order
            var central = source.Position.Z < 96;
            return center + new WDir(safeSide * 15, central ? -2 : 10);
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
            ForbiddenPlayers.Set(Raid.FindSlot(spell.MainTargetID));
        }
    }
}

class RaisedTribute(BossModule module) : Components.GenericWildCharge(module, 4, ActionID.MakeSpell(AID.RaisedTribute), 80)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.RaisedTribute)
        {
            Source = actor;
            foreach (var (i, p) in Raid.WithSlot(true))
            {
                PlayerRoles[i] = p.InstanceID == targetID ? PlayerRole.Target : p.Tether.ID != 0 ? PlayerRole.Avoid : PlayerRole.Share;
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
