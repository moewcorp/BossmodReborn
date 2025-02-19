namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

class RuinationCross(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _aoeShape = new(20, 4, 20);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Ruination)
        {
            _aoes.Add(new(_aoeShape, caster.Position));
            _aoes.Add(new(_aoeShape, caster.Position, 90.Degrees()));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Ruination)
        {
            _aoes.Clear();
        }
    }
}

class RuinationExaflare(BossModule module) : Components.Exaflare(module, 4f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RuinationExaStart)
        {
            Lines.Add(new() { Next = caster.Position, Advance = 4f * spell.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 1.1f, ExplosionsLeft = 6, MaxShownExplosions = 6 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RuinationExaStart or (uint)AID.RuinationExaMove)
        {
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
