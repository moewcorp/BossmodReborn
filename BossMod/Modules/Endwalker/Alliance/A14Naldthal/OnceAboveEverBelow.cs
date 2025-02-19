namespace BossMod.Endwalker.Alliance.A14Naldthal;

class OnceAboveEverBelow(BossModule module) : Components.Exaflare(module, 6f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.EverfireFirst or (uint)AID.OnceBurnedFirst)
        {
            var pos = caster.Position;
            var advance = 6f * spell.Rotation.ToDirection();
            // lines are offset by 6/18/30; outer have 1 explosion only, mid have 4 or 5, inner 5
            var numExplosions = (pos - Arena.Center).LengthSq() > 500f ? 1 : 5;
            AddLine(advance);
            AddLine(-advance);

            void AddLine(WDir dir)
            => Lines.Add(new() { Next = pos, Advance = dir, NextExplosion = Module.CastFinishAt(spell), TimeToMove = 1.5f, ExplosionsLeft = numExplosions, MaxShownExplosions = 5 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.EverfireFirst:
            case (uint)AID.OnceBurnedFirst:
                var dir = caster.Rotation.ToDirection();
                Advance(dir);
                Advance(-dir);
                ++NumCasts;
                break;
            case (uint)AID.EverfireRest:
            case (uint)AID.OnceBurnedRest:
                Advance(caster.Rotation.ToDirection());
                ++NumCasts;
                break;
        }
        void Advance(WDir dir)
        {
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f) && line.Advance.Dot(dir) > 5f)
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
            ReportError($"Failed to find entry for {caster.InstanceID:X}, {pos} / {dir}");
        }
    }
}
