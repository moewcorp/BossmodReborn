﻿namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

class Echoes(BossModule module) : Components.UniformStackSpread(module, 6, 0, 8)
{
    public int NumCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.EchoesAOE)
            ++NumCasts;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Echoes)
            AddStack(actor);
    }
}
