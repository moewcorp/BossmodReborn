namespace BossMod.Endwalker.Alliance.A34Eulogia;

class LovesLight(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(4);
    private static readonly AOEShapeRect _shape = new(80f, 12.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(AOEs)[..max];
        if (count > 1)
        {
            aoes[0].Color = Colors.Danger;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FirstBlush1 or (uint)AID.FirstBlush2 or (uint)AID.FirstBlush3 or (uint)AID.FirstBlush4)
        {
            AOEs.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            AOEs.Sort((a, b) => a.Activation.CompareTo(b.Activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FirstBlush1 or (uint)AID.FirstBlush2 or (uint)AID.FirstBlush3 or (uint)AID.FirstBlush4)
        {
            ++NumCasts;
            if (AOEs.Count > 0)
                AOEs.RemoveAt(0);
        }
    }
}
