namespace BossMod.Dawntrail.Trial.T05Necron;

sealed class Prisons(BossModule module) : BossComponent(module)
{
    private BitMask activeTeleporters;

    private static readonly WPos[] prisonPositions = [new(100f, -100f), new(300f, -100f), new(300f, 100f), new(300f, 300f),
    new(100f, 300f), new(-100f, 300f), new(-100f, 100f), new(-100f, -100f)];

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index <= 0x07 && state == 0x00020001u)
        {
            activeTeleporters.Set(index);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (pc.PosRot.Y < -100f)
        {
            var playerPos = pc.Position;
            for (var i = 0; i < 8; ++i)
            {
                var pos = prisonPositions[i];
                if (playerPos.InSquare(pos, 50f))
                {
                    if (activeTeleporters[i])
                    {
                        var color = Colors.SafeFromAOE;
                        Arena.AddCircleFilled(pos + new WDir(default, -7.4f), 2f, color);
                        Arena.AddCircleFilled(pos + new WDir(-2.5f, -20f), 2f, color);
                        Arena.AddCircleFilled(pos + new WDir(15f, -11.5f), 2f, color);
                        Arena.AddCircleFilled(pos + new WDir(20f, default), 1.5f, color);
                    }
                    return;
                }
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (pc.PosRot.Y < -100f)
        {
            var playerPos = pc.Position;
            if (Arena.Bounds.Radius == 17.5f && playerPos.InSquare(Arena.Center, 50f)) // grand cross could happen while player is in prison
            {
                return;
            }
            for (var i = 0; i < 8; ++i)
            {
                var pos = prisonPositions[i];
                if (playerPos.InSquare(pos, 50f))
                {
                    var arena = new ArenaBoundsCustom([new Polygon(pos, 9.5f, 32), new Polygon(pos + new WDir(-5f, -21f), 4.5f, 32),
                        new Polygon(pos + new WDir(14f, -14f), 4.5f, 32), new Polygon(pos + new WDir(20f, default), 3.25f, 32)]);
                    Arena.Bounds = arena;
                    Arena.Center = arena.Center;
                    return;
                }
            }
        }
        else if (Arena.Bounds.Radius == 17.5f)
        {
            Arena.Bounds = new ArenaBoundsRect(18f, 15f);
            Arena.Center = Necron.ArenaCenter;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.PosRot.Y < -100f)
        {
            var playerPos = actor.Position;
            for (var i = 0; i < 8; ++i)
            {
                var pos = prisonPositions[i];
                if (playerPos.InSquare(pos, 50f))
                {
                    if (activeTeleporters[i])
                    {
                        hints.GoalZones.Add(hints.GoalSingleTarget(pos + new WDir(default, -7.4f), 1f, 9f));
                        hints.GoalZones.Add(hints.GoalSingleTarget(pos + new WDir(-2.5f, -20f), 1f, 9f));
                        hints.GoalZones.Add(hints.GoalSingleTarget(pos + new WDir(15f, -11.5f), 1f, 9f));
                        hints.GoalZones.Add(hints.GoalSingleTarget(pos + new WDir(20f, default), 1f, 9f));
                    }
                    return;
                }
            }
        }
    }
}
