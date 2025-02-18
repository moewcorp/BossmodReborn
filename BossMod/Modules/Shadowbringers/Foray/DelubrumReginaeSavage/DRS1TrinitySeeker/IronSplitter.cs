namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker;

class IronSplitter(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.IronSplitter))
{
    private readonly List<AOEInstance> _aoes = new(3);
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(4), new AOEShapeDonut(8, 12), new AOEShapeDonut(16, 20),
    new AOEShapeDonut(4, 8), new AOEShapeDonut(12, 16), new AOEShapeDonut(20, 25)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var distance = (caster.Position - Arena.Center).Length();
            var activation = Module.CastFinishAt(spell);
            if (distance is < 3 or > 9 and < 11 or > 17 and < 19) // tiles
            {
                _aoes.Add(new(_shapes[0], Arena.Center, default, activation));
                _aoes.Add(new(_shapes[1], Arena.Center, default, activation));
                _aoes.Add(new(_shapes[2], Arena.Center, default, activation));
            }
            else
            {
                _aoes.Add(new(_shapes[3], Module.Center, default, activation));
                _aoes.Add(new(_shapes[4], Module.Center, default, activation));
                _aoes.Add(new(_shapes[5], Module.Center, default, activation));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _aoes.Clear();
        }
    }
}
