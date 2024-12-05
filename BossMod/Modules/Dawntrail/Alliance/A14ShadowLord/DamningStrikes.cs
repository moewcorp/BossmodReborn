namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

class DamningStrikes(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DamningStrikesImpact1 or AID.DamningStrikesImpact2 or AID.DamningStrikesImpact3)
        {
            Towers.Add(new(caster.Position, 3, 8, 8, default, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.DamningStrikesImpact1 or AID.DamningStrikesImpact2 or AID.DamningStrikesImpact3)
        {
            ++NumCasts;
            Towers.RemoveAll(t => t.Position == caster.Position);
            var forbidden = Raid.WithSlot().WhereActor(a => spell.Targets.Any(t => t.ID == a.InstanceID)).Mask();
            for (var i = 0; i < Towers.Count; ++i)
            {
                var tower = Towers[i];
                tower.ForbiddenSoakers |= forbidden;
                Towers[i] = tower;
            }
        }
    }
}
