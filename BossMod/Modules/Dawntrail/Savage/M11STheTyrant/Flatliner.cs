namespace BossMod.Dawntrail.Savage.M11STheTyrant;

sealed class Flatliner(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect Shape = new(40f, 20f);
    private readonly List<AOEInstance> _aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo cast)
    {
        if (cast.Action.ID is (uint)AID.Flatliner or (uint)AID.Flatliner1)
        {
            _aoes.Add(new(
                Shape,
                caster.Position,
                caster.Rotation,
                Module.CastFinishAt(cast, 0.1f)
            ));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Flatliner or (uint)AID.Flatliner1)
        {
            Arena.Bounds = ArenaChanges.SplitArena;
            _aoes.Clear();
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => CollectionsMarshal.AsSpan(_aoes);
}
