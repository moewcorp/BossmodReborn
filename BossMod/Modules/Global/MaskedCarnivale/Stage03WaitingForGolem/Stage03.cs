namespace BossMod.Global.MaskedCarnivale.Stage03;

public enum OID : uint
{
    Boss = 0x25D4, //R=2.2
    Voidzone = 0x1E8FEA,
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    BoulderClap = 14363, // Boss->self, 3.0s cast, range 14 120-degree cone
    EarthenHeart = 14364, // Boss->location, 3.0s cast, range 6 circle
    Obliterate = 14365 // Boss->self, 6.0s cast, range 60 circle
}

class BoulderClap(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BoulderClap), new AOEShapeCone(14, 60.Degrees()));
class EarthenHeart(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, ActionID.MakeSpell(AID.EarthenHeart), m => m.Enemies(OID.Voidzone).Where(e => e.EventState != 7), 1.2f);
class Obliterate(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.Obliterate));

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} is weak against water based spells.\nFlying Sardine is recommended to interrupt raidwide.");
    }
}

class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} is weak against water based spells.\nEarth based spells are useless against {Module.PrimaryActor.Name}.");
    }
}

class Stage03States : StateMachineBuilder
{
    public Stage03States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BoulderClap>()
            .ActivateOnEnter<EarthenHeart>()
            .ActivateOnEnter<Obliterate>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 613, NameID = 8084)]
public class Stage03 : BossModule
{
    public Stage03(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        ActivateComponent<Hints>();
    }
}
