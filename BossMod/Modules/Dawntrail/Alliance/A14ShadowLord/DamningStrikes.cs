namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

class DamningStrikes(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DamningStrikesImpact1 or (uint)AID.DamningStrikesImpact2 or (uint)AID.DamningStrikesImpact3)
        {
            Towers.Add(new(spell.LocXZ, 3f, 8, 8, default, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.DamningStrikesImpact1 or (uint)AID.DamningStrikesImpact2 or (uint)AID.DamningStrikesImpact3)
        {
            ++NumCasts;
            Towers.RemoveAll(t => t.Position == caster.Position);
            var forbidden = Raid.WithSlot(false, false, true).WhereActor(a => spell.Targets.Any(t => t.ID == a.InstanceID)).Mask();
            for (var i = 0; i < Towers.Count; ++i)
            {
                var tower = Towers[i];
                tower.ForbiddenSoakers |= forbidden;
                Towers[i] = tower;
            }
        }
    }
}
