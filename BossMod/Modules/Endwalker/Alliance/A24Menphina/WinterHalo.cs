namespace BossMod.Endwalker.Alliance.A24Menphina;

class WinterHalo(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeDonut _shape = new(10, 60);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.WinterHaloShortAOE or AID.WinterHaloLongMountedAOE or AID.WinterHaloLongDismountedAOE)
            _aoe = new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.WinterHaloShortAOE or AID.WinterHaloLongMountedAOE or AID.WinterHaloLongDismountedAOE)
        {
            ++NumCasts;
            _aoe = null;
        }
    }
}
