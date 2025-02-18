namespace BossMod.Dawntrail.Alliance.A12Fafnir;

class HurricaneWingRaidwide(BossModule module) : Components.CastCounterMulti(module, [ActionID.MakeSpell(AID.HurricaneWingRaidwideAOE1), ActionID.MakeSpell(AID.HurricaneWingRaidwideAOE2), ActionID.MakeSpell(AID.HurricaneWingRaidwideAOE3),
    ActionID.MakeSpell(AID.HurricaneWingRaidwideAOE4), ActionID.MakeSpell(AID.HurricaneWingRaidwideAOE5), ActionID.MakeSpell(AID.HurricaneWingRaidwideAOE6),
    ActionID.MakeSpell(AID.HurricaneWingRaidwideAOE7), ActionID.MakeSpell(AID.HurricaneWingRaidwideAOE8), ActionID.MakeSpell(AID.HurricaneWingRaidwideAOE9)]);

class HurricaneWingAOE(BossModule module) : Components.GenericAOEs(module)
{
    public override bool KeepOnPhaseChange => true;

    public readonly List<AOEInstance> AOEs = new(4);

    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(9f), new AOEShapeDonut(9f, 16f), new AOEShapeDonut(16f, 23f), new AOEShapeDonut(23f, 30f)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Count != 0 ? [AOEs[0]] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = ShapeForAction(spell.Action);
        if (shape != null)
        {
            NumCasts = 0;
            AOEs.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell), ActorID: caster.InstanceID));
            AOEs.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var shape = ShapeForAction(spell.Action);
        if (shape != null)
        {
            for (var i = 0; i < AOEs.Count; ++i)
            {
                var aoe = AOEs[i];
                if (aoe.ActorID == caster.InstanceID)
                {
                    AOEs.Remove(aoe);
                    break;
                }
            }
            ++NumCasts;
        }
    }

    private static AOEShape? ShapeForAction(ActionID aid) => aid.ID switch
    {
        (uint)AID.HurricaneWingLongExpanding1 or (uint)AID.HurricaneWingShortExpanding1 or (uint)AID.HurricaneWingLongShrinking4 or (uint)AID.HurricaneWingShortShrinking4 => _shapes[0],
        (uint)AID.HurricaneWingLongExpanding2 or (uint)AID.HurricaneWingShortExpanding2 or (uint)AID.HurricaneWingLongShrinking3 or (uint)AID.HurricaneWingShortShrinking3 => _shapes[1],
        (uint)AID.HurricaneWingLongExpanding3 or (uint)AID.HurricaneWingShortExpanding3 or (uint)AID.HurricaneWingLongShrinking2 or (uint)AID.HurricaneWingShortShrinking2 => _shapes[2],
        (uint)AID.HurricaneWingLongExpanding4 or (uint)AID.HurricaneWingShortExpanding4 or (uint)AID.HurricaneWingLongShrinking1 or (uint)AID.HurricaneWingShortShrinking1 => _shapes[3],
        _ => null
    };
}

class GreatWhirlwindLarge(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GreatWhirlwindLarge), 10f)
{
    public override bool KeepOnPhaseChange => true;
}

class GreatWhirlwindSmall(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GreatWhirlwindSmall), 3f)
{
    public override bool KeepOnPhaseChange => true;
}

class Whirlwinds(BossModule module) : Components.GenericAOEs(module)
{
    public override bool KeepOnPhaseChange => true;

    private const float Length = 5f;
    private static readonly AOEShapeCapsule capsuleSmall = new(3f, Length), capsuleBig = new(9f, Length);
    private static readonly AOEShapeCircle circleSmall = new(3f), circleBig = new(9f);
    private readonly List<Actor> _smallWhirldwinds = new(3), _bigWhirldwinds = new(3);
    public bool Active => _smallWhirldwinds.Count != 0 || _bigWhirldwinds.Count != 0;
    private static readonly Angle a180 = 180f.Degrees();
    private bool moving;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var countSmall = _smallWhirldwinds.Count;
        var countBig = _bigWhirldwinds.Count;
        var total = countSmall + countBig;
        if (total == 0)
            return [];
        var aoes = new AOEInstance[total];
        for (var i = 0; i < countSmall; ++i)
        {
            var w = _smallWhirldwinds[i];
            aoes[i] = new(moving ? capsuleSmall : circleSmall, w.Position, w.Rotation);
        }
        for (var i = 0; i < countBig; ++i)
        {
            var w = _bigWhirldwinds[i];
            aoes[i + countSmall] = new(moving ? capsuleBig : circleBig, w.Position, w.Rotation);
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GreatWhirlwindLarge)
        {
            _bigWhirldwinds.Add(caster);
            moving = false;
        }
        else if (spell.Action.ID == (uint)AID.GreatWhirlwindSmall)
            _smallWhirldwinds.Add(caster);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.BitingWind && id == 0x1E3C)
            _smallWhirldwinds.Remove(actor);
        else if (actor.OID == (uint)OID.RavagingWind && id == 0x1E39)
            _bigWhirldwinds.Remove(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (!moving && spell.Action.ID is (uint)AID.GreatWhirlwindLargeAOE or (uint)AID.GreatWhirlwindSmallAOE)
            moving = true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var countSmall = _smallWhirldwinds.Count;
        var countBig = _bigWhirldwinds.Count;
        var total = countSmall + countBig;
        if (countSmall == 0 && countBig == 0)
            return;
        var forbidden = new Func<WPos, float>[total]; // merging all forbidden zones into one to make pathfinding less demanding

        const float length = Length + 6f;
        for (var i = 0; i < countBig; ++i)
        {
            var w = _bigWhirldwinds[i];
            forbidden[i] = ShapeDistance.Capsule(w.Position, !moving ? w.Rotation + a180 : w.Rotation, length, 10);
        }
        for (var i = 0; i < countSmall; ++i)
        {
            var w = _smallWhirldwinds[i];
            forbidden[i + countBig] = ShapeDistance.Capsule(w.Position, !moving ? w.Rotation + a180 : w.Rotation, length, 5);
        }

        hints.AddForbiddenZone(ShapeDistance.Union(forbidden), WorldState.FutureTime(1.1d));
    }
}

class HorridRoarPuddle(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HorridRoarPuddle), 4f)
{
    public override bool KeepOnPhaseChange => true;
}

class HorridRoarSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.HorridRoarSpread), 8f)
{
    public override bool KeepOnPhaseChange => true;
}
