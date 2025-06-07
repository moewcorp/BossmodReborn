namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.DemonTablet;

sealed class DemonographTowers(BossModule module) : Components.CastTowersOpenWorld(module, (uint)AID.Explosion1, 4f, 4, 8, true);

// this is tricky to show in a useful way on radar since there is a grounded and floating tower in each position
sealed class GravityTowers(BossModule module) : Components.GenericTowersOpenWorld(module)
{
    private readonly HashSet<Actor> levitatingPlayers = [];
    private readonly HashSet<Actor> groundedPlayers = Soakers(module);

    public static HashSet<Actor> Soakers(BossModule module)
    {
        HashSet<Actor> actors = new(module.WorldState.Actors.Actors.Values.Count);
        foreach (var a in module.WorldState.Actors.Actors.Values)
            if (a.OID == default)
                actors.Add(a);
        return actors;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (spell.Action.ID is (uint)AID.Explosion2 or (uint)AID.Explosion3)
            Towers.Add(new(spell.LocXZ, 4f, 4, 8, id == (uint)AID.Explosion2 ? levitatingPlayers : groundedPlayers, activation: Module.CastFinishAt(spell), actorID: id));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Explosion2 or (uint)AID.Explosion3)
        {
            Towers.Clear();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.EraseGravity)
        {
            var targets = spell.Targets;
            var count = targets.Count;
            for (var i = 0; i < count; ++i)
            {
                var target = WorldState.Actors.Find(targets[i].ID)!;
                levitatingPlayers.Add(target);
                groundedPlayers.Remove(target);
            }
            // var towers = CollectionsMarshal.AsSpan(Towers);
            // var len = towers.Length;

            // for (var i = 0; i < len; ++i)
            // {
            //     ref var tower = ref towers[i];
            //     if (tower.ActorID == (uint)AID.Explosion2)
            //     {
            //         tower.AllowedSoakers = levitatingPlayers;
            //     }
            //     else
            //     {
            //         tower.AllowedSoakers = groundedPlayers;
            //     }
            // }
        }
    }
}
