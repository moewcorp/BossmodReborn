namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA2Raiden;

class CloudToGround(BossModule module) : Components.Exaflare(module, 6)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CloudToGroundFirst)
        {
            var explosions = spell.LocXZ.InRect(Arena.Center, spell.Rotation, 35f, 35f, 15f) ? 8 : spell.LocXZ.InRect(Arena.Center, spell.Rotation, 35f, 35f, 20f) ? 6 : 4;
            Lines.Add(new() { Next = caster.Position, Advance = 8f * spell.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 1.1f, ExplosionsLeft = explosions, MaxShownExplosions = 10 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CloudToGroundFirst or (uint)AID.CloudToGroundRest)
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
