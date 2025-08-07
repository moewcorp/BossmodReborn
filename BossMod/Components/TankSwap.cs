namespace BossMod.Components;

// generic tank-swap component for multi-hit tankbusters, with optional aoe
// assume that target of the first hit is locked when mechanic starts, then subsequent targets are selected based on who the boss targets
// TODO: this version assumes that boss cast and first-hit are potentially from different actors; the target lock could also be things like icons, etc - generalize more...
public class TankSwap(BossModule module, uint bossCast, uint firstCast, uint subsequentHit, double timeBetweenHits, AOEShape? shape = null, bool centerAtTarget = false) : GenericBaitAway(module, centerAtTarget: centerAtTarget, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public TankSwap(BossModule module, uint bossCast, uint firstCast, uint subsequentHit, double timeBetweenHits, float radius, bool centerAtTarget = true) : this(module, bossCast, firstCast, subsequentHit, timeBetweenHits, new AOEShapeCircle(radius), centerAtTarget) { }

    protected Actor? _source;
    protected ulong _prevTarget; // before first cast, this is the target of the first hit
    protected DateTime _activation;
    public readonly AOEShape? Shape = shape;

    public override void Update()
    {
        if (_source != null && Shape != null && WorldState.Actors.Find(NumCasts == 0 ? _prevTarget : _source.TargetID) is Actor t)
        {
            if (CurrentBaits.Count != 0)
            {
                CurrentBaits.Ref(0).Target = t;
            }
            else
            {
                CurrentBaits.Add(new(Module.PrimaryActor, t, Shape, _activation));
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_source?.TargetID == _prevTarget && actor.Role == Role.Tank)
        {
            hints.Add(_prevTarget != actor.InstanceID ? "Provoke!" : "Pass aggro!");
        }
        base.AddHints(slot, actor, hints);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == bossCast)
        {
            _source = caster;
        }
        else if (id == firstCast)
        {
            NumCasts = 0;
            _prevTarget = spell.TargetID;
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == firstCast || id == subsequentHit)
        {
            ++NumCasts;
            _prevTarget = spell.MainTargetID == caster.InstanceID && spell.Targets.Count != 0 ? spell.Targets.Ref(0).ID : spell.MainTargetID;
            _activation = Module.WorldState.FutureTime(timeBetweenHits);
        }
    }
}
