namespace BossMod.Global.MaskedCarnivale.Stage08.Act1;

public enum OID : uint
{
    Boss = 0x2708, //R=0.6
    Bomb = 0x2709, //R=1.2
    Snoll = 0x270A, //R=0.9
}

public enum AID : uint
{
    SelfDestruct = 14687, // Boss->self, no cast, range 10 circle
    HypothermalCombustion = 14689, // Snoll->self, no cast, range 6 circle
    SelfDestruct2 = 14688, // Bomb->self, no cast, range 6 circle
}

class Selfdetonations(BossModule module) : BossComponent(module)
{
    private const string hint = "In bomb explosion radius!";
    private static readonly uint[] _bombs = [(uint)OID.Bomb, (uint)OID.Snoll];

    private static List<Actor> GetBombs(BossModule module)
    {
        var enemies = module.Enemies(_bombs);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var bombs = new List<Actor>(count);
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (!z.IsDead)
                bombs.Add(z);
        }
        return bombs;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!Module.PrimaryActor.IsDead)
            Arena.AddCircle(Module.PrimaryActor.Position, 10f);
        var bombs = GetBombs(Module);
        var count = bombs.Count;
        for (var i = 0; i < count; ++i)
            Arena.AddCircle(bombs[i].Position, 6f);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Module.PrimaryActor.IsDead && actor.Position.InCircle(Module.PrimaryActor.Position, 1f))
        {
            hints.Add(hint);
            return;
        }
        var bombs = GetBombs(Module);
        var count = bombs.Count;
        for (var i = 0; i < count; ++i)
            if (actor.Position.InCircle(bombs[i].Position, 6f))
            {
                hints.Add(hint);
                return;
            }
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("For this stage the spell Flying Sardine to interrupt the Progenitrix in Act 2\nis highly recommended. Hit the Cherry Bomb from a safe distance\nwith anything but fire damage to set of a chain reaction to win this act.");
    }
}

class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Hit the Cherry Bomb from a safe distance to win this act.");
    }
}

class Stage08Act1States : StateMachineBuilder
{
    public Stage08Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<Hints2>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(Stage08Act1.Trash);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    var enemy = enemies[i];
                    if (!enemy.IsDeadOrDestroyed)
                        return false;
                }
                return true;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 618, NameID = 8140, SortOrder = 1)]
public class Stage08Act1 : BossModule
{
    public Stage08Act1(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        ActivateComponent<Hints>();
        ActivateComponent<Selfdetonations>();
    }
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.Bomb, (uint)OID.Snoll];

    protected override bool CheckPull()
    {
        var enemies = Enemies(Trash);
        var count = enemies.Count;
        for (var i = 0; i < count; ++i)
        {
            var enemy = enemies[i];
            if (enemy.InCombat)
                return true;
        }
        return false;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Bomb));
        Arena.Actors(Enemies((uint)OID.Snoll));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Boss => 1,
                _ => 0
            };
        }
    }
}
