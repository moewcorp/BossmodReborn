namespace BossMod.Global.MaskedCarnivale.Stage07.Act2;

public enum OID : uint
{
    Boss = 0x2705, //R=1.6
    Sprite = 0x2704, //R=0.8
}

public enum AID : uint
{
    Detonation = 14696, // Boss->self, no cast, range 6+R circle
    Blizzard = 14709, // Sprite->player, 1.0s cast, single-target
}

class SlimeExplosion(BossModule module) : Components.GenericStackSpread(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var p in Module.Enemies(OID.Boss).Where(x => !x.IsDead))
            Arena.AddCircle(p.Position, 7.6f, Colors.Danger);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var p in Module.Enemies(OID.Boss).Where(x => !x.IsDead))
            if (actor.Position.InCircle(p.Position, 7.5f))
                hints.Add("In slime explosion radius!");
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Pull or push the Lava Slimes to the Ice Sprites and then hit the slimes\nfrom a distance to set of the explosions.");
    }
}

class Stage07Act2States : StateMachineBuilder
{
    public Stage07Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .Raw.Update = () => module.Enemies(Stage07Act2.Trash).All(e => e.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 617, NameID = 8094, SortOrder = 2)]
public class Stage07Act2 : BossModule
{
    public Stage07Act2(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.Layout4Quads)
    {
        ActivateComponent<Hints>();
        ActivateComponent<SlimeExplosion>();
    }
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.Sprite];

    protected override bool CheckPull() => Enemies(Trash).Any(e => e.InCombat);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.Boss));
        Arena.Actors(Enemies(OID.Sprite));
    }
}
