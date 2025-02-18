namespace BossMod.Dawntrail.Unreal.Un1Byakko;

class UnrelentingAnguish(BossModule module) : Components.PersistentVoidzone(module, 2, m => m.Enemies(OID.AratamaForce).Where(z => !z.IsDead), 2);

class OminousWind(BossModule module) : BossComponent(module)
{
    public BitMask Targets;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Targets[slot] && Raid.WithSlot(false, true, true).IncludedInMask(Targets).InRadiusExcluding(actor, 6).Any())
            hints.Add("GTFO from other bubble!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => Targets[pcSlot] && Targets[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Targets[pcSlot])
            foreach (var (_, p) in Raid.WithSlot(false, true, true).IncludedInMask(Targets).Exclude(pc))
                Arena.AddCircle(p.Position, 6, Colors.Danger);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.OminousWind)
            Targets.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.OminousWind)
            Targets.Clear(Raid.FindSlot(actor.InstanceID));
    }
}

class GaleForce(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.Bombogenesis, ActionID.MakeSpell(AID.GaleForce), 8.1f, true);

class VacuumClaw(BossModule module) : Components.PersistentVoidzone(module, 12, m => m.Enemies(OID.VacuumClaw).Where(z => !z.IsDead));
class VacuumBlade(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.VacuumBlade));
