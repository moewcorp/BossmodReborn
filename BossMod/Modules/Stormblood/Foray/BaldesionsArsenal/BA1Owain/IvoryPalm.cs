namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA1Owain;

class IvoryPalm(BossModule module) : Components.GenericGaze(module, inverted: true)
{
    public readonly List<(Actor target, Actor source)> Tethers = new(2);

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        var count = Tethers.Count;
        if (count == 0)
            return [];

        for (var i = 0; i < count; ++i)
        {
            var tether = Tethers[i];
            if (tether.target == actor && !tether.source.IsDead) // apparently tethers don't get removed immediately upon death
            {
                return [new(tether.source.Position)];
            }
        }
        return [];
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var eye in ActiveEyes(slot, actor))
        {
            if (HitByEye(actor, eye) != Inverted)
            {
                hints.Add("Face the hand to petrify it!");
                break;
            }
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        // only tethers in this fight are from this mechanic, so no need to check tether IDs
        Tethers.Add((WorldState.Actors.Find(tether.Target)!, source));
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        Tethers.Remove((WorldState.Actors.Find(tether.Target)!, source));
    }
}

class IvoryPalmExplosion(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.Explosion), "Ivory Palm is enraging!", true);

class EurekanAero(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.EurekanAero), new AOEShapeCone(6f, 60f.Degrees()), [(uint)OID.IvoryPalm])
{
    public override List<(Actor origin, Actor target, Angle angle)> OriginsAndTargets()
    {
        var count = Enemies.Count;
        List<(Actor, Actor, Angle)> origins = new(count);
        for (var i = 0; i < count; ++i)
        {
            var enemy = Enemies[i];
            if (enemy.IsDead || enemy.FindStatus((uint)SID.Petrification) != null)
                continue;

            var target = WorldState.Actors.Find(enemy.TargetID);
            if (target != null)
                origins.Add(new(OriginAtTarget ? target : enemy, target, Angle.FromDirection(target.Position - enemy.Position)));
        }
        return origins;
    }
}
