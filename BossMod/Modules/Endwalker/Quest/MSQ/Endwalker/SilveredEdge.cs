namespace BossMod.Endwalker.Quest.MSQ.Endwalker;

class SilveredEdge(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    private bool active;
    private bool casting;
    private Angle _rotation;
    private static readonly Angle a120 = 120f.Degrees(), a240 = 240f.Degrees();
    private static readonly AOEShapeRect rect = new(40f, 3f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (active)
        {
            var pos = Module.PrimaryActor.Position;
            var aoes = new AOEInstance[3];
            if (casting)
            {
                var angle = Angle.FromDirection(actor.Position - pos);
                aoes[0] = new(rect, pos, angle, _activation);
                aoes[1] = new(rect, pos, angle + a120, _activation);
                aoes[2] = new(rect, pos, angle + a240, _activation);
            }
            else
            {
                aoes[0] = new(rect, pos, _rotation, _activation);
                aoes[1] = new(rect, pos, _rotation + a120, _activation);
                aoes[2] = new(rect, pos, _rotation + a240, _activation);
            }
            return aoes;
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SilveredEdge)
        {
            active = true;
            casting = true;
            _activation = Module.CastFinishAt(spell, 1.4f);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SilveredEdge)
        {
            casting = false;
            _rotation = Angle.FromDirection(Raid.Player()!.Position - caster.Position);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SilveredEdgeVisual)
        {
            if (++NumCasts == 3)
            {
                NumCasts = 0;
                active = false;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!casting)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}
