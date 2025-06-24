﻿namespace BossMod.Dawntrail.Ultimate.FRU;

class P4AkhRhai(BossModule module) : Components.GenericAOEs(module, (uint)AID.AkhRhaiAOE)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeCircle _shape = new(4);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (AOEs.Count == 0)
        {
            // preposition for baits - note that this is very arbitrary...
            var off = 10f * 45f.Degrees().ToDirection();
            var p1 = ShapeDistance.Circle(Arena.Center + off, 1f);
            var p2 = ShapeDistance.Circle(Arena.Center - off, 1f);
            hints.AddForbiddenZone(p => -Math.Min(p1(p), p2(p)), DateTime.MaxValue);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AkhRhai)
            AOEs.Add(new(_shape, spell.LocXZ, default, Module.CastFinishAt(spell)));
    }
}
