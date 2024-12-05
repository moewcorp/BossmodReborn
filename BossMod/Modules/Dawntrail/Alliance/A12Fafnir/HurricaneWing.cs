namespace BossMod.Dawntrail.Alliance.A12Fafnir;

class HurricaneWingRaidwide(BossModule module) : Components.CastCounterMulti(module, [ActionID.MakeSpell(AID.HurricaneWingRaidwideAOE1), ActionID.MakeSpell(AID.HurricaneWingRaidwideAOE2), ActionID.MakeSpell(AID.HurricaneWingRaidwideAOE3),
    ActionID.MakeSpell(AID.HurricaneWingRaidwideAOE4), ActionID.MakeSpell(AID.HurricaneWingRaidwideAOE5), ActionID.MakeSpell(AID.HurricaneWingRaidwideAOE6),
    ActionID.MakeSpell(AID.HurricaneWingRaidwideAOE7), ActionID.MakeSpell(AID.HurricaneWingRaidwideAOE8), ActionID.MakeSpell(AID.HurricaneWingRaidwideAOE9)]);

class HurricaneWingAOE(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(9), new AOEShapeDonut(9, 16), new AOEShapeDonut(16, 23), new AOEShapeDonut(23, 30)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = ShapeForAction(spell.Action);
        if (shape != null)
        {
            NumCasts = 0;
            AOEs.Add(new(shape, caster.Position, default, Module.CastFinishAt(spell)));
            AOEs.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var shape = ShapeForAction(spell.Action);
        if (shape != null)
        {
            AOEs.RemoveAll(aoe => aoe.Shape == shape);
            ++NumCasts;
        }
    }

    private static AOEShape? ShapeForAction(ActionID aid) => (AID)aid.ID switch
    {
        AID.HurricaneWingLongExpanding1 or AID.HurricaneWingShortExpanding1 or AID.HurricaneWingLongShrinking4 or AID.HurricaneWingShortShrinking4 => _shapes[0],
        AID.HurricaneWingLongExpanding2 or AID.HurricaneWingShortExpanding2 or AID.HurricaneWingLongShrinking3 or AID.HurricaneWingShortShrinking3 => _shapes[1],
        AID.HurricaneWingLongExpanding3 or AID.HurricaneWingShortExpanding3 or AID.HurricaneWingLongShrinking2 or AID.HurricaneWingShortShrinking2 => _shapes[2],
        AID.HurricaneWingLongExpanding4 or AID.HurricaneWingShortExpanding4 or AID.HurricaneWingLongShrinking1 or AID.HurricaneWingShortShrinking1 => _shapes[3],
        _ => null
    };
}

class GreatWhirlwindLarge(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GreatWhirlwindLarge), new AOEShapeCircle(10));
class GreatWhirlwindSmall(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GreatWhirlwindSmall), new AOEShapeCircle(3));

class Whirlwinds(BossModule module) : Components.GenericAOEs(module)
{
    private const int Length = 5;
    private static readonly AOEShapeCapsule capsuleSmall = new(3, Length), capsuleBig = new(9, Length);
    private static readonly AOEShapeCircle circleSmall = new(3), circleBig = new(9);
    private readonly List<Actor> _smallWhirldwinds = [], _bigWhirldwinds = [];
    public bool Active => _smallWhirldwinds.Count != 0 || _bigWhirldwinds.Count != 0;
    private static readonly Angle a180 = 180.Degrees();
    private bool moving;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var countSmall = _smallWhirldwinds.Count;
        var countBig = _bigWhirldwinds.Count;
        if (countSmall == 0 && countBig == 0)
            yield break;
        for (var i = 0; i < countSmall; ++i)
        {
            var w = _smallWhirldwinds[i];
            yield return new(moving ? capsuleSmall : circleSmall, w.Position, w.Rotation);
        }
        for (var i = 0; i < countBig; ++i)
        {
            var w = _bigWhirldwinds[i];
            yield return new(moving ? capsuleBig : circleBig, w.Position, w.Rotation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GreatWhirlwindLarge)
        {
            _bigWhirldwinds.Add(caster);
            moving = false;
        }
        else if ((AID)spell.Action.ID == AID.GreatWhirlwindSmall)
            _smallWhirldwinds.Add(caster);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.BitingWind && id == 0x1E3C)
            _smallWhirldwinds.Remove(actor);
        else if ((OID)actor.OID == OID.RavagingWind && id == 0x1E39)
            _bigWhirldwinds.Remove(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (!moving && (AID)spell.Action.ID is AID.GreatWhirlwindLargeAOE or AID.GreatWhirlwindSmallAOE)
            moving = true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var countSmall = _smallWhirldwinds.Count;
        var countBig = _bigWhirldwinds.Count;
        if (countSmall == 0 && countBig == 0)
            return;
        var forbidden = new List<Func<WPos, float>>(); // merging all forbidden zones into one to make pathfinding less demanding

        const int length = Length + 5;
        for (var i = 0; i < countBig; ++i)
        {
            var w = _bigWhirldwinds[i];
            forbidden.Add(ShapeDistance.Capsule(w.Position, !moving ? w.Rotation + a180 : w.Rotation, length, 10));
        }
        for (var i = 0; i < countSmall; ++i)
        {
            var w = _smallWhirldwinds[i];
            forbidden.Add(ShapeDistance.Capsule(w.Position, !moving ? w.Rotation + a180 : w.Rotation, length, 5));
        }
        hints.AddForbiddenZone(p => forbidden.Min(f => f(p)));
    }
}

class HorridRoarPuddle(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HorridRoarPuddle), 4);
class HorridRoarSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.HorridRoarSpread), 8);
