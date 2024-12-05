namespace BossMod.Dawntrail.Alliance.A11Prishe;

class Explosion(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(8);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count > 0)
        {
            for (var i = 0; i < count; ++i)
            {
                var aoe = _aoes[i];
                yield return (aoe.Activation - _aoes[0].Activation).TotalSeconds <= 1 ? aoe with { Color = Colors.Danger } : aoe with { Risky = false };
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Explosion)
            _aoes.Add(new(circle, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && (AID)spell.Action.ID == AID.Explosion)
            _aoes.RemoveAt(0);
    }
}
