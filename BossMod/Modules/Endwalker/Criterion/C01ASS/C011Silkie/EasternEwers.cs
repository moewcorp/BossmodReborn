namespace BossMod.Endwalker.VariantCriterion.C01ASS.C011Silkie;

class EasternEwers(BossModule module) : Components.Exaflare(module, 4f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NBrimOver or (uint)AID.SBrimOver)
        {
            Lines.Add(new() { Next = spell.LocXZ, Advance = new(0, 5.1f), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 0.8f, ExplosionsLeft = 11, MaxShownExplosions = int.MaxValue });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NBrimOver or (uint)AID.SBrimOver or (uint)AID.NRinse or (uint)AID.SRinse)
        {
            var index = Lines.FindIndex(item => Math.Abs(item.Next.X - caster.Position.X) < 1f);
            if (index == -1)
            {
                ReportError($"Failed to find entry for {caster.InstanceID:X}");
                return;
            }

            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}
