namespace BossMod.Endwalker.Alliance.A33Oschon;

class P2ArrowTrail(BossModule module) : Components.Exaflare(module, new AOEShapeRect(10f, 5f))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ArrowTrailHint)
            Lines.Add(new() { Next = caster.Position, Advance = 5 * caster.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell, 0.4f), TimeToMove = 0.5f, ExplosionsLeft = 8, MaxShownExplosions = 3 });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ArrowTrailAOE)
        {
            ++NumCasts;
            var index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1f));
            if (index >= 0)
            {
                AdvanceLine(Lines[index], caster.Position);
                if (Lines[index].ExplosionsLeft == 0)
                    Lines.RemoveAt(index);
            }
        }
    }
}

class P2DownhillArrowTrailDownhill(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ArrowTrailDownhill), 6f);
