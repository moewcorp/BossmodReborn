namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V14ZelessGah;

sealed class BallOfFire(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeCircle circle = new(12f);
    private readonly List<Actor> cachedPortals = new(4);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    // various packages can appear in random order in the replays, so we have to carefully manage the information
    public override void OnActorCreated(Actor actor)
    {
        switch (actor.OID)
        {
            case (uint)OID.BallOfFire:
                var portals = Module.Enemies((uint)OID.Portal);
                var countP = portals.Count;
                for (var i = 0; i < countP; ++i)
                {
                    if ((int)portals[i].Rotation.Deg is 0 or 180) // actors with 0 or 180 degrees rot get a rotation icon later
                    {
                        return;
                    }
                }
                _aoes.Add(new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(countP == 0 ? 10.2d : 15.8f)));
                break;
            case (uint)OID.Portal:
                var rot = actor.Rotation.Round(90f);
                var count = _aoes.Count;
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                var pos = actor.Position;
                if ((int)rot.Deg is not 0 and not 180) // actors with 0 or 180 degrees rot get a rotation icon later
                {
                    var act = WorldState.FutureTime(12.5d);
                    var found = false;
                    for (var i = 0; i < count; ++i)
                    {
                        ref var aoe = ref aoes[i];
                        aoe.Activation = act;
                        if (aoe.Origin.AlmostEqual(pos, 11f))
                        {
                            aoe.Origin = new WPos(MathF.Round(aoe.Origin.X) == 284f ? 294f : 284f, pos.Z).Quantized();
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        cachedPortals.Add(actor);
                    }
                }
                else
                {
                    for (var i = 0; i < count; ++i)
                    {
                        ref var aoe = ref aoes[i];
                        if (aoe.Origin.AlmostEqual(pos, 11f))
                        {
                            _aoes.RemoveAt(i); // aoe was already created and should be removed due to later teleportation
                            return;
                        }
                    }
                }
                break;
        }
    }

    public override void Update()
    {
        var count = cachedPortals.Count;
        if (count == 0)
        {
            return;
        }
        var count2 = count - 1;
        var act = WorldState.FutureTime(12.5d);
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var len = aoes.Length;

        for (var i = count2; i >= 0; --i)
        {
            var pos = cachedPortals[i].Position;
            for (var j = 0; j < len; ++j)
            {
                ref var aoe = ref aoes[j];
                if (aoe.Origin.AlmostEqual(pos, 11f))
                {
                    aoe.Origin = new WPos(MathF.Round(aoe.Origin.X) == 284f ? 294f : 284f, pos.Z).Quantized();
                    aoe.Activation = act;
                    cachedPortals.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Burn:
                if (++NumCasts % 4 == 0) // sometimes the activation of an aoe is delays multiple frames, so we wait until all are done
                {
                    _aoes.Clear();
                }
                break;
            case (uint)AID.BlazingBenifice:
                cachedPortals.Clear();
                break;
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID != (uint)OID.Portal)
        {
            return;
        }
        var offset = state switch
        {
            0x00400080u => -90f,
            0x01000200u => 90f,
            _ => default
        };
        if (offset != default)
        {
            var count = _aoes.Count;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var act = WorldState.FutureTime(12.5d);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                aoe.Activation = act;
            }
            _aoes.Add(new(circle, (actor.Position + 10f * (actor.Rotation.Round(90f) + offset.Degrees()).ToDirection()).Quantized(), default, act));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.InfernGaleVisual) // if knockback happens during the mechanic, the AOEs will activate later
        {
            var count = _aoes.Count;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var act = WorldState.FutureTime(16.5d);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                aoe.Activation = act;
            }
        }
    }
}
