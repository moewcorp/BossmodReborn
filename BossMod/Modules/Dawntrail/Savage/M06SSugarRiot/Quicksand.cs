namespace BossMod.Dawntrail.Savage.M06SSugarRiot;

sealed class Quicksand(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(23f);
    private static readonly AOEShapeCircle circleInvert = new(23f, true);

    public AOEInstance? AOE;
    private readonly QuicksandDoubleStyleHeavenBomb _baits1 = module.FindComponent<QuicksandDoubleStyleHeavenBomb>()!;
    private readonly QuicksandDoubleStylePaintBomb _baits2 = module.FindComponent<QuicksandDoubleStylePaintBomb>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (AOE is not AOEInstance aoe)
            return [];
        var countBaits1 = _baits1.Targets.Any();
        var countBaits2 = _baits2.Targets.Any();
        if (!countBaits1 && !countBaits2 || _baits1.Targets[slot])
            return Utils.ZeroOrOne(ref AOE);
        else if (_baits2.Targets[slot])
        {
            return CollectionsMarshal.AsSpan([aoe with { Shape = circleInvert, Color = Colors.SafeFromAOE, Risky = false }]);
        }
        return [];
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index is >= 0x1F and <= 0x23)
        {
            if (state == 0x00020001u)
            {
                var pos = index switch
                {
                    0x1F => Arena.Center,
                    0x20 => new(100f, 80f),
                    0x21 => new(100f, 120f),
                    0x22 => new(120f, 100f),
                    0x23 => new(80f, 100f),
                    _ => default
                };
                if (pos != default)
                    AOE = new(circle, pos, default, WorldState.FutureTime(6d));
            }
            else if (state == 0x00080004u)
            {
                AOE = null;
            }
        }
    }
}

sealed class QuicksandDoubleStylePaintBomb(BossModule module) : BossComponent(module)
{
    public BitMask Targets;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.ActivateMechanicDoubleStyle2)
        {
            Targets[Raid.FindSlot(source.InstanceID)] = true;
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.PaintBomb && id == 0x11D1)
        {
            Targets = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Targets[slot])
            hints.Add("Place bomb inside quicksand");
    }
}

sealed class QuicksandDoubleStyleHeavenBomb(BossModule module) : Components.GenericKnockback(module, default, true, stopAfterWall: true)
{
    public BitMask Targets;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (Targets[slot])
            return new Knockback[1] { new(actor.Position, 16f, default, default, actor.Rotation, Kind.DirForward) };
        return [];
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.ActivateMechanicDoubleStyle1)
        {
            Targets[Raid.FindSlot(source.InstanceID)] = true;
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.HeavenBomb && id == 0x11D1)
        {
            Targets = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var movements = CalculateMovements(slot, actor);
        if (movements.Count != 0)
            hints.Add("Aim bomb into quicksand!", DestinationUnsafe(slot, actor, movements[0].to));
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (!Module.InBounds(pos))
            return true;
        var comp = Module.FindComponent<Quicksand>();
        if (comp != null)
        {
            var aoes = comp.ActiveAOEs(slot, actor);
            var len = aoes.Length;
            for (var i = 0; i < len; ++i)
            {
                if (!aoes[i].Check(pos))
                    return true;
            }
        }
        return false;
    }
}

sealed class PaintBombHeavenBomb(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Burst1, (uint)AID.Burst2], 10f);
sealed class PuddingGraf(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.PuddingGraf, 6f);
