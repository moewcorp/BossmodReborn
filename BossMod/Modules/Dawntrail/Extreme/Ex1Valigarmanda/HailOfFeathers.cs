namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

class HailOfFeathers(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(6);

    private static readonly AOEShapeCircle _shape = new(20f); // TODO: verify falloff

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.HailOfFeathersAOE1:
            case (uint)AID.HailOfFeathersAOE2:
            case (uint)AID.HailOfFeathersAOE3:
            case (uint)AID.HailOfFeathersAOE4:
            case (uint)AID.HailOfFeathersAOE5:
            case (uint)AID.HailOfFeathersAOE6:
                _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                if (_aoes.Count == 6)
                    _aoes.SortBy(x => x.Activation);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.HailOfFeathersAOE1:
            case (uint)AID.HailOfFeathersAOE2:
            case (uint)AID.HailOfFeathersAOE3:
            case (uint)AID.HailOfFeathersAOE4:
            case (uint)AID.HailOfFeathersAOE5:
            case (uint)AID.HailOfFeathersAOE6:
                ++NumCasts;
                if (_aoes.Count != 0)
                    _aoes.RemoveAt(0);
                break;
        }
    }
}

class FeatherOfRuin(BossModule module) : Components.Adds(module, (uint)OID.FeatherOfRuin);

class BlightedBolt : Components.GenericAOEs
{
    private readonly List<Actor> _targets = [];
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(8);

    public BlightedBolt(BossModule module) : base(module)
    {
        var platform = module.FindComponent<ThunderPlatform>();
        if (platform != null)
        {
            var party = module.Raid.WithSlot(true, true, true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                platform.RequireHint[i] = true;
                platform.RequireLevitating[i] = false;
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _targets.Count;
        if (count == 6)
            return [];
        var targetsspan = CollectionsMarshal.AsSpan(_targets);
        var aoes = new AOEInstance[count];
        var index = 0;
        var hasDead = false;

        for (var i = 0; i < count; ++i)
        {
            ref var span = ref targetsspan[i];
            if (span.IsDead)
                hasDead = true;
            else
                aoes[index++] = new(_shape, span.Position, default, _activation);
        }
        return hasDead ? aoes.AsSpan(0, index) : [];
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        var count = _targets.Count;

        if (count == 6)
        {
            for (var i = 0; i < 6; ++i)
            {
                if (_targets[i].IsDead)
                    return;
            }
            hints.Add("Kill a feather!");
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BlightedBoltAOE)
        {
            _targets.AddIfNonNull(WorldState.Actors.Find(spell.TargetID));
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BlightedBoltAOE)
        {
            ++NumCasts;
            var count = _targets.Count;
            var id = spell.TargetID;
            for (var i = 0; i < count; ++i)
            {
                if (_targets[i].InstanceID == id)
                {
                    _targets.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
