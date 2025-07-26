﻿namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker;

sealed class IronSplitter(BossModule module) : Components.GenericAOEs(module, (uint)AID.IronSplitter)
{
    public readonly List<AOEInstance> AOEs = new(3);
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(4f), new AOEShapeDonut(8f, 12f), new AOEShapeDonut(16f, 20f),
    new AOEShapeDonut(4f, 8f), new AOEShapeDonut(12f, 16f), new AOEShapeDonut(20f, 25f)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var distance = (caster.Position - Arena.Center).Length();
            var activation = Module.CastFinishAt(spell);
            var pos = Arena.Center.Quantized();
            if (distance is < 3 or > 9 and < 11 or > 17 and < 19) // tiles
            {
                for (var i = 0; i < 3; ++i)
                    AddAOE(i);
            }
            else
            {
                for (var i = 3; i < 6; ++i)
                    AddAOE(i);
            }
            void AddAOE(int index) => AOEs.Add(new(_shapes[index], pos, default, activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            AOEs.Clear();
    }
}
