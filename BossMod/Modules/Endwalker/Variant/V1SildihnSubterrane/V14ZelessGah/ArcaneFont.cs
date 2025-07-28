namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V14ZelessGah;

sealed class ArcaneFont(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);
    private static readonly AOEShapeRect rect = new(100f, 5f);
    private readonly List<Actor> cachedPortals = new(4);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    // various packages can appear in random order in the replays, so we have to carefully manage the information
    public override void OnActorCreated(Actor actor)
    {
        switch (actor.OID)
        {
            case (uint)OID.ArcaneFont:
                var portals = Module.Enemies((uint)OID.Portal);
                var countP = portals.Count;
                var rot = actor.Rotation;
                _aoes.Add(new(rect, (actor.Position - 50f * rot.Round(90f).ToDirection()).Quantized(), rot, WorldState.FutureTime(countP == 0 ? 10.1d : 12.8f)));
                break;
            case (uint)OID.Portal:
                var count = _aoes.Count;
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                var pos = actor.Position;
                var act = WorldState.FutureTime(12.5d);
                var dir = actor.Rotation.Round(90f).ToDirection();
                var found = false;
                for (var i = 0; i < count; ++i)
                {
                    ref var aoe = ref aoes[i];
                    aoe.Activation = act;
                    if ((aoe.Origin + 50f * dir).AlmostEqual(pos, 11f))
                    {
                        aoe.Origin = (pos - 50f * dir + (aoe.Origin.AlmostEqual(pos, 1f) ? 10f : -10f) * dir.OrthoR()).Quantized();
                        found = true;
                    }
                }
                if (!found)
                {
                    cachedPortals.Add(actor);
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
            var p = cachedPortals[i];
            var rot = p.Rotation;
            var pos = p.Position;
            var dir = rot.Round(90f).ToDirection();
            for (var j = 0; j < len; ++j)
            {
                ref var aoe = ref aoes[j];
                if ((aoe.Origin + 50f * dir).AlmostEqual(pos, 11f))
                {
                    aoe.Origin = (pos + (aoe.Origin.AlmostEqual(pos, 1f) ? 10f : -10f) * dir.OrthoR()).Quantized();
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
                cachedPortals.Clear();
                break;
            case (uint)AID.BlazingBenifice:
                if (++NumCasts == _aoes.Count) // sometimes the activation of an aoe is delays multiple frames, so we wait until all are done
                {
                    NumCasts = 0;
                    _aoes.Clear();
                }
                break;
        }
    }
}
