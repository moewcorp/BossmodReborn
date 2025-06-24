﻿namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN6Queen;

class HeavensWrathAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavensWrathKnockback, new AOEShapeRect(60f, 5f));

// TODO: generalize
sealed class HeavensWrathKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> _sources = new(2);
    private static readonly AOEShapeRect _shape = new(60f, 50f, -5f);
    private static readonly Func<WPos, float> eastKB = ShapeDistance.InvertedTri(new WPos(-262f, -415f) + 15f * (-90f.Degrees()).ToDirection(),
    new RelTriangle(default, 20f * 150f.Degrees().ToDirection(), 20f * 30f.Degrees().ToDirection()));

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(_sources);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HeavensWrathKnockback)
        {
            var act = Module.CastFinishAt(spell);
            _sources.Add(new(caster.Position, 20f, act, _shape, spell.Rotation + 90f.Degrees(), Kind.DirForward));
            _sources.Add(new(caster.Position, 20f, act, _shape, spell.Rotation - 90f.Degrees(), Kind.DirForward));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HeavensWrathKnockback)
        {
            _sources.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_sources.Count != 0)
        {
            hints.AddForbiddenZone(eastKB, _sources[0].Activation);
        }
    }
}
