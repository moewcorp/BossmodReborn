namespace BossMod.Endwalker.Alliance.A33Oschon;

class P2WanderingShot(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.GreatWhirlwind))
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCircle _shape = new(23f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var coords = spell.Action.ID switch
        {
            (uint)AID.WanderingShotN or (uint)AID.WanderingVolleyN => WPos.ClampToGrid(new(0f, 740f)),
            (uint)AID.WanderingShotS or (uint)AID.WanderingVolleyS => WPos.ClampToGrid(new(0f, 760f)),
            _ => default
        };
        if (coords != default)
            _aoe = new(_shape, coords, default, Module.CastFinishAt(spell, 3.6f));
    }
}
