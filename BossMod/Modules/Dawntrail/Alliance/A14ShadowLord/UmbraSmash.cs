namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

class UmbraSmash(BossModule module) : Components.Exaflare(module, new AOEShapeRect(5f, 30f))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var linesCount = Lines.Count;
        if (linesCount == 0)
            return;

        var imminentAOEs = ImminentAOEs(linesCount);

        // use only imminent aoes for hints
        for (var i = 0; i < imminentAOEs.Length; ++i)
        {
            var aoe = imminentAOEs[i];
            hints.AddForbiddenZone(Shape, aoe.Item1, aoe.Item3, aoe.Item2);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.UmbraSmashAOE1:
            case (uint)AID.UmbraSmashAOE2:
            case (uint)AID.UmbraSmashAOE3:
            case (uint)AID.UmbraSmashAOE4:
            case (uint)AID.UmbraSmashAOEClone:
                var dir = spell.Rotation.ToDirection();
                var origin = caster.Position + 30f * dir;
                Lines.Add(new() { Next = origin, Advance = 5f * dir.OrthoL(), Rotation = spell.Rotation + 90f.Degrees(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 2.3f, ExplosionsLeft = 6, MaxShownExplosions = 2 });
                Lines.Add(new() { Next = origin, Advance = 5f * dir.OrthoR(), Rotation = spell.Rotation - 90f.Degrees(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 2.3f, ExplosionsLeft = 6, MaxShownExplosions = 2 });
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.UmbraSmashAOE1:
            case (uint)AID.UmbraSmashAOE2:
            case (uint)AID.UmbraSmashAOE3:
            case (uint)AID.UmbraSmashAOE4:
            case (uint)AID.UmbraSmashAOEClone:
                ++NumCasts;
                var origin = caster.Position + 30f * spell.Rotation.ToDirection();
                foreach (var l in Lines.Where(l => l.Next.AlmostEqual(origin, 1f)))
                {
                    l.Next = origin + l.Advance;
                    l.TimeToMove = 1;
                    l.NextExplosion = WorldState.FutureTime(l.TimeToMove);
                    --l.ExplosionsLeft;
                }
                break;
            case (uint)AID.UmbraWave:
                ++NumCasts;
                var index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1f));
                if (index == -1)
                {
                    ReportError($"Failed to find entry for {caster.InstanceID:X}");
                    return;
                }

                AdvanceLine(Lines[index], caster.Position);
                if (Lines[index].ExplosionsLeft == 0)
                    Lines.RemoveAt(index);
                break;
        }
    }
}
