﻿namespace BossMod.Endwalker.Ultimate.DSW2;

// spreads
class P2StrengthOfTheWard1LightningStorm : Components.UniformStackSpread
{
    public P2StrengthOfTheWard1LightningStorm(BossModule module) : base(module, default, 5f)
    {
        AddSpreads(Raid.WithoutSlot(true, true, true));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LightningStormAOE)
            Spreads.Clear();
    }
}

// charges
class P2StrengthOfTheWard1SpiralThrust(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.SpiralThrust), "GTFO from charge aoe!")
{
    private readonly List<Actor> _knights = [];

    private static readonly AOEShapeRect _shape = new(52f, 8f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _knights.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var k = _knights[i];
            aoes[i] = new(_shape, k.Position, k.Rotation); // TODO: activation
        }
        return aoes;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E43 && actor.OID is (uint)OID.SerVellguine or (uint)OID.SerPaulecrain or (uint)OID.SerIgnasse)
            _knights.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            _knights.Remove(caster);
            ++NumCasts;
        }
    }
}

// rings
class P2StrengthOfTheWard1HeavyImpact(BossModule module) : HeavyImpact(module, 8.2f);
