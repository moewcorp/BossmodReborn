namespace BossMod.Endwalker.Alliance.A12Rhalgr;

class HandOfTheDestroyer(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(90, 20);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.HandOfTheDestroyerWrathAOE or AID.HandOfTheDestroyerJudgmentAOE)
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.HandOfTheDestroyerWrathAOE or AID.HandOfTheDestroyerJudgmentAOE)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}

class BrokenWorld(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BrokenWorldAOE), 30); // TODO: determine falloff

// this is not an official mechanic name - it refers to broken world + hand of the destroyer combo, which creates multiple small aoes
class BrokenShards(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(9);

    private static readonly WPos[] _eastLocations = [new(-30.025f, 266.9f), new(-46.525f, 269.6f), new(-26.225f, 292.9f), new(-2.825f, 283.5f), new(-37.425f, 283.7f), new(1.575f, 271.5f), new(-18.825f, 278.8f), new(-12.325f, 298.3f), new(-34.125f, 250.5f)];
    private static readonly WPos[] _westLocations = [new(-6.925f, 268.0f), new(-0.175f, 285.0f), new(-25.625f, 298.5f), new(-34.225f, 283.5f), new(-11.625f, 293.5f), new(-46.125f, 270.5f), new(-18.125f, 279.0f), new(-40.325f, 290.5f), new(-2.125f, 252.0f)];
    private static readonly AOEShapeCircle _shape = new(20);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var locs = (AID)spell.Action.ID switch
        {
            AID.BrokenShardsE => _eastLocations,
            AID.BrokenShardsW => _westLocations,
            _ => null
        };
        if (locs != null)
            for (var i = 0; i < 9; ++i)
                _aoes.Add(new(_shape, locs[i], default, Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BrokenShardsAOE)
        {
            ++NumCasts;
            for (var i = 0; i < _aoes.Count; ++i)
            {
                var aoe = _aoes[i];
                if (aoe.Origin.AlmostEqual(caster.Position, 0.1f))
                {
                    _aoes.Remove(aoe);
                    break;
                }
            }
        }
    }
}

class LightningStorm(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.LightningStorm), 5);
