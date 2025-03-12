﻿namespace BossMod.Endwalker.Savage.P6SHegemone;

class ChorosIxou(BossModule module) : Components.GenericAOEs(module)
{
    public bool FirstDone;
    public bool SecondDone;
    private readonly AOEShapeCone _cone = new(40f, 45f.Degrees());
    private readonly List<Angle> _directions = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (SecondDone)
            return [];

        // TODO: timing
        var offset = (FirstDone ? 90f : 0f).Degrees();
        var count = _directions.Count;
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
            aoes[i] = new(_cone, Module.PrimaryActor.Position, _directions[i] + offset);
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ChorosIxouFSFrontAOE or (uint)AID.ChorosIxouSFSidesAOE)
            _directions.Add(caster.Rotation);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ChorosIxouFSFrontAOE or (uint)AID.ChorosIxouSFSidesAOE)
            FirstDone = true;
        else if (spell.Action.ID is (uint)AID.ChorosIxouFSSidesAOE or (uint)AID.ChorosIxouSFFrontAOE)
            SecondDone = true;
    }
}
