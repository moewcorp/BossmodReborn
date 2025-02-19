namespace BossMod.Endwalker.Alliance.A31Thaliak;

class RheognosisKnockback(BossModule module) : Components.Knockback(module)
{
    private Source? _knockback;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_knockback);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Rheognosis or (uint)AID.RheognosisPetrine)
            _knockback = new(Arena.Center, 25f, Module.CastFinishAt(spell, 20.3f), Direction: spell.Rotation + 180f.Degrees(), Kind: Kind.DirForward);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RheognosisKnockback)
        {
            _knockback = null;
            ++NumCasts;
        }
    }
}

public class RheognosisCrash : Components.Exaflare
{
    public RheognosisCrash(BossModule module) : base(module, new AOEShapeRect(10f, 12f)) => ImminentColor = Colors.AOE;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index <= 1 && state is 0x01000001 or 0x02000001)
        {
            var west = index == 0;
            var right = state == 0x01000001;
            var south = west == right;
            var start = Arena.Center + new WDir(west ? -Arena.Bounds.Radius : +Arena.Bounds.Radius, (south ? +Arena.Bounds.Radius : -Arena.Bounds.Radius) * 0.5f);
            var dir = (west ? 90f : -90f).Degrees();
            Lines.Add(new() { Next = start, Advance = 10f * dir.ToDirection(), Rotation = dir, NextExplosion = WorldState.FutureTime(4d), TimeToMove = 0.2f, ExplosionsLeft = 5, MaxShownExplosions = 5 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.RheognosisCrash)
        {
            ++NumCasts;
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
            ReportError($"Failed to find entry for {caster.InstanceID:X}");
        }
    }
}
