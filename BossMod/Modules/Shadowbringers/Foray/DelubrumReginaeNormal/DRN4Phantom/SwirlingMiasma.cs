namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN4Phantom;

class SwirlingMiasma(BossModule module) : Components.Exaflare(module, new AOEShapeDonut(5f, 19f))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SwirlingMiasmaFirst)
        {
            Lines.Add(new() { Next = spell.LocXZ, Advance = 6f * caster.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 1.6f, ExplosionsLeft = 8, MaxShownExplosions = 2 });
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SwirlingMiasmaFirst or (uint)AID.SwirlingMiasmaRest)
        {
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
        }
    }
}
