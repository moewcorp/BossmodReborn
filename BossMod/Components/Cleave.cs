namespace BossMod.Components;

// generic component for cleaving autoattacks; shows shape outline and warns when anyone other than main target is inside
// enemy OID == 0 means 'primary actor'
public class Cleave(BossModule module, ActionID aid, AOEShape shape, uint[]? enemyOID = null, bool activeForUntargetable = false, bool originAtTarget = false, bool activeWhileCasting = true) : CastCounter(module, aid)
{
    public readonly AOEShape Shape = shape;
    public readonly bool ActiveForUntargetable = activeForUntargetable;
    public readonly bool ActiveWhileCasting = activeWhileCasting;
    public readonly bool OriginAtTarget = originAtTarget;
    public DateTime NextExpected;
    public readonly List<Actor> Enemies = module.Enemies(enemyOID ?? [module.PrimaryActor.OID]);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (OriginsAndTargets().Any(e => e.target != actor && Shape.Check(WPos.ClampToGrid(actor.Position), e.origin.Position, e.angle)))
        {
            hints.Add("GTFO from cleave!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (OriginsAndTargets().Count == 0)
            return;

        foreach (var (origin, target, angle) in OriginsAndTargets())
            if (actor != target)
                hints.AddForbiddenZone(Shape, WPos.ClampToGrid(origin.Position), angle, NextExpected);
            else
                AddTargetSpecificHints(actor, origin, hints);
    }

    private void AddTargetSpecificHints(Actor actor, Actor source, AIHints hints)
    {
        foreach (var a in Raid.WithoutSlot().Exclude(actor))
            switch (Shape)
            {
                case AOEShapeCircle circle:
                    hints.AddForbiddenZone(circle, WPos.ClampToGrid(a.Position));
                    break;
                case AOEShapeCone cone:
                    hints.AddForbiddenZone(ShapeDistance.Cone(WPos.ClampToGrid(source.Position), 100f, source.AngleTo(a), cone.HalfAngle));
                    break;
                case AOEShapeRect rect:
                    hints.AddForbiddenZone(ShapeDistance.Cone(WPos.ClampToGrid(source.Position), 100f, source.AngleTo(a), Angle.Asin(rect.HalfWidth / (a.Position - source.Position).Length())));
                    break;
            }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var e in OriginsAndTargets())
        {
            Shape.Outline(Arena, WPos.ClampToGrid(e.origin.Position), e.angle);
        }
    }

    public virtual List<(Actor origin, Actor target, Angle angle)> OriginsAndTargets()
    {
        var count = Enemies.Count;
        List<(Actor, Actor, Angle)> origins = new(count);
        for (var i = 0; i < count; ++i)
        {
            var enemy = Enemies[i];
            if (enemy.IsDead)
                continue;

            if (!ActiveForUntargetable && !enemy.IsTargetable)
                continue;

            if (!ActiveWhileCasting && enemy.CastInfo != null)
                continue;

            var target = WorldState.Actors.Find(enemy.TargetID);
            if (target != null)
            {
                origins.Add(new(OriginAtTarget ? target : enemy, target, Angle.FromDirection(target.Position - enemy.Position)));
            }
        }
        return origins;
    }
}
