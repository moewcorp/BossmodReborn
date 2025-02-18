namespace BossMod.Endwalker.Trial.T08Asura;

class ManyFaces(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(20, 90.Degrees());
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        Angle? rotation = (AID)spell.Action.ID switch
        {
            AID.TheFaceOfDelightA or AID.TheFaceOfWrathB => Angle.AnglesCardinals[2],
            AID.TheFaceOfDelightB => Angle.AnglesCardinals[1],
            AID.TheFaceOfWrathA => Angle.AnglesCardinals[3],
            _ => null
        };

        if (rotation != null)
            _aoe = new(cone, spell.LocXZ, rotation.Value, Module.CastFinishAt(spell, 0.1f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TheFaceOfDelightAOE or AID.TheFaceOfWrathAOE)
            _aoe = null;
    }
}
