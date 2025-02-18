﻿namespace BossMod.Endwalker.Alliance.A11Byregot;

class Reproduce(BossModule module) : Components.Exaflare(module, 7)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.CloudToGroundFast or AID.CloudToGroundSlow)
        {
            var fast = (AID)spell.Action.ID == AID.CloudToGroundFast;
            Lines.Add(new() { Next = spell.LocXZ, Advance = new(-8.5f, 0), NextExplosion = Module.CastFinishAt(spell), TimeToMove = fast ? 0.6f : 1.4f, ExplosionsLeft = 6, MaxShownExplosions = fast ? 5 : 2 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CloudToGroundFast or AID.CloudToGroundSlow or AID.CloudToGroundFastAOE or AID.CloudToGroundSlowAOE)
        {
            ++NumCasts;

            var index = Lines.FindIndex(item => Math.Abs(item.Next.Z - caster.Position.Z) < 1);
            if (index == -1)
            {
                ReportError($"Failed to find entry for {caster.InstanceID:X}");
                return;
            }

            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].Next.X < Arena.Center.X - Arena.Bounds.Radius)
                Lines.RemoveAt(index);
        }
    }
}
