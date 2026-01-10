namespace BossMod.DawnTrail.Raid.M10NDaringDevils;
// Struggling with Cutback Blaze and Persistent puddles, needs sorting.
// Got the spread markers working, and the persistent cone AOE along with the Cleanse on Divers Dare.
// Still need to fix the Baitaway for Cutback Blaze. Currently it shows an errant cone and Boss front indicator and does not show them on targets/players correctly.
// Also need to fix the puddles for Alley Oop Inferno, they currently do not appear at all.
// Will keep working on this one.
// _aoes for Cutback Blaze Persistent - working, _puddles for Alley Oop Inferno puddles - not working.
// Alleyoop Maelstrom AOEs here, need tweaking.
sealed class CutbackBlaze(BossModule module) : Components.BaitAwayCast(module, (uint)AID.CutbackBlaze, ConeShape, centerAtTarget: true, damageType: AIHints.PredictedDamageType.None)
{
    internal static readonly AOEShapeCone ConeShape = new(40f, 22.5f.Degrees());
}
sealed class CutbackBlazePersistent(BossModule module) : Components.GenericAOEs(module, (uint)AID.CutbackBlaze1)
{
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    internal static readonly AOEShapeCone ConeShape = new(40f, 22.5f.Degrees());

    public override void OnEventCast(Actor actor, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CutbackBlaze1:
                _aoes.Add(new(CutbackBlazePersistent.ConeShape, actor.Position, actor.Rotation));
                break;
            case (uint)AID.DiversDare:
                _aoes.Clear();
                break;
        }
    }
}

sealed class AlleyOopInfernoSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AlleyOopInferno1, 5f);

sealed class AlleyOopInfernoPuddles(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _puddles = [];
    private static readonly AOEShapeCircle Shape = new(5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_puddles);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.AlleyOopMaelstrom or (uint)AID.AlleyOopMaelstrom2 or (uint)AID.AlleyOopInferno or (uint)AID.AlleyOopInferno1 && _puddles.Count != 0)
        {
            var activation = Module.CastFinishAt(spell);
            var puddles = CollectionsMarshal.AsSpan(_puddles);
            for (var i = 0; i < puddles.Length; ++i)
            {
                ref var aoe = ref puddles[i];
                aoe.Activation = activation;
                aoe.Color = Colors.Danger;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AlleyOopInferno1:
                _puddles.Add(new(Shape, spell.TargetXZ));
                break;
            case (uint)AID.DiversDare:
            case (uint)AID.AlleyOopMaelstrom:
            case (uint)AID.AlleyOopMaelstrom2:
                _puddles.Clear();
                break;
        }
    }
}

sealed class AlleyOopMaelstromAOEs(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone Shape = new(20f, 11.25f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.AlleyOopMaelstrom or (uint)AID.AlleyOopMaelstrom2)
            _aoes.Add(new(Shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AlleyOopMaelstrom or (uint)AID.AlleyOopMaelstrom2)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                if (aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
