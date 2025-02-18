namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class SerpentsTide(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];
    private static readonly AOEShapeRect _shape = new(80f, 10f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SerpentsTide)
            AOEs.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SerpentsTideAOEPerykosNS or (uint)AID.SerpentsTideAOEPerykosEW or (uint)AID.SerpentsTideAOEThalaosNS or (uint)AID.SerpentsTideAOEThalaosEW)
        {
            AOEs.Clear();
            ++NumCasts;
        }
    }
}
