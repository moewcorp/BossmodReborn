namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

class GreatBallOfFire(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _smallFlames = module.Enemies(OID.RagingFlame);
    private readonly List<Actor> _bigFlames = module.Enemies(OID.ImmolatingFlame);
    private readonly DateTime _activation = module.WorldState.FutureTime(6.6f);

    private static readonly AOEShapeCircle _shapeSmall = new(10);
    private static readonly AOEShapeCircle _shapeBig = new(18);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (var i = 0; i < _smallFlames.Count; ++i)
        {
            var f = _smallFlames[i];
            yield return new(_shapeSmall, f.Position, new(), Module.CastFinishAt(f.CastInfo, 0, _activation));
        }
        for (var i = 0; i < _bigFlames.Count; ++i)
        {
            var f = _bigFlames[i];
            yield return new(_shapeBig, f.Position, new(), Module.CastFinishAt(f.CastInfo, 0, _activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BurnSmall or AID.BurnBig)
            ++NumCasts;
    }
}
