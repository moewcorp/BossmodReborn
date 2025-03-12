namespace BossMod.Global.MaskedCarnivale.Stage01;

public enum OID : uint
{
    Boss = 0x25BE, //R=1.5
    Slime = 0x25BD //R=1.5
}

public enum AID : uint
{
    AutoAttack = 6499, // Slime->player, no cast, single-target
    AutoAttack2 = 6497, // Boss->player, no cast, single-target

    FluidSpread = 14198, // Slime->player, no cast, single-target
    IronJustice = 14199 // Boss->self, 2.5s cast, range 8+R 120-degree cone
}

class IronJustice(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.IronJustice), new AOEShapeCone(9.5f, 60f.Degrees()));

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("This stage is trivial.\nUse whatever skills you have to defeat these opponents.");
    }
}

class Stage01States : StateMachineBuilder
{
    public Stage01States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IronJustice>()
            .DeactivateOnEnter<Hints>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(Stage01.Trash);
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 610, NameID = 8077)]
public class Stage01 : BossModule
{
    public Stage01(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        ActivateComponent<Hints>();
    }

    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.Slime];

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
        Arena.Actors(Enemies((uint)OID.Slime));
    }
}
