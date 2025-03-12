﻿namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN1TrinitySeeker;

class ActOfMercy(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.ActOfMercy))
{
    private readonly DateTime _activation = module.WorldState.FutureTime(7.6d); // from verdant path cast start
    private static readonly AOEShapeCross _shape = new(50f, 4f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return new AOEInstance[1] { new(_shape, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation, _activation) };
    }
}
