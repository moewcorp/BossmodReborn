namespace BossMod.Global.MaskedCarnivale.Stage11.Act1;

public enum OID : uint
{
    Boss = 0x2718, //R=1.2
}

public enum AID : uint
{
    Fulmination = 14583, // Boss->self, 23.0s cast, range 60 circle
}

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("These bombs start self-destruction on combat start. Pull them together\nwith Sticky Tongue and attack them with anything to interrupt them.\nThey are weak against wind and strong against fire.");
    }
}

sealed class Stage11Act1States : StateMachineBuilder
{
    public Stage11Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies((uint)OID.Boss);
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 621, NameID = 2280, SortOrder = 1)]
public sealed class Stage11Act1 : BossModule
{
    public Stage11Act1(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss));
    }
}
