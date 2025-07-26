﻿namespace BossMod.Endwalker.Extreme.Ex7Zeromus;

class FlowOfTheAbyssDimensionalSurge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlowOfTheAbyssDimensionalSurge, new AOEShapeRect(60f, 7f));

class FlowOfTheAbyssSpreadStack(BossModule module) : Components.GenericStackSpread(module)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var act = WorldState.FutureTime(5d);
        switch (iconID)
        {
            case (uint)IconID.AkhRhai:
                Spreads.Add(new(actor, 5f, act));
                break;
            case (uint)IconID.DarkBeckonsUmbralRays:
                Stacks.Add(new(actor, 6f, 8, 8, act));
                break;
            case (uint)IconID.UmbralPrism:
                Stacks.Add(new(actor, 5f, 2, 2, act));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AkhRhaiStart or (uint)AID.UmbralRays or (uint)AID.UmbralPrism)
        {
            Spreads.Clear();
            Stacks.Clear();
        }
    }
}

class FlowOfTheAbyssAkhRhai(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AkhRhaiStart:
                _aoes.Add(new(_shape, caster.Position.Quantized()));
                break;
            case (uint)AID.AkhRhaiAOE:
                if (++NumCasts >= _aoes.Count * 10)
                    _aoes.Clear();
                break;
        }
    }
}

class ChasmicNails(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(5);

    private static readonly AOEShapeCone _shape = new(60f, 20f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 3 ? 3 : count;
        var aoes = new AOEInstance[max];

        for (var i = 0; i < max; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ChasmicNailsAOE1 or (uint)AID.ChasmicNailsAOE2 or (uint)AID.ChasmicNailsAOE3 or (uint)AID.ChasmicNailsAOE4 or (uint)AID.ChasmicNailsAOE5)
        {
            _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            if (_aoes.Count == 5)
                _aoes.Sort((x, y) => x.Activation.CompareTo(y.Activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ChasmicNailsAOE1 or (uint)AID.ChasmicNailsAOE2 or (uint)AID.ChasmicNailsAOE3 or (uint)AID.ChasmicNailsAOE4 or (uint)AID.ChasmicNailsAOE5)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}
