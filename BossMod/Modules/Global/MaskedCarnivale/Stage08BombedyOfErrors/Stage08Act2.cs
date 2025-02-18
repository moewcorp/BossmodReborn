namespace BossMod.Global.MaskedCarnivale.Stage08.Act2;

public enum OID : uint
{
    Boss = 0x270B, //R=3.75
    Bomb = 0x270C, //R=0.6
    Snoll = 0x270D //R=0.9
}

public enum AID : uint
{
    AutoAttack = 6499, // Bomb/Boss->player, no cast, single-target

    SelfDestruct = 14730, // Bomb->self, no cast, range 6 circle
    HypothermalCombustion = 14731, // Snoll->self, no cast, range 6 circle
    Sap = 14708, // Boss->location, 5.0s cast, range 8 circle
    Burst = 14680 // Boss->self, 6.0s cast, range 50 circle
}

class Sap(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Sap), 8);
class Burst(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.Burst));

class Selfdetonations(BossModule module) : BossComponent(module)
{
    private const string hint = "In bomb explosion radius!";

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var p in Module.Enemies(OID.Bomb).Where(x => !x.IsDead))
            Arena.AddCircle(p.Position, 10);
        foreach (var p in Module.Enemies(OID.Snoll).Where(x => !x.IsDead))
            Arena.AddCircle(p.Position, 6);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var p in Module.Enemies(OID.Bomb).Where(x => !x.IsDead))
            if (actor.Position.InCircle(p.Position, 10))
                hints.Add(hint);
        foreach (var p in Module.Enemies(OID.Snoll).Where(x => !x.IsDead))
            if (actor.Position.InCircle(p.Position, 6))
                hints.Add(hint);
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Clever activation of cherry bombs will freeze the Progenitrix.\nInterrupt its burst skill or wipe. The Progenitrix is weak to wind spells.");
    }
}

class Stage08Act2States : StateMachineBuilder
{
    public Stage08Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<Sap>()
            .Raw.Update = () => module.Enemies(Stage08Act2.Trash).All(e => e.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 618, NameID = 8098, SortOrder = 2)]
public class Stage08Act2 : BossModule
{
    public Stage08Act2(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.Layout2Corners)
    {
        ActivateComponent<Hints>();
        ActivateComponent<Selfdetonations>();
    }
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.Bomb, (uint)OID.Snoll];
    protected override bool CheckPull() => Enemies(Trash).Any(e => e.InCombat);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.Bomb));
        Arena.Actors(Enemies(OID.Snoll));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
