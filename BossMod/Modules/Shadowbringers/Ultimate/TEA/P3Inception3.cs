namespace BossMod.Shadowbringers.Ultimate.TEA;

// note: boss moves to position around the component activation time
[SkipLocalsInit]
sealed class P3Inception3Sacrament(BossModule module) : Components.GenericAOEs(module, (uint)AID.SacramentInception)
{
    public bool Active => _source != null;

    private AOEInstance[] _aoe = [];
    private readonly Actor? _source = ((TEA)module).AlexPrime();
    private readonly Actor? _source2 = ((TEA)module).TrueHeart();
    private readonly AOEShapeCross cross = new(100f, 8f);
    private WPos initialPos;
    private DateTime activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe.Length == 0 && initialPos != default && initialPos != _source!.Position) // only relevant if one or more crystals didn't get destroyed
        {
            return new AOEInstance[1] { new(cross, _source.Position, _source.Rotation, activation) };
        }
        return _aoe;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Inception) // only relevant if one or more crystals didn't get destroyed
        {
            initialPos = caster.Position;
            activation = Module.CastFinishAt(spell, 8.3d);
        }
    }

    public override void Update()
    {
        if (_source2 != null && _aoe.Length == 0)
        {
            var last = _source2.LastFrameMovement;
            if (last != default && last.LengthSq() > 1e-3f)
            {
                _ = Intersect.RayCircleAnglesDeg(Arena.Center, 20f, _source2.Position, last, out _, out var exit);
                var angle = (int)(Math.Round(exit / 10f) * 10f);
                (WPos loc, Angle angle) pos = angle switch
                {
                    -160 => (new(91.772f, 80.137f), 22.498f.Degrees()),
                    160 => (new(108.228f, 80.137f), -22.503f.Degrees()),
                    110 => (new(119.863f, 91.772f), -67.504f.Degrees()),
                    -110 => (new(80.137f, 91.772f), 67.448f.Degrees()),
                    -20 => (new(91.772f, 119.863f), 157.5f.Degrees()),
                    20 => (new(108.228f, 119.863f), -157.505f.Degrees()),
                    70 => (new(119.863f, 108.228f), -112.504f.Degrees()),
                    -70 => (new(80.137f, 108.228f), 112.499f.Degrees()),
                    _ => default
                };
                if (pos == default)
                {
                    return;
                }
                var loc = pos.loc;
                var dir = pos.angle;
                _aoe = [new(cross, loc, dir, WorldState.FutureTime(6.9d), shapeDistance: cross.Distance(loc, dir))];
            }
        }
    }
}

[SkipLocalsInit]
sealed class P3Inception3Debuffs(BossModule module) : Components.GenericStackSpread(module)
{
    private Actor? _sharedSentence;
    private BitMask _avoid;
    private BitMask _tethered;
    private bool _inited; // we init stuff on first update, since component is activated when statuses are already applied

    public override void Update()
    {
        if (!_inited)
        {
            _inited = true;
            if (_sharedSentence != null)
                Stacks.Add(new(_sharedSentence, 4f, 3, int.MaxValue, WorldState.FutureTime(4d), _avoid));
        }
        base.Update();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_tethered[slot] && FindPartner(slot) is var partner && partner != null && (partner.Position - actor.Position).LengthSq() < 900f)
            hints.Add("Stay farther from partner!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (_tethered[pcSlot] && FindPartner(pcSlot) is var partner && partner != null)
            Arena.AddLine(pc.Position, partner.Position, (partner.Position - pc.Position).LengthSq() < 900f ? default : Colors.Safe);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.AggravatedAssault:
                _avoid.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.SharedSentence:
                _sharedSentence = actor;
                break;
            case (uint)SID.RestrainingOrder:
                _tethered.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    private Actor? FindPartner(int slot)
    {
        var remaining = _tethered;
        remaining[slot] = false;
        return remaining.Any() ? Raid[remaining.LowestSetBit()] : null;
    }
}
