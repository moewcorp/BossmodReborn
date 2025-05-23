namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN5TrinityAvowed;

class ElementalImpact(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeCircle circle = new(20f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ElementalImpactVisual1 or (uint)AID.ElementalImpactVisual2)
            _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 0.3f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ElementalImpact1 or (uint)AID.ElementalImpact2 or (uint)AID.ElementalImpact3 or (uint)AID.ElementalImpact4)
        {
            _aoes.Clear();
        }
    }
}

class ElementalImpactTemperature(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeCircle circle = new(22f);
    private readonly PlayerTemperatures _temps = module.FindComponent<PlayerTemperatures>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (slot is < 0 or > 23)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var aoe = ref aoes[i];
            var id = aoe.ActorID;
            if (id != default && id == _temps.Temperatures[slot])
            {
                aoe.Color = Colors.SafeFromAOE;
            }
            else
            {
                aoe.Color = default;
            }
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var temp = spell.Action.ID switch
        {
            (uint)AID.ChillBlast => 1u,
            (uint)AID.HeatedBlast => 3u,
            (uint)AID.FreezingBlast => 2u,
            (uint)AID.SearingBlast => 4u,
            _ => default
        };
        if (temp != default)
            _aoes.Add(new(circle, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), ActorID: temp));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ChillBlast:
            case (uint)AID.HeatedBlast:
            case (uint)AID.FreezingBlast:
            case (uint)AID.SearingBlast:
                _aoes.Clear();
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length;
        if (len == 0)
            return;
        var isInsideDanger = false;
        var isinsideCorrect = false;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(actor.Position))
            {
                if (aoe.Color == Colors.SafeFromAOE)
                {
                    isinsideCorrect = true;
                }
                else
                {
                    isInsideDanger = true;
                }
            }
        }

        if (isInsideDanger)
            hints.Add(WarningText);
        hints.Add("Get hit by correct AOE!", !isinsideCorrect);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length;
        if (len == 0)
            return;
        var forbiddenInverted = new List<Func<WPos, float>>(2);
        var act = aoes[0].Activation;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Color != Colors.SafeFromAOE)
            {
                hints.AddForbiddenZone(ShapeDistance.Circle(aoe.Origin, 22f), act);
            }
            else
            {
                forbiddenInverted.Add(ShapeDistance.InvertedCircle(aoe.Origin, 22f));
            }
        }
        hints.AddForbiddenZone(ShapeDistance.Intersection(forbiddenInverted), act);
    }
}
