namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to 'single' and 'multi' fireplumes (normal or parts of gloryplume)
class Fireplume(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circleBig = new(15);
    private static readonly AOEShapeCircle circleSmall = new(10);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i < 2)
                yield return count > 1 ? aoe with { Color = Colors.Danger } : aoe;
            else if (i is > 1 and < 8)
                yield return aoe with { Risky = false };
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ExperimentalFireplumeSingleAOE:
            case AID.ExperimentalGloryplumeSingleAOE:
                _aoes.Add(new(circleBig, spell.LocXZ, default, Module.CastFinishAt(spell)));
                break;
            case AID.ExperimentalFireplumeMultiAOE:
            case AID.ExperimentalGloryplumeMultiAOE:
                if (_aoes.Count == 0)
                {
                    var startingDirection = Angle.FromDirection(caster.Position - Arena.Center);
                    for (var i = 0; i < 4; ++i)
                    {
                        var activation = Module.CastFinishAt(spell, i * 1);
                        var dir = 15 * (startingDirection - i * 45.Degrees()).ToDirection();
                        _aoes.Add(new(circleSmall, Arena.Center + dir, default, activation));
                        _aoes.Add(new(circleSmall, Arena.Center - dir, default, activation));
                    }
                    _aoes.Add(new(circleSmall, Arena.Center, default, Module.CastFinishAt(spell, 4)));
                }
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
            switch ((AID)spell.Action.ID)
            {
                case AID.ExperimentalFireplumeSingleAOE:
                case AID.ExperimentalGloryplumeSingleAOE:
                case AID.ExperimentalFireplumeMultiAOE:
                case AID.ExperimentalGloryplumeMultiAOE:
                    _aoes.RemoveAt(0);
                    break;
            }
    }
}
