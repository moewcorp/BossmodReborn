namespace BossMod.Endwalker.Alliance.A21Nophica;

class SowingCircle(BossModule module) : Components.Exaflare(module, 5f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SowingCircleFirst)
        {
            Lines.Add(new() { Next = spell.LocXZ, Advance = 5f * caster.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 1, ExplosionsLeft = 10, MaxShownExplosions = 2 });
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SowingCircleFirst or (uint)AID.SowingCircleRest)
        {
            ++NumCasts;
            var count = Lines.Count;
            var pos = spell.LocXZ;
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
            ReportError($"Failed to find entry for {caster.InstanceID:X}");
        }
    }
}
