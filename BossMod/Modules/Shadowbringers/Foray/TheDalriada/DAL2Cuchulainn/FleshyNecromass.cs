namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL2Cuchulainn;

sealed class FleshNecromass(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly WPos[] positions = [new(637.27f, -174.667f), new(662.71f, -174.667f), new(662.71f, -200.133f), new(637.27f, -200.133f)];
    public static readonly AOEShapeCustom Circles = VoidzoneShape();
    private AOEInstance[] _voidzone = [];
    private AOEInstance? _invertedvoidzone;
    private BitMask gelatinous;
    private bool active;

    private static AOEShapeCustom VoidzoneShape()
    {
        var circles = new Polygon[4];
        var circle = new Polygon(default, 5.676f, 40);
        for (var i = 0; i < 4; ++i)
        {
            circles[i] = circle with { Center = positions[i] };
        }
        return new(circles);
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (active && !gelatinous[slot])
        {
            return Utils.ZeroOrOne(ref _invertedvoidzone);
        }
        else
        {
            return _voidzone;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Gelatinous)
        {
            gelatinous[Raid.FindSlot(actor.InstanceID)] = true;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Gelatinous)
        {
            gelatinous[Raid.FindSlot(actor.InstanceID)] = false;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (_voidzone.Length == 0 && id == (uint)AID.PutrifiedSoul1)
        {
            _voidzone = [new(Circles, Arena.Center, default, Module.CastFinishAt(spell, 0.2d))];
        }
        else if (id == (uint)AID.FleshyNecromassVisual)
        {
            active = true;
            _invertedvoidzone = new(Circles with { InvertForbiddenZone = true }, Arena.Center, default, Module.CastFinishAt(spell), Colors.SafeFromAOE);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (active && spell.Action.ID is (uint)AID.FleshyNecromassJump1 or (uint)AID.FleshyNecromassJump2)
        {
            if (++NumCasts == 5) // boss does 7 casts, but if morphed early, the buff runs out before mechanic ends and AI might morph again for just 1-2 extra hits
            {
                NumCasts = 0;
                active = false;
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (active && !gelatinous[slot])
        {
            hints.Add("Move into a voidzone to turn into pudding!");
        }
        else
        {
            base.AddHints(slot, actor, hints);
        }
    }
}

sealed class FleshNecromassJumps(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCircle circle = new(12f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id is (uint)AID.FleshyNecromassJump1 or (uint)AID.FleshyNecromassJump2)
        {
            // unfortunately the jump locations seem to be RNG, so there is only about 1s time to do a tiny position correction
            _aoe = new(circle, spell.TargetXZ, default, WorldState.FutureTime(1d));
        }
        else if (id == (uint)AID.FleshyNecromass1)
        {
            _aoe = null;
        }
    }
}
